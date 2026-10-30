using Identity.Application.Contracts.Messaging;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Messaging
{
    internal sealed class OutboxStore : IOutboxStore
    {
        private readonly IdentityDbContext _context;

        public OutboxStore(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(
            int batchSize, int maxAttempts, DateTime nowUtc, DateTime lockedUntilUtc, CancellationToken cancellationToken)
        {
            var candidateIds = await _context.OutboxMessages
                .AsNoTracking()
                .Where(m => m.ProcessedAt == null && m.Attempts < maxAttempts && (m.LockedUntil == null || m.LockedUntil < nowUtc))
                .OrderBy(m => m.OccurredAt)
                .Select(m => m.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            var claimedIds = new List<Guid>(candidateIds.Count);

            foreach (var id in candidateIds)
            {
                // UPDATE ... WHERE lock is free: one affected row means this instance owns the message now
                var affected = await _context.OutboxMessages
                    .Where(m => m.Id == id && m.ProcessedAt == null && (m.LockedUntil == null || m.LockedUntil < nowUtc))
                    .ExecuteUpdateAsync(m => m.SetProperty(x => x.LockedUntil, lockedUntilUtc), cancellationToken);

                if (affected == 1)
                    claimedIds.Add(id);
            }

            if (claimedIds.Count == 0)
                return [];

            return await _context.OutboxMessages
                .AsNoTracking()
                .Where(m => claimedIds.Contains(m.Id))
                .OrderBy(m => m.OccurredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task MarkProcessedAsync(Guid messageId, DateTime nowUtc, CancellationToken cancellationToken) =>
            await _context.OutboxMessages
                .Where(m => m.Id == messageId)
                .ExecuteUpdateAsync(m => m
                    .SetProperty(x => x.ProcessedAt, nowUtc)
                    .SetProperty(x => x.LockedUntil, (DateTime?)null)
                    .SetProperty(x => x.LastError, (string?)null), cancellationToken);

        public async Task MarkFailedAsync(Guid messageId, string error, CancellationToken cancellationToken)
        {
            var truncated = error.Length > OutboxMessage.LastErrorMaxLength ? error[..OutboxMessage.LastErrorMaxLength] : error;

            await _context.OutboxMessages
                .Where(m => m.Id == messageId)
                .ExecuteUpdateAsync(m => m
                    .SetProperty(x => x.Attempts, x => x.Attempts + 1)
                    .SetProperty(x => x.LastError, truncated)
                    .SetProperty(x => x.LockedUntil, (DateTime?)null), cancellationToken);
        }
    }
}
