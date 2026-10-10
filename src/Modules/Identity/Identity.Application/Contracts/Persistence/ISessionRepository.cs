using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    public interface ISessionRepository
    {
        /// <summary> Tracking includes the user </summary>
        Task AddAsync(Session session, CancellationToken cancellationToken);
        /// <summary> Revokes atomically only if it remains unrevoked. Returns false if another request won the race </summary>
        Task<Session?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken); 
        Task<bool> TryMarkReplacedAsync(Guid sessionId, Guid replacedById, DateTime now, CancellationToken cancellationToken);
        Task RevokeAllActiveByUserAsync(Guid userId, DateTime now, CancellationToken cancellationToken); 

    }
}
