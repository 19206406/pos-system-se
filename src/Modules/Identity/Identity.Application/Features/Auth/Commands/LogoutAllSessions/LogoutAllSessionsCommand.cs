using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.LogoutAllSessions
{
    internal record LogoutAllSessionsCommand(Guid Id) : ICommand; 
}
