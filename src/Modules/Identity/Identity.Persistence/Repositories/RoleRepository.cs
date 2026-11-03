using Identity.Application.Contracts.Persistence;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    public sealed class RoleRepository : IRoleRepository
    {
        private readonly IdentityDbContext _context;

        public RoleRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public void AddRole(Role role)
        {
            _context.Roles.Add(role);
        }

        public async Task<Role?> GetByIdWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken) =>
            await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        public async Task<bool> ExistsAsync(Guid roleId, CancellationToken cancellationToken) =>
            await _context.Roles.AnyAsync(r => r.Id == roleId, cancellationToken);

        // lower() on both sides; uq_roles_role_name is case-sensitive, so this check is what rejects "admin" next to "Admin"
        public async Task<bool> NameExistsAsync(string roleName, Guid? excludedRoleId, CancellationToken cancellationToken)
        {
            var loweredName = roleName.ToLowerInvariant();

            return await _context.Roles.AnyAsync(
                r => r.RoleName.ToLower() == loweredName && (excludedRoleId == null || r.Id != excludedRoleId),
                cancellationToken);
        }

        public async Task<IReadOnlyList<Guid>> GetExistingIdsAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken) =>
            await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

        public async Task<bool> HasUsersAsync(Guid roleId, CancellationToken cancellationToken) =>
            await _context.UserRoles.AnyAsync(ur => ur.RoleId == roleId, cancellationToken);

        // Single DELETE ... WHERE NOT EXISTS: the check and the delete cannot be split by another request.
        // roles_permissions rows go with the role (ON DELETE CASCADE); users_roles is RESTRICT as a last guard.
        public async Task<bool> TryDeleteUnassignedAsync(Guid roleId, CancellationToken cancellationToken) =>
            await _context.Roles
                .Where(r => r.Id == roleId && !r.UserRoles.Any())
                .ExecuteDeleteAsync(cancellationToken) > 0;

        public async Task LockAsync(Guid roleId, CancellationToken cancellationToken)
        {
            // Outside a transaction the lock would be released as soon as the statement ends
            if (_context.Database.CurrentTransaction is null)
                throw new InvalidOperationException("LockAsync must run inside ExecuteInTransactionAsync.");

            // Schema written literally: identifiers cannot be SQL parameters (it is Schemas.Identity)
            await _context.Database.ExecuteSqlAsync(
                $"SELECT 1 FROM identity.roles WHERE id = {roleId} FOR UPDATE", cancellationToken);
        }
    }
}
