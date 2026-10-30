namespace Identity.Application.Contracts.Persistence
{
    /// <summary> Deletes rows that no longer serve any purpose. Used only by the purge background job. </summary>
    public interface IIdentityDataRetention
    {
        /// <summary>
        /// Removes sessions and password tokens that expired, were revoked or were used before <paramref name="cutoffUtc"/>,
        /// and outbox messages delivered before it. Undelivered outbox messages are kept for inspection.
        /// </summary>
        Task<RetentionPurgeResult> PurgeAsync(DateTime cutoffUtc, CancellationToken cancellationToken);
    }

    public sealed record RetentionPurgeResult(int Sessions, int PasswordTokens, int OutboxMessages);
}
