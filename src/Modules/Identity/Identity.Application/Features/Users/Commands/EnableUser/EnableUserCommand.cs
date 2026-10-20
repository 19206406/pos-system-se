using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.EnableUser
{
    internal sealed record EnableUserCommand(Guid userId) : ICommand; 
}
