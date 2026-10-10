using Identity.Application.Dtos.Authentication;
using Identity.Domain.Entities;

namespace Identity.Application.Contracts.Services
{
    public interface IAuthSessionService
    {
        Task<AuthTokensDto> CreateSessionAsync(User user, ClientContext clientContext, CancellationToken cancellationToken);
        Task<AuthTokensDto> RefreshSessionAsync(string refreshToken, ClientContext clientContext, CancellationToken cancellationToken);
        Task RevokeSessionAsync(string refreshToken, CancellationToken cancellationToken);
        Task RevokeAllUserSessionAsync(Guid userId, CancellationToken cancellationToken); 
    }
}
