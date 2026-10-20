using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.ResendInvite
{
    public record ResendInviteCommand(Guid Id) : ICommand; 
}
