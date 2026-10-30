using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.LogoutUser
{
    public sealed record LogoutUserCommand(Guid Id, string RefreshToken) : ICommand;
}
