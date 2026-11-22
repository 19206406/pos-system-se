namespace Identity.Application.Contracts.Messaging
{
    /// <summary>
    /// Idempotency of the message consumers. The broker delivers at least once, so a consumer asks whether its handler
    /// already processed a message and records it afterwards. Used only by the message consumers.
    /// </summary>
    public interface IIdentityInboxStore
    {
        Task<bool> WasProcessedAsync(Guid messageId, string handler, CancellationToken cancellationToken);

        /// <summary> Records the message as processed. Recording it twice is not an error. </summary>
        Task MarkProcessedAsync(Guid messageId, string handler, DateTime nowUtc, CancellationToken cancellationToken);
    }
}
