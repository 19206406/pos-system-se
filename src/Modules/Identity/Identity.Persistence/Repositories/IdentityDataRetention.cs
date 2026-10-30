using Identity.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories
{
    internal sealed class IdentityDataRetention : IIdentityDataRetention
    {
        private readonly IdentityDbContext _context;

        public IdentityDataRetention(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<RetentionPurgeResult> PurgeAsync(DateTime cutoffUtc, CancellationToken cancellationToken)
        {
            // One statement on purpose: a rotation chain shares ExpiresAt, and every older link of a chain was revoked
            // earlier than the newer one, so a chain is always deleted together and fk_sessions_replaced_by
            // (checked at the end of the statement) never points to a deleted row.
            var sessions = await _context.Sessions
                .Where(s => s.ExpiresAt < cutoffUtc || s.RevokedAt < cutoffUtc)
                .ExecuteDeleteAsync(cancellationToken);

            var passwordTokens = await _context.PasswordTokens
                .Where(pt => pt.ExpiresAt < cutoffUtc || pt.UsedAt < cutoffUtc)
                .ExecuteDeleteAsync(cancellationToken);

            var outboxMessages = await _context.OutboxMessages
                .Where(m => m.ProcessedAt < cutoffUtc)
                .ExecuteDeleteAsync(cancellationToken);

            return new RetentionPurgeResult(sessions, passwordTokens, outboxMessages);
        }
    }
}
