using Identity.Application.Contracts.Messaging;
using Identity.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Messaging
{
    /// <summary>
    /// Polls the outbox and delivers pending messages. Safe with several instances: each message is claimed
    /// with a conditional update before delivery. Never blocks startup: if the database is down it logs and retries.
    /// </summary>
    internal sealed class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly OutboxOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<OutboxProcessorService> _logger;

        public OutboxProcessorService(
            IServiceScopeFactory scopeFactory, IOptions<OutboxOptions> options,
            TimeProvider timeProvider, ILogger<OutboxProcessorService> logger)
        {
            _scopeFactory = scopeFactory;
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
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogWarning(exception, "Outbox batch failed; it will be retried on the next tick");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task ProcessBatchAsync(CancellationToken cancellationToken)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;

            IReadOnlyList<Domain.Entities.OutboxMessage> messages;
            using (var claimScope = _scopeFactory.CreateScope())
            {
                messages = await claimScope.ServiceProvider.GetRequiredService<IOutboxStore>().ClaimPendingAsync(
                    _options.BatchSize, _options.MaxAttempts, now, now.AddSeconds(_options.LockSeconds), cancellationToken);
            }

            foreach (var message in messages)
            {
                // A fresh scope (and DbContext) per message, so one failure leaves nothing behind for the next
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessor>();

                var error = await processor.ProcessAsync(message, cancellationToken);

                if (error is null)
                    _logger.LogInformation("Delivered outbox message {MessageId} ({MessageType})", message.Id, message.Type);
                else
                    _logger.LogWarning("Outbox message {MessageId} ({MessageType}) failed on attempt {Attempt}: {Error}",
                        message.Id, message.Type, message.Attempts + 1, error);
            }
        }
    }
}
