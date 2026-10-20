using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.DisableUser
{
    internal sealed record DisableUserCommand(Guid userId) : ICommand; 
}
