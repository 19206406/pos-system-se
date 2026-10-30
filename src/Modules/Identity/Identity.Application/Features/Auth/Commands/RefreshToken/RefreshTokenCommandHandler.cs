using Identity.Application.Contracts.Persistence;
using Identity.Application.Contracts.Services;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
    {
        private readonly IAuthSessionService _authSessionService;

        public RefreshTokenCommandHandler(IAuthSessionService authSessionService)
        {
            _authSessionService = authSessionService;
        }

        public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var tokens = await _authSessionService.RefreshSessionAsync(command.RefreshToken, command.Client, cancellationToken);

            return new RefreshTokenResponse(
                tokens.AccessToken, tokens.AccessTokenExpiresAt,
                tokens.RefreshToken, tokens.RefreshTokenExpiresAt); 
        }
    }
}
