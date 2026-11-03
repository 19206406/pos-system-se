using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    /// <summary> Write side of roles: tracked entities and the conditional operations commands need. </summary>
    public interface IRoleRepository
    {
        void AddRole(Role role);

        /// <summary> Tracked, with <see cref="Role.RolePermissions"/> loaded. </summary>
        Task<Role?> GetByIdWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(Guid roleId, CancellationToken cancellationToken);

        /// <summary> Case-insensitive. <paramref name="excludedRoleId"/> lets a role keep its own name on update. </summary>
        Task<bool> NameExistsAsync(string roleName, Guid? excludedRoleId, CancellationToken cancellationToken);

        /// <summary> The subset of <paramref name="roleIds"/> that exists. </summary>
        Task<IReadOnlyList<Guid>> GetExistingIdsAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken);

        Task<bool> HasUsersAsync(Guid roleId, CancellationToken cancellationToken);

        /// <summary>
        /// Conditional delete: removes the role only if no user holds it at that moment (its permission assignments go
        /// with it). Returns false if it was not deleted.
        /// </summary>
        Task<bool> TryDeleteUnassignedAsync(Guid roleId, CancellationToken cancellationToken);

        /// <summary>
        /// Locks the role row until the current transaction ends. Must run inside ExecuteInTransactionAsync.
        /// It serializes changes to who holds the role, e.g. so two requests cannot both remove the last administrator.
        /// </summary>
        Task LockAsync(Guid roleId, CancellationToken cancellationToken);
    }
}
