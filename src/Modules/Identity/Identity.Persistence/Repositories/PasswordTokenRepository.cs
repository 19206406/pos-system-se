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

        public async Task<List<PasswordToken>> GetAllByUserIdAsync(Guid userId)
        {
            var passwordTokens = await
                _context.PasswordTokens.Where(pt => pt.UserId == userId && !pt.UsedAt.HasValue).ToListAsync();

            return passwordTokens;
        }

        public async Task<PasswordToken?> GetByTokenAsync(string tokenHash)
        {
            var passwordToken = await _context.PasswordTokens.FirstOrDefaultAsync(pt => pt.TokenHash == tokenHash);
            return passwordToken;
        }
    }
}
