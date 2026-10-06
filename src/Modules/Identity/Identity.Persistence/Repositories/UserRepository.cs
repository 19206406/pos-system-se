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

        public async Task CreateUser(User user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync(); 
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            return user; 
        }

        public async Task<User?> GetUserById(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user; 
        }

        public async Task<User?> GetUserWithPasswordTokens(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.PasswordTokens.Where(pt => !pt.UsedAt.HasValue))
                .FirstOrDefaultAsync(u => u.Id == id);

            return user; 
        }

        public Task UpdateUser()
        {
            throw new NotImplementedException();
        }
    }
}
