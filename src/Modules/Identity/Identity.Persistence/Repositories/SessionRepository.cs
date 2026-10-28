using Identity.Application.Contracts.Persistence;
using Identity.Application.Dtos.Authentication;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    internal sealed class SessionRepository : ISessionRepository
    {
        private readonly IdentityDbContext _context;

        public SessionRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Session session, CancellationToken cancellationToken)
            => await _context.Sessions.AddAsync(session, cancellationToken);

        public async Task<Session?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
            => await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);

        public async Task<bool> TryMarkAsReplacedAsync(Guid sessionId, Guid replacedById, DateTime now, CancellationToken cancellationToken)
        {
            var affected = await _context.Sessions
                .Where(s => s.Id == sessionId && s.RevokedAt == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.RevokedAt, now)
                    .SetProperty(x => x.ReplacedById, replacedById), cancellationToken);

            return affected == 1;
        }

        public async Task RevokeAllActiveByUserAsync(Guid userId, DateTime now, CancellationToken cancellationToken)
            => await _context.Sessions
                .Where(s => s.UserId == userId && s.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now), cancellationToken);

        public async Task<bool> RevokeActiveByIdAsync(Guid userId, Guid sessionId, DateTime nowUtc, CancellationToken cancellationToken)
        {
            var affected = await _context.Sessions
                .Where(s => s.Id == sessionId && s.UserId == userId && s.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, nowUtc), cancellationToken);

            return affected == 1;
        }

        public async Task RevokeAllActiveByUserExceptAsync(Guid userId, Guid exceptSessionId, DateTime nowUtc, CancellationToken cancellationToken) =>
            await _context.Sessions.Where(s => s.UserId == userId && s.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, nowUtc), cancellationToken);

        public async Task<IReadOnlyList<SessionSummaryDto>> GetActiveByUserAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken) =>
            await _context.Sessions.AsNoTracking()
                .Where(s => s.UserId == userId && s.RevokedAt == null && s.ExpiresAt > nowUtc)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new SessionSummaryDto(s.Id, s.DeviceInfo, s.IpAddress, s.CreatedAt, s.ExpiresAt))
                .ToListAsync(cancellationToken); 




        public async Task<int> CountAllActiveByUserAsync(Guid userId, CancellationToken cancellationToken)
            => await _context.Sessions
                .CountAsync(s => s.UserId == userId && s.RevokedAt == null); 

        public async Task<List<Session>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken) 
            => await _context.Sessions
                .Where(s => s.UserId == userId && s.RevokedAt == null).ToListAsync();
    }
}
