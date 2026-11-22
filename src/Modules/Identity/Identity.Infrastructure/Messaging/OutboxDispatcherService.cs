using Identity.Application.Contracts.Messaging;
using Identity.Application.Messaging;
using Identity.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Messaging;

namespace Identity.Infrastructure.Messaging
{
    /// <summary>
    /// Publishes the pending outbox messages to the message broker. Safe with several instances: each message is claimed
    /// with a conditional update before it is published. A message is marked as processed only after the broker confirmed it,
    /// so with the broker down the messages stay pending and go out when it comes back. Never blocks startup: if the
    /// database or the broker is down it logs and retries.
    /// </summary>
    internal sealed class OutboxDispatcherService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMessagePublisher _publisher;
        private readonly OutboxOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<OutboxDispatcherService> _logger;

        public OutboxDispatcherService(
            IServiceScopeFactory scopeFactory, IMessagePublisher publisher, IOptions<OutboxOptions> options,
            TimeProvider timeProvider, ILogger<OutboxDispatcherService> logger)
        {
            _scopeFactory = scopeFactory;
            _publisher = publisher;
            _options = options.Value;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), _timeProvider);

            do
            {
                try
                {
                    await DispatchBatchAsync(stoppingToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogWarning(exception, "Outbox batch failed; it will be retried on the next tick");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task DispatchBatchAsync(CancellationToken cancellationToken)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;

            using var scope = _scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();

            var messages = await store.ClaimPendingAsync(
                _options.BatchSize, _options.MaxAttempts, now, now.AddSeconds(_options.LockSeconds), cancellationToken);

            foreach (var message in messages)
            {
                var error = await PublishAsync(message, cancellationToken);

                if (error is null)
                {
                    await store.MarkProcessedAsync(message.Id, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
                    _logger.LogInformation("Published outbox message {MessageId} ({MessageType})", message.Id, message.Type);
                }
                else
                {
                    await store.MarkFailedAsync(message.Id, error, cancellationToken);
                    _logger.LogWarning("Outbox message {MessageId} ({MessageType}) failed on attempt {Attempt}: {Error}",
                        message.Id, message.Type, message.Attempts + 1, error);
                }
            }
        }

        /// <returns> null when the broker accepted the message; otherwise the error to record on it. </returns>
        private async Task<string?> PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
        {
            var envelope = new MessageEnvelope(
                message.Id, message.Type, IdentityMessaging.MessageVersion, message.OccurredAt, message.CorrelationId, message.Payload);

            try
            {
                await _publisher.PublishAsync(IdentityMessaging.Exchange, envelope, cancellationToken);
                return null;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Type and message only: no stack trace in the table, and nothing here puts secrets in exception messages
                return $"{exception.GetType().Name}: {exception.Message}";
            }
        }
    }
}
