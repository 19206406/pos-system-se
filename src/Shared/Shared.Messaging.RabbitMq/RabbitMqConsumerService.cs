using System.Globalization;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared.Messaging.RabbitMq
{
    /// <summary>
    /// Reads the queue of every registered subscription with manual acknowledgement. A message is acknowledged only after
    /// its handler succeeded, or after the failed message was safely copied to the retry or dead-letter queue.
    /// Never blocks startup: if the broker is down it logs and tries again.
    /// </summary>
    internal sealed class RabbitMqConsumerService : BackgroundService
    {
        private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);
        private const int ErrorMaxLength = 500;

        private readonly RabbitMqConnectionProvider _connections;
        private readonly RabbitMqTopology _topology;
        private readonly RabbitMqMessagePublisher _publisher;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqConsumerService> _logger;

        public RabbitMqConsumerService(
            RabbitMqConnectionProvider connections, RabbitMqTopology topology, RabbitMqMessagePublisher publisher,
            IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> options, ILogger<RabbitMqConsumerService> logger)
        {
            _connections = connections;
            _topology = topology;
            _publisher = publisher;
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_topology.Subscriptions.Count == 0)
                return;

            IChannel? channel = null;

            while (channel is null && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    channel = await StartConsumersAsync(stoppingToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogWarning(exception, "RabbitMQ is not reachable; consumers will retry in {Seconds}s", ReconnectDelay.TotalSeconds);
                    await DelayAsync(ReconnectDelay, stoppingToken);
                }
            }

            if (channel is null)
                return;

            // Automatic recovery restores the connection, the channel and the consumers; there is nothing else to do but wait
            await using (channel)
                await DelayAsync(Timeout.InfiniteTimeSpan, stoppingToken);
        }

        private static async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Shutting down
            }
        }

        private async Task<IChannel> StartConsumersAsync(CancellationToken cancellationToken)
        {
            await _topology.EnsureDeclaredAsync(null, cancellationToken);

            var connection = await _connections.GetConnectionAsync(cancellationToken);
            var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await channel.BasicQosAsync(0, (ushort)_options.PrefetchCount, false, cancellationToken);

            foreach (var subscription in _topology.Subscriptions)
            {
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += (_, delivery) => OnReceivedAsync(channel, subscription, delivery, cancellationToken);
                await channel.BasicConsumeAsync(subscription.Queue, autoAck: false, consumer, cancellationToken);
            }

            _logger.LogInformation("Consuming {Count} RabbitMQ queue(s)", _topology.Subscriptions.Count);
            return channel;
        }

        private async Task OnReceivedAsync(
            IChannel channel, MessageSubscription subscription, BasicDeliverEventArgs delivery, CancellationToken cancellationToken)
        {
            var properties = delivery.BasicProperties;
            var attempt = ReadAttempt(properties);

            try
            {
                var envelope = ToEnvelope(properties, delivery.Body);

                using var scope = _scopeFactory.CreateScope();
                await subscription.Handle(scope.ServiceProvider, envelope, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Shutting down: no ack, the broker redelivers the message to the next consumer
                return;
            }
            catch (Exception exception)
            {
                // Type and message only: the error travels in a header and handlers never put secrets in exception messages
                var error = $"{exception.GetType().Name}: {exception.Message}";
                var type = properties.Type ?? "unknown";

                try
                {
                    await RetryOrDeadLetterAsync(subscription, delivery, attempt, error, cancellationToken);

                    // Acknowledge the original only now: the broker confirmed the copy in the retry or dead-letter queue
                    await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken);
                    _logger.LogWarning("Message {MessageId} ({MessageType}) failed on delivery {Attempt} in {Queue}: {Error}",
                        properties.MessageId, type, attempt + 1, subscription.Queue, error);
                }
                catch (Exception republishException) when (republishException is not OperationCanceledException)
                {
                    // The copy was not confirmed: do not acknowledge, so the broker keeps the original and redelivers it
                    _logger.LogError(republishException, "Could not move message {MessageId} to the retry queue; it stays in {Queue}",
                        properties.MessageId, subscription.Queue);
                    await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: true, cancellationToken);
                }

                return;
            }

            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken);
        }

        private async Task RetryOrDeadLetterAsync(
            MessageSubscription subscription, BasicDeliverEventArgs delivery, int attempt, string error, CancellationToken cancellationToken)
        {
            var nextAttempt = attempt + 1;
            var exhausted = nextAttempt >= _options.MaxDeliveryAttempts;

            var headers = new Dictionary<string, object?>();
            if (delivery.BasicProperties.Headers is not null)
                foreach (var (key, value) in delivery.BasicProperties.Headers)
                    headers[key] = value;

            headers[RabbitMqMessagePublisher.AttemptHeader] = nextAttempt;
            headers[RabbitMqMessagePublisher.ErrorHeader] = error.Length > ErrorMaxLength ? error[..ErrorMaxLength] : error;

            var copy = new BasicProperties
            {
                MessageId = delivery.BasicProperties.MessageId,
                Type = delivery.BasicProperties.Type,
                CorrelationId = delivery.BasicProperties.CorrelationId,
                ContentType = delivery.BasicProperties.ContentType,
                DeliveryMode = DeliveryModes.Persistent,
                Headers = headers
            };

            var target = exhausted ? RabbitMqTopology.DeadLetterQueue(subscription.Queue) : RabbitMqTopology.RetryQueue(subscription.Queue);
            await _publisher.PublishToQueueAsync(target, copy, delivery.Body, cancellationToken);

            if (exhausted)
                _logger.LogError("Message {MessageId} moved to {Queue} after {Attempts} deliveries", delivery.BasicProperties.MessageId, target, nextAttempt);
        }

        private static int ReadAttempt(IReadOnlyBasicProperties properties) =>
            properties.Headers is not null
            && properties.Headers.TryGetValue(RabbitMqMessagePublisher.AttemptHeader, out var value)
            && value is int attempt
                ? attempt
                : 0;

        private static MessageEnvelope ToEnvelope(IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            if (!Guid.TryParse(properties.MessageId, out var messageId) || string.IsNullOrWhiteSpace(properties.Type))
                throw new InvalidOperationException("The message has no valid message id or type.");

            var version = properties.Headers is not null
                && properties.Headers.TryGetValue(RabbitMqMessagePublisher.VersionHeader, out var versionValue)
                && versionValue is int parsedVersion
                    ? parsedVersion
                    : 1;

            if (properties.Headers is null
                || !properties.Headers.TryGetValue(RabbitMqMessagePublisher.OccurredAtHeader, out var occurredValue)
                || occurredValue is not byte[] occurredBytes
                || !DateTime.TryParse(Encoding.UTF8.GetString(occurredBytes), CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var occurredAt))
            {
                throw new InvalidOperationException("The message has no valid occurred-at header.");
            }

            Guid? correlationId = Guid.TryParse(properties.CorrelationId, out var parsedCorrelation) ? parsedCorrelation : null;

            return new MessageEnvelope(messageId, properties.Type, version, occurredAt, correlationId, Encoding.UTF8.GetString(body.Span));
        }
    }
}
