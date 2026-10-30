using Identity.Application.Dtos.Authentication;
using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Services
{
    /// <summary>
    /// Single owner of the session lifecycle: issues, rotates and revokes refresh tokens.
    /// Handlers use it; controllers and token generators never bypass it.
    /// </summary>
    public interface IAuthSessionService
    {
        Task<AuthTokensDto> CreateSessionAsync(User user, ClientContext client, CancellationToken cancellationToken);

        /// <summary>
        /// Rotates the refresh token. Throws <see cref="Shared.Exceptions.UnauthorizedException"/>
        /// with the same message for every failure (missing, expired, revoked, reused, inactive user, lost race).
        /// </summary>
        Task<AuthTokensDto> RefreshSessionAsync(string refreshToken, ClientContext client, CancellationToken cancellationToken);

        /// <summary> Revokes the session that owns the refresh token. Idempotent: unknown or revoked tokens are ignored. </summary>
        Task RevokeSessionAsync(string refreshToken, CancellationToken cancellationToken);

        /// <summary>
        /// Revokes one session only if it belongs to the user.
        /// false = it does not exist, belongs to someone else or was already revoked.
        /// </summary>
        Task<bool> RevokeSessionByIdAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);

        Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken);

        /// <summary> Revokes every active session of the user except <paramref name="currentSessionId"/>. </summary>
        Task RevokeOtherUserSessionsAsync(Guid userId, Guid currentSessionId, CancellationToken cancellationToken);
    }
}
