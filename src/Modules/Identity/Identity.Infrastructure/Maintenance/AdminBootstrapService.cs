using Identity.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Maintenance
{
    /// <summary>
    /// Runs AdminBootstrapper once, in the background so startup never waits for the database.
    /// Retries a few times in case the database or the migrations are not ready yet.
    /// </summary>
    internal sealed class AdminBootstrapService : BackgroundService
    {
        private const int MaxAttempts = 5;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly BootstrapOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<AdminBootstrapService> _logger;

        public AdminBootstrapService(
            IServiceScopeFactory scopeFactory, IOptions<BootstrapOptions> options,
            TimeProvider timeProvider, ILogger<AdminBootstrapService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(_options.AdminEmail))
                return;

            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var created = await scope.ServiceProvider.GetRequiredService<AdminBootstrapper>()
                        .EnsureFirstAdminAsync(_options.AdminEmail, _options.AdminFullName, stoppingToken);

                    if (created)
                        _logger.LogInformation("Admin role assigned to the bootstrap user; an invite email was queued if it had no password");

                    return;
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogWarning(exception, "Admin bootstrap attempt {Attempt} of {MaxAttempts} failed", attempt, MaxAttempts);
                    await Task.Delay(RetryDelay, _timeProvider, stoppingToken);
                }
            }
        }
    }
}
