using Identity.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Identity.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDbCotext _context;

        public UserRepository(IdentityDbCotext context)
        {
            _context = context;
        }

        public async Task<Entities.User?> GetUserById(Guid id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return user; 
        }

        public async Task<Entities.User?> GetUserByEmail(string email)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email);

            return user;
        }

        public async Task CreateUser(Entities.User user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync(); 
        }

        public async Task UpdateUser()
        {
            await _context.SaveChangesAsync(); 
        }
    }
}
