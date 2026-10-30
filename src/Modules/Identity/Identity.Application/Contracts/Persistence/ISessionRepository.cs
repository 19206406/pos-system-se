using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    /// <summary> Session writes. Only <see cref="Services.IAuthSessionService"/> uses it; reads go through ISessionReadRepository. </summary>
    public interface ISessionRepository
    {
        Task AddAsync(Session session, CancellationToken cancellationToken);

        /// <summary> Tracked and includes the user. </summary>
        Task<Session?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

        /// <summary> Revokes atomically only if it is still not revoked. false = another request won the race. </summary>
        Task<bool> TryMarkAsReplacedAsync(Guid sessionId, Guid replacedById, DateTime nowUtc, CancellationToken cancellationToken);

        Task RevokeAllActiveByUserAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken);

        /// <summary> Filters by user in the query itself, so another user's session is indistinguishable from a missing one. </summary>
        Task<bool> RevokeActiveByIdAsync(Guid userId, Guid sessionId, DateTime nowUtc, CancellationToken cancellationToken);

        Task RevokeAllActiveByUserExceptAsync(Guid userId, Guid exceptSessionId, DateTime nowUtc, CancellationToken cancellationToken);
    }
}
