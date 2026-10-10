using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Services;
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
        {
            await _context.Sessions.AddAsync(session, cancellationToken); 
        }

        public async Task<Session?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            return await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken); ; 
        }

        public async Task RevokeAllActiveByUserAsync(Guid userId, DateTime now, CancellationToken cancellationToken)
        {
            await _context.Sessions
                .Where(s => s.UserId == userId && s.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now), cancellationToken); 
        }

        public async Task<bool> TryMarkReplacedAsync(Guid sessionId, Guid replacedById, DateTime now, CancellationToken cancellationToken)
        {
            var affected = await _context.Sessions
                .Where(s => s.Id == sessionId && s.RevokedAt == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.RevokedAt, now)
                    .SetProperty(x => x.ReplacedById, replacedById), cancellationToken);

            return affected == 1; 
        }
    }
}
