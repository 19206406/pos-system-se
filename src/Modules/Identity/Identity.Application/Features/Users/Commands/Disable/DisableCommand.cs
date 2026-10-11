using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.Disable
{
    internal sealed record DisableCommand(Guid userId) : ICommand; 
}
