using Shared.Authorization;

namespace Identity.Application.Contracts.Persistence
{
    /// <summary>
    /// Idempotent writes used only by the permission synchronization. Every operation is safe to run concurrently
    /// from several instances starting at the same time.
    /// </summary>
    public interface IPermissionSynchronizationStore
    {
        /// <summary>
        /// Inserts the permission, or updates its module, name and description if any of them changed.
        /// Returns true if a row was written; false if it was already up to date.
        /// </summary>
        Task<bool> UpsertAsync(string moduleCode, PermissionDefinition permission, CancellationToken cancellationToken);

        /// <summary> Gives the role every permission in <paramref name="codes"/> it does not have yet. Returns how many it added. </summary>
        Task<int> GrantMissingToRoleAsync(Guid roleId, IReadOnlyCollection<string> codes, CancellationToken cancellationToken);

        /// <summary> Codes stored in the table that no catalog declares anymore. </summary>
        Task<IReadOnlyList<string>> GetUndeclaredCodesAsync(IReadOnlyCollection<string> declaredCodes, CancellationToken cancellationToken);
    }
}
