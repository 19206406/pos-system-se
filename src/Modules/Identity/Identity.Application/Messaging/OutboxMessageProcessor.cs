using Identity.Application.Contracts.Messaging;
using Identity.Domain.Entities;

namespace Identity.Application.Messaging
{
    /// <summary>
    /// Delivers one claimed outbox message with its handler and records the outcome.
    /// The caller runs each message in its own DI scope so a failure cannot leave changes behind for the next one.
    /// </summary>
    public sealed class OutboxMessageProcessor
    {
        private readonly IReadOnlyDictionary<string, IOutboxMessageHandler> _handlers;
        private readonly IOutboxStore _outboxStore;
        private readonly TimeProvider _timeProvider;

        public OutboxMessageProcessor(IEnumerable<IOutboxMessageHandler> handlers, IOutboxStore outboxStore, TimeProvider timeProvider)
        {
            _handlers = handlers.ToDictionary(handler => handler.MessageType);
            _outboxStore = outboxStore;
            _timeProvider = timeProvider;
        }

        /// <returns> null when delivered; otherwise the error recorded on the message. </returns>
        public async Task<string?> ProcessAsync(OutboxMessage message, CancellationToken cancellationToken)
        {
            if (!_handlers.TryGetValue(message.Type, out var handler))
            {
                var missing = $"No handler is registered for message type '{message.Type}'.";
                await _outboxStore.MarkFailedAsync(message.Id, missing, cancellationToken);
                return missing;
            }

            try
            {
                await handler.HandleAsync(message.Payload, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Type and message only: no stack trace in the table, and handlers never put secrets in exception messages
                var error = $"{exception.GetType().Name}: {exception.Message}";
                await _outboxStore.MarkFailedAsync(message.Id, error, cancellationToken);
                return error;
            }

            await _outboxStore.MarkProcessedAsync(message.Id, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
            return null;
        }
    }
}
