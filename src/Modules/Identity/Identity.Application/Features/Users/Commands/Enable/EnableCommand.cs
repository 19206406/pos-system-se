using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.Enable
{
    internal sealed record EnableCommand(Guid userId) : ICommand; 
}
