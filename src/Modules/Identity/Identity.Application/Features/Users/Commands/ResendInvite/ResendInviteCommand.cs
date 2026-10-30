using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.ResendInvite
{
    public sealed record ResendInviteCommand(Guid Id) : ICommand; 
}
