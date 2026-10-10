using Identity.Application.Contracts.Authentication;
using Identity.Application.Dtos.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    internal sealed class UserAccessReader : IUserAccessReader
    {
        private readonly IdentityDbContext _context;

        public UserAccessReader(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<UserAccessDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var userRoles = _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId);

            var roles = await userRoles
                .Select(ur => ur.Role.RoleName)
                .Distinct()
                .ToListAsync(cancellationToken);

            var permissions = await userRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Identifier)
                .Distinct()
                .ToListAsync();

            return new UserAccessDto(roles, permissions); 
        }
    }
}
