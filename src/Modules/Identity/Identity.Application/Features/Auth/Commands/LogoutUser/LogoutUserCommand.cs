using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.LogoutUser
{
    // Anonymous: the refresh token alone identifies the session, so logout works after the access token expired
    public sealed record LogoutUserCommand(string RefreshToken) : ICommand;
}
