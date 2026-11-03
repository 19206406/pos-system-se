using Identity.Application.Contracts.Persistence;
using Identity.Application.Dtos.Permissions;
using Identity.Application.Dtos.Roles;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    internal sealed class RoleReadRepository : IRoleReadRepository
    {
        private readonly IdentityDbContext _context;

        public RoleReadRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<RoleSummaryDto>> GetAllAsync(CancellationToken cancellationToken) =>
            await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.RoleName).ThenBy(r => r.Id)
                .Select(r => new RoleSummaryDto(
                    r.Id, r.RoleName, r.RoleDescription, r.RolePermissions.Count, r.UserRoles.Count))
                .ToListAsync(cancellationToken);

        public async Task<RoleDetailDto?> GetDetailByIdAsync(Guid roleId, CancellationToken cancellationToken) =>
            await _context.Roles
                .AsNoTracking()
                .Where(r => r.Id == roleId)
                .Select(r => new RoleDetailDto(
                    r.Id,
                    r.RoleName,
                    r.RoleDescription,
                    r.RolePermissions
                        .Select(rp => new PermissionDto(
                            rp.Permission.Identifier,
                            rp.Permission.PermissionName,
                            rp.Permission.PermissionDescription,
                            rp.Permission.Module))
                        .ToList()))
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyList<RoleReferenceDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
            await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .OrderBy(ur => ur.Role.RoleName)
                .Select(ur => new RoleReferenceDto(ur.RoleId, ur.Role.RoleName))
                .ToListAsync(cancellationToken);
    }
}
