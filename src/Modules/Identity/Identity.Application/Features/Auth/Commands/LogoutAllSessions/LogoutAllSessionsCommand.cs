using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.LogoutAllSessions
{
    public sealed record LogoutAllSessionsCommand(Guid UserId) : ICommand; 
}
