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

        public async Task DisableAsync(Guid id, CancellationToken cancellationToken)
        {
            await _context.Users.Where(u => u.Id == id)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.IsActive, false), cancellationToken); 
        }

        public async Task EnableAsync(Guid id, CancellationToken cancellationToken)
        {
            await _context.Users.Where(u => u.Id == id)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.IsActive, true), cancellationToken);
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

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            return user;
        }

        public async Task<User?> GetWithPasswordTokensAsync(Guid id, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.PasswordTokens.Where(pt => !pt.UsedAt.HasValue))
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            return user;
        }
    }
}
