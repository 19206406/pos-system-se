using Identity.Application.Contracts.Persistence;
using Identity.Application.Dtos.Permissions;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    internal sealed class PermissionReadRepository : IPermissionReadRepository
    {
        private readonly IdentityDbContext _context;

        public PermissionReadRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken cancellationToken) =>
            await _context.Permissions
                .AsNoTracking()
                .OrderBy(p => p.Module).ThenBy(p => p.Identifier)
                .Select(p => new PermissionDto(p.Identifier, p.PermissionName, p.PermissionDescription, p.Module))
                .ToListAsync(cancellationToken);

        // Served by idx_permissions_module
        public async Task<IReadOnlyList<PermissionDto>> GetByModuleAsync(string moduleCode, CancellationToken cancellationToken) =>
            await _context.Permissions
                .AsNoTracking()
                .Where(p => p.Module == moduleCode)
                .OrderBy(p => p.Identifier)
                .Select(p => new PermissionDto(p.Identifier, p.PermissionName, p.PermissionDescription, p.Module))
                .ToListAsync(cancellationToken);
    }
}
