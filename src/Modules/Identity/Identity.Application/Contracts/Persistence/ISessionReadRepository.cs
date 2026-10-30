using Identity.Application.Dtos.Authentication;

namespace Identity.Application.Contracts.Persistence
{
    /// <summary> Read-only access to sessions: no tracking, projections only. Never exposes TokenHash. </summary>
    public interface ISessionReadRepository
    {
        /// <summary> Sessions not revoked and not expired at <paramref name="nowUtc"/>, newest first. </summary>
        Task<IReadOnlyList<SessionSummaryDto>> GetActiveByUserAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken);
    }
}
