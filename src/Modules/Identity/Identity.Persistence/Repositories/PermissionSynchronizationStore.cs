using Identity.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Authorization;

namespace Identity.Persistence.Repositories
{
    /// <summary>
    /// Raw SQL on purpose: EF Core has no upsert, and INSERT ... ON CONFLICT is what makes two instances starting at the
    /// same time safe (the second waits for the first and then finds nothing to do). The schema is written literally
    /// because identifiers cannot be SQL parameters; it is Schemas.Identity.
    /// </summary>
    internal sealed class PermissionSynchronizationStore : IPermissionSynchronizationStore
    {
        private readonly IdentityDbContext _context;

        public PermissionSynchronizationStore(IdentityDbContext context)
        {
            _context = context;
        }

        // The WHERE of DO UPDATE skips rows already up to date, so a run with no changes writes nothing
        public async Task<bool> UpsertAsync(string moduleCode, PermissionDefinition permission, CancellationToken cancellationToken) =>
            await _context.Database.ExecuteSqlAsync($"""
                INSERT INTO identity.permissions (identifier, module, permission_name, permission_description)
                VALUES ({permission.Code}, {moduleCode}, {permission.Name}, {permission.Description})
                ON CONFLICT (identifier) DO UPDATE
                SET module = EXCLUDED.module,
                    permission_name = EXCLUDED.permission_name,
                    permission_description = EXCLUDED.permission_description
                WHERE permissions.module IS DISTINCT FROM EXCLUDED.module
                   OR permissions.permission_name IS DISTINCT FROM EXCLUDED.permission_name
                   OR permissions.permission_description IS DISTINCT FROM EXCLUDED.permission_description
                """, cancellationToken) > 0;

        public async Task<int> GrantMissingToRoleAsync(Guid roleId, IReadOnlyCollection<string> codes, CancellationToken cancellationToken)
        {
            var codeArray = codes.ToArray();

            return await _context.Database.ExecuteSqlAsync($"""
                INSERT INTO identity.roles_permissions (role_id, permission_id)
                SELECT {roleId}, p.id
                FROM identity.permissions AS p
                WHERE p.identifier = ANY ({codeArray})
                ON CONFLICT (role_id, permission_id) DO NOTHING
                """, cancellationToken);
        }

        public async Task<IReadOnlyList<string>> GetUndeclaredCodesAsync(
            IReadOnlyCollection<string> declaredCodes, CancellationToken cancellationToken) =>
            await _context.Permissions
                .AsNoTracking()
                .Where(p => !declaredCodes.Contains(p.Identifier))
                .OrderBy(p => p.Identifier)
                .Select(p => p.Identifier)
                .ToListAsync(cancellationToken);
    }
}
