using Identity.Application.Dtos.Authentication;
using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Services
{
    public interface IAuthSessionService
    {
        Task<AuthTokensDto> CreateSessionAsync(User user, ClientContext client, CancellationToken cancellationToken);
        Task<AuthTokensDto> RefreshSessionAsync(string refreshToken, ClientContext client, CancellationToken cancellationToken);
        Task RevokeSessionAsync(string refreshToken, CancellationToken cancellationToken);
        Task RevokeAllUserSessionAsync(Guid userId, CancellationToken cancellationToken);
        Task RevokeOtherUserSessionsAsync(Guid userId, Guid currentSessionId, CancellationToken cancellationToken);
        Task<bool> RevokeSessionByIdAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken); 
    }
}
