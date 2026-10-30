using Identity.Application.Messaging;

namespace Identity.Application.Contracts.Messaging
{
    /// <summary>
    /// Adds a message to the unit of work: it is written by the next SaveChangesAsync, in the same transaction
    /// as the change that caused it, so either both are stored or neither is.
    /// </summary>
    public interface IOutbox
    {
        void Enqueue<TMessage>(TMessage message) where TMessage : IOutboxMessage;
    }
}
