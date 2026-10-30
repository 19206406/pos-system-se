namespace Identity.Application.Messaging
{
    public interface IOutboxMessageHandler
    {
        string MessageType { get; }
        Task HandleAsync(string payload, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Base for the handler of one message type. Delivery is at-least-once: a handler may run again for the
    /// same message (crash before it was marked processed), so it must tolerate repetition.
    /// </summary>
    public abstract class OutboxMessageHandler<TMessage> : IOutboxMessageHandler
        where TMessage : IOutboxMessage
    {
        public string MessageType => TMessage.MessageType;

        public Task HandleAsync(string payload, CancellationToken cancellationToken) =>
            HandleAsync(OutboxSerializer.Deserialize<TMessage>(payload), cancellationToken);

        protected abstract Task HandleAsync(TMessage message, CancellationToken cancellationToken);
    }
}
