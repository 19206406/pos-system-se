using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.LogoutSession
{
    public sealed record LogoutSessionCommand(Guid Id, string RefreshToken) : ICommand;
}
