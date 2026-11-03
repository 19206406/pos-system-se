using Identity.Application.Contracts.Persistence;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDbContext _context;

        public UserRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
            await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);

        public async Task<bool> AnyInRoleAsync(Guid roleId, CancellationToken cancellationToken) =>
            await _context.UserRoles.AnyAsync(ur => ur.RoleId == roleId, cancellationToken);

        public async Task<bool> AnyOtherUserInRoleAsync(Guid roleId, Guid excludedUserId, CancellationToken cancellationToken) =>
            await _context.UserRoles.AnyAsync(ur => ur.RoleId == roleId && ur.UserId != excludedUserId, cancellationToken);

        public async Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken) =>
            await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            return user;
        }
    }
}
