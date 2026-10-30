using Identity.Application.Contracts.Persistence;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    public sealed class PasswordTokenRepository : IPasswordTokenRepository
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

        public async Task<PasswordToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
            await _context.PasswordTokens
                .FirstOrDefaultAsync(pt => pt.TokenHash == tokenHash, cancellationToken);

        public async Task<bool> TryConsumeAsync(Guid tokenId, DateTime nowUtc, CancellationToken cancellationToken)
        {
            // UPDATE ... WHERE id = @id AND used_at IS NULL AND expires_at > now: one affected row means this request consumed it
            var affected = await _context.PasswordTokens
                .Where(pt => pt.Id == tokenId && pt.UsedAt == null && pt.ExpiresAt > nowUtc)
                .ExecuteUpdateAsync(pt => pt.SetProperty(x => x.UsedAt, nowUtc), cancellationToken);

            return affected == 1;
        }

        public async Task RevokeAllActiveByUserAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken) =>
            await _context.PasswordTokens
                .Where(pt => pt.UserId == userId && pt.UsedAt == null)
                .ExecuteUpdateAsync(pt => pt.SetProperty(x => x.UsedAt, nowUtc), cancellationToken);
    }
}
