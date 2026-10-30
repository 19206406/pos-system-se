using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.ReactivateUser
{
    public sealed record ReactivateUserCommand(Guid UserId) : ICommand;
}
