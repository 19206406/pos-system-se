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

        public void Add(PasswordToken passwordToken)
        {
            _context.PasswordTokens.Add(passwordToken); 
        }

        public async Task<List<PasswordToken>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var passwordTokens = await _context.PasswordTokens
                .Where(pt => pt.UserId == userId && !pt.UsedAt.HasValue)
                .ToListAsync(cancellationToken);

            return passwordTokens;
        }

        public async Task<PasswordToken?> GetByTokenAsync(string tokenHash, CancellationToken cancellationToken)
        {
            var passwordToken = await _context.PasswordTokens
                .FirstOrDefaultAsync(pt => pt.TokenHash == tokenHash, cancellationToken);

            return passwordToken;
        }

        public async Task RevokeAllActiveByUserAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
        {
            await _context.PasswordTokens
                .Where(pt => pt.UserId == userId && pt.UsedAt == null)
                .ExecuteUpdateAsync(pt => pt
                    .SetProperty(x => x.UsedAt, now), cancellationToken); 
        }
    }
}
