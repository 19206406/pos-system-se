using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.LogoutSession
{
    internal record LogoutSessionCommand(Guid Id, string RefreshToken) : ICommand; 
}
