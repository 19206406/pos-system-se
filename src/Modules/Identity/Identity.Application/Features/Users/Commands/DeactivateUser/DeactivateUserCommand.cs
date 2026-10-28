using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.DeactivateUser
{
    public sealed record DeactivateUserCommand(Guid UserId, Guid RequestedByUserId) : ICommand; 
}
