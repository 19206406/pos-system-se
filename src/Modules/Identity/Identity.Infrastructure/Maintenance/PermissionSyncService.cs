using Identity.Application.Authorization;
using Identity.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Maintenance
{
    /// <summary>
    /// Runs PermissionCatalogSynchronizer once at startup, in the background so startup never waits for the database.
    /// Retries a few times in case the database or the migrations are not ready yet.
    /// </summary>
    internal sealed class PermissionSyncService : BackgroundService
    {
        private const int MaxAttempts = 5;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

        private readonly PermissionCatalog _catalog;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<PermissionSyncService> _logger;

        /// <param name="catalog">
        /// Injected here, and not only inside the synchronizer, so it is built while the host starts: an invalid catalog
        /// (a duplicate code, a bad format) stops the application at startup instead of failing on the first request.
        /// </param>
        public PermissionSyncService(
            PermissionCatalog catalog, IServiceScopeFactory scopeFactory,
            TimeProvider timeProvider, ILogger<PermissionSyncService> logger)
        {
            _catalog = catalog;
            _scopeFactory = scopeFactory;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Synchronizing {PermissionCount} permissions declared by {ModuleCount} module(s)",
                _catalog.AllCodes.Count, _catalog.Modules.Count);

            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var result = await scope.ServiceProvider.GetRequiredService<PermissionCatalogSynchronizer>()
                        .SynchronizeAsync(stoppingToken);

                    _logger.LogInformation(
                        "Permissions synchronized: {WrittenPermissions} inserted or updated, {GrantedToAdmin} granted to Admin",
                        result.WrittenPermissions, result.GrantedToAdmin);

                    if (result.UndeclaredCodes.Count > 0)
                        _logger.LogWarning(
                            "These permissions are in the database but no module declares them anymore; they were kept, remove them on purpose if they are obsolete: {UndeclaredCodes}",
                            string.Join(", ", result.UndeclaredCodes));

                    return;
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogWarning(exception, "Permission synchronization attempt {Attempt} of {MaxAttempts} failed", attempt, MaxAttempts);
                    await Task.Delay(RetryDelay, _timeProvider, stoppingToken);
                }
            }

            _logger.LogError("Permission synchronization gave up after {MaxAttempts} attempts; new permissions are missing from the database", MaxAttempts);
        }
    }
}
