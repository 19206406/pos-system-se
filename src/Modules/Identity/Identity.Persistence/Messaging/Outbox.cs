using System.Diagnostics;
using Identity.Application.Contracts.Messaging;
using Identity.Application.Messaging;
using Identity.Domain.Entities;

namespace Identity.Persistence.Messaging
{
    /// <summary> Adds the message to the same DbContext as the business change, so one SaveChanges writes both. </summary>
    internal sealed class Outbox : IOutbox
    {
        private readonly IdentityDbContext _context;
        private readonly TimeProvider _timeProvider;

        public Outbox(IdentityDbContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public void Enqueue<TMessage>(TMessage message) where TMessage : IOutboxMessage
        {
            _context.OutboxMessages.Add(new OutboxMessage
            {
                Type = TMessage.MessageType,
                Payload = OutboxSerializer.Serialize(message),
                OccurredAt = _timeProvider.GetUtcNow().UtcDateTime,
                CorrelationId = CurrentCorrelationId()
            });
        }

        // The request trace id (32 hex digits) is a valid Guid, so it ties the message to the request that caused it.
        // Outside a request (the admin bootstrap) there is no trace and the message has no correlation id.
        private static Guid? CurrentCorrelationId() =>
            Activity.Current is { } activity && activity.TraceId != default
                ? Guid.ParseExact(activity.TraceId.ToHexString(), "N")
                : null;
    }
}
