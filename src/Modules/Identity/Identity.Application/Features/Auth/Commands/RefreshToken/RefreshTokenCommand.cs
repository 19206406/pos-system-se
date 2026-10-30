using Identity.Application.Dtos.Authentication;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.RefreshToken
{
    public sealed record RefreshTokenCommand(string RefreshToken, ClientContext Client) 
        : ICommand<RefreshTokenResponse>;
}
