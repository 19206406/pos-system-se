using Identity.Application.Contracts.Persistence;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    public class PasswordTokenRepository : IPasswordTokenRepository
    {
        private readonly IdentityDbContext _context;

        public PasswordTokenRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<PasswordToken?> GetPasswordToken(string tokenHash)
        {
            var passwordToken = await _context.PasswordTokens.FirstOrDefaultAsync(pt => pt.TokenHash == tokenHash);
            return passwordToken; 
        }

        public async Task<List<PasswordToken>> GetPasswordTokensByUserId(Guid userId)
        {
            var passwordTokens = await 
                _context.PasswordTokens.Where(pt => pt.UserId == userId && !pt.UsedAt.HasValue).ToListAsync();

            return passwordTokens; 
        }
    }
}
