using Identity.Application.Contracts.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Maintenance
{
    /// <summary> Periodically deletes expired or revoked sessions, used or expired password tokens and delivered outbox messages. </summary>
    internal sealed class RetentionPurgeService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RetentionOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<RetentionPurgeService> _logger;

        public RetentionPurgeService(
            IServiceScopeFactory scopeFactory, IOptions<RetentionOptions> options,
            TimeProvider timeProvider, ILogger<RetentionPurgeService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes), _timeProvider);

            // First run after one interval, not at startup, to keep startup free of database work
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var cutoff = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-_options.RetentionDays);

                    using var scope = _scopeFactory.CreateScope();
                    var result = await scope.ServiceProvider.GetRequiredService<IIdentityDataRetention>()
                        .PurgeAsync(cutoff, stoppingToken);

                    _logger.LogInformation(
                        "Retention purge removed {Sessions} sessions, {PasswordTokens} password tokens and {OutboxMessages} outbox messages",
                        result.Sessions, result.PasswordTokens, result.OutboxMessages);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogWarning(exception, "Retention purge failed; it will run again on the next interval");
                }
            }
        }
    }
}
