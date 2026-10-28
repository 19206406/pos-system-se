using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.RevokeUserSessions
{
    public sealed record RevokeUserSessionsCommand(Guid UserId) : ICommand;
}
