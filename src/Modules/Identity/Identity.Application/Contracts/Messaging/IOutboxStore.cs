using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Messaging
{
    /// <summary> Used only by the outbox processor. </summary>
    public interface IOutboxStore
    {
        /// <summary>
        /// Takes up to <paramref name="batchSize"/> pending messages and locks each one until <paramref name="lockedUntilUtc"/>
        /// with a conditional update, so two processor instances never deliver the same message at the same time.
        /// </summary>
        Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(
            int batchSize, int maxAttempts, DateTime nowUtc, DateTime lockedUntilUtc, CancellationToken cancellationToken);

        Task MarkProcessedAsync(Guid messageId, DateTime nowUtc, CancellationToken cancellationToken);

        /// <summary> Counts the attempt and releases the lock so the message is retried later. </summary>
        Task MarkFailedAsync(Guid messageId, string error, CancellationToken cancellationToken);
    }
}
