using Identity.Application.Contracts.Persistence;
using Identity.Application.Dtos.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    internal sealed class SessionReadRepository : ISessionReadRepository
    {
        private readonly IdentityDbContext _context;

        public SessionReadRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<SessionSummaryDto>> GetActiveByUserAsync(
            Guid userId, DateTime nowUtc, CancellationToken cancellationToken) =>
            await _context.Sessions
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.RevokedAt == null && s.ExpiresAt > nowUtc)
                .OrderByDescending(s => s.CreatedAt)
                // TokenHash is deliberately left out of the projection
                .Select(s => new SessionSummaryDto(s.Id, s.DeviceInfo, s.IpAddress, s.CreatedAt, s.ExpiresAt))
                .ToListAsync(cancellationToken);
    }
}
