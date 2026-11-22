using System.Globalization;
using System.Text;
using RabbitMQ.Client;

namespace Shared.Messaging.RabbitMq
{
    /// <summary>
    /// Publishes persistent messages with publisher confirms: the call completes only after the broker accepted the
    /// message, and throws if it did not. A single confirm channel is shared; publishes are serialized.
    /// </summary>
    internal sealed class RabbitMqMessagePublisher : IMessagePublisher, IAsyncDisposable
    {
        public const string VersionHeader = "x-version";
        public const string OccurredAtHeader = "x-occurred-at";
        public const string AttemptHeader = "x-attempt";
        public const string ErrorHeader = "x-error";

        private readonly RabbitMqConnectionProvider _connections;
        private readonly RabbitMqTopology _topology;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private IChannel? _channel;

        public RabbitMqMessagePublisher(RabbitMqConnectionProvider connections, RabbitMqTopology topology)
        {
            _connections = connections;
            _topology = topology;
        }

        public async Task PublishAsync(string exchange, MessageEnvelope message, CancellationToken cancellationToken)
        {
            await _topology.EnsureDeclaredAsync(exchange, cancellationToken);

            var properties = new BasicProperties
            {
                MessageId = message.MessageId.ToString(),
                Type = message.Type,
                CorrelationId = message.CorrelationId?.ToString(),
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Headers = new Dictionary<string, object?>
                {
                    [VersionHeader] = message.Version,
                    [OccurredAtHeader] = message.OccurredAtUtc.ToString("O", CultureInfo.InvariantCulture)
                }
            };

            await PublishAsync(exchange, message.Type, properties, Encoding.UTF8.GetBytes(message.Payload), cancellationToken);
        }

        /// <summary> Sends a copy of a message to a named queue through the default exchange (retry and dead-letter queues). </summary>
        public Task PublishToQueueAsync(string queue, BasicProperties properties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken) =>
            PublishAsync("", queue, properties, body, cancellationToken);

        private async Task PublishAsync(
            string exchange, string routingKey, BasicProperties properties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
        {
            await _gate.WaitAsync(cancellationToken);
            try
            {
                var channel = await GetChannelAsync(cancellationToken);

                // mandatory = false on purpose: some events have no receiver yet (ItemBelowMinimum) and must not fail the outbox
                await channel.BasicPublishAsync(exchange, routingKey, mandatory: false, basicProperties: properties, body: body, cancellationToken: cancellationToken);
            }
            finally
            {
                _gate.Release();
            }
        }

        private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
        {
            if (_channel is { IsOpen: true })
                return _channel;

            if (_channel is not null)
                await _channel.DisposeAsync();

            var connection = await _connections.GetConnectionAsync(cancellationToken);
            _channel = await connection.CreateChannelAsync(
                new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
                cancellationToken);
            return _channel;
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
                await _channel.DisposeAsync();

            _gate.Dispose();
        }
    }
}
