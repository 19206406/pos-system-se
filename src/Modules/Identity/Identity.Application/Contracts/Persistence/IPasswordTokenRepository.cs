using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Persistence
{
    public interface IPasswordTokenRepository
    {
        Task<PasswordToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

        void Add(PasswordToken passwordToken);

        /// <summary>
        /// Marks the token as used only if it is still unused and not expired (conditional update).
        /// false = it was already consumed, possibly by a concurrent request, or it expired.
        /// </summary>
        Task<bool> TryConsumeAsync(Guid tokenId, DateTime nowUtc, CancellationToken cancellationToken);

        /// <summary> Invalidates every pending token of the user, of any type (decision D9). </summary>
        Task RevokeAllActiveByUserAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken);
    }
}
