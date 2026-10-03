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
            User user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            return user; 
        }

        public Task<User?> GetUserById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUser()
        {
            throw new NotImplementedException();
        }
    }
}
