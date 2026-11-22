using Identity.Application.Contracts.Messaging;
using Shared.Messaging;

namespace Identity.Application.Messaging
{
    /// <summary>
    /// Handles one message read from the notifications queue with the handler registered for its type.
    /// The transport runs each message in its own DI scope, so a failure cannot leave changes behind for the next one,
    /// and reacts to an exception by retrying later and, after the maximum attempts, dead-lettering the message.
    /// </summary>
    public sealed class NotificationMessageConsumer
    {
        private readonly IReadOnlyDictionary<string, IOutboxMessageHandler> _handlers;
        private readonly IIdentityInboxStore _inbox;
        private readonly TimeProvider _timeProvider;

        public NotificationMessageConsumer(IEnumerable<IOutboxMessageHandler> handlers, IIdentityInboxStore inbox, TimeProvider timeProvider)
        {
            _handlers = handlers.ToDictionary(handler => handler.MessageType);
            _inbox = inbox;
            _timeProvider = timeProvider;
        }

        public async Task HandleAsync(MessageEnvelope message, CancellationToken cancellationToken)
        {
            if (!_handlers.TryGetValue(message.Type, out var handler))
                throw new InvalidOperationException($"No handler is registered for message type '{message.Type}'.");

            var handlerName = handler.GetType().Name;

            // A redelivery of something already sent is acknowledged without sending it again
            if (await _inbox.WasProcessedAsync(message.MessageId, handlerName, cancellationToken))
                return;

            await handler.HandleAsync(message.Payload, cancellationToken);

            // After the handler, not before: sending an email cannot join a database transaction, and recording the message
            // first would lose the email if the process died in between. The worst case is one repeated email, never a missing one.
            await _inbox.MarkProcessedAsync(message.MessageId, handlerName, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        }
    }
}
