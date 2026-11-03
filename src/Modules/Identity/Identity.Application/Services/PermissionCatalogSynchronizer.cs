using Identity.Application.Authorization;
using Identity.Application.Contracts.Persistence;
using Identity.Domain.Constants;

namespace Identity.Application.Services
{
    /// <param name="WrittenPermissions"> Permissions inserted, or updated because their module, name or description changed. </param>
    /// <param name="GrantedToAdmin"> Permissions the Admin role did not have and received. </param>
    /// <param name="UndeclaredCodes"> Codes still in the table that no module declares anymore; they are kept, never deleted. </param>
    public sealed record PermissionSyncResult(int WrittenPermissions, int GrantedToAdmin, IReadOnlyList<string> UndeclaredCodes);

    /// <summary>
    /// Makes the permissions table match the catalogs the modules declare in code. Idempotent: running it again changes
    /// nothing, and it never touches the permissions of roles other than Admin, which always holds every permission.
    /// A permission that disappears from the code is not deleted, because roles may still reference it; it is reported
    /// so it can be removed on purpose.
    /// </summary>
    public sealed class PermissionCatalogSynchronizer
    {
        private readonly PermissionCatalog _catalog;
        private readonly IPermissionSynchronizationStore _store;
        private readonly IUnitOfWork _unitOfWork;

        public PermissionCatalogSynchronizer(PermissionCatalog catalog, IPermissionSynchronizationStore store, IUnitOfWork unitOfWork)
        {
            _catalog = catalog;
            _store = store;
            _unitOfWork = unitOfWork;
        }

        public async Task<PermissionSyncResult> SynchronizeAsync(CancellationToken cancellationToken)
        {
            var written = 0;
            var granted = 0;
            IReadOnlyList<string> undeclared = [];

            // One transaction: the table never shows half a catalog. The counters reset because the strategy may retry.
            await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
            {
                written = 0;

                foreach (var module in _catalog.Modules)
                {
                    foreach (var permission in module.Permissions)
                    {
                        if (await _store.UpsertAsync(module.ModuleCode, permission, transactionToken))
                            written++;
                    }
                }

                granted = await _store.GrantMissingToRoleAsync(SystemRoles.AdminId, _catalog.AllCodes, transactionToken);
                undeclared = await _store.GetUndeclaredCodesAsync(_catalog.AllCodes, transactionToken);
            }, cancellationToken);

            return new PermissionSyncResult(written, granted, undeclared);
        }
    }
}
