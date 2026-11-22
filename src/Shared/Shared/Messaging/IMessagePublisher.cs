namespace Shared.Messaging
{
    /// <summary>
    /// Sends an envelope to the exchange of the module that emits it. Only the outbox dispatchers use it:
    /// business code never publishes directly, it writes to its module's outbox in the same transaction as the change.
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Completes only when the transport has accepted the message (broker confirm). Throws if it did not,
        /// so the dispatcher keeps the outbox row pending and retries it.
        /// </summary>
        Task PublishAsync(string exchange, MessageEnvelope message, CancellationToken cancellationToken);
    }
}
