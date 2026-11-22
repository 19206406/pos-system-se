using Identity.Application.Contracts.Messaging;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Messaging
{
    internal sealed class InboxStore : IIdentityInboxStore
    {
        private readonly IdentityDbContext _context;

        public InboxStore(IdentityDbContext context)
        {
            _context = context;
        }

        public Task<bool> WasProcessedAsync(Guid messageId, string handler, CancellationToken cancellationToken) =>
            _context.InboxMessages
                .AsNoTracking()
                .AnyAsync(m => m.MessageId == messageId && m.Handler == handler, cancellationToken);

        // ON CONFLICT DO NOTHING: two deliveries of the same message that finish at the same time must not fail on the primary key
        public async Task MarkProcessedAsync(Guid messageId, string handler, DateTime nowUtc, CancellationToken cancellationToken) =>
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO identity.inbox_messages (message_id, handler, processed_at)
                VALUES ({messageId}, {handler}, {nowUtc})
                ON CONFLICT (message_id, handler) DO NOTHING
                """,
                cancellationToken);
    }
}
