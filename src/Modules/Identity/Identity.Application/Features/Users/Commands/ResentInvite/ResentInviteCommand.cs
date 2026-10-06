using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.ResentInvite
{
    public record ResentInviteCommand(Guid Id) : ICommand; 
}
