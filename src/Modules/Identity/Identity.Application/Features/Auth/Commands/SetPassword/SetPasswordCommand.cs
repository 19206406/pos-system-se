using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.SetPassword
{
    // One action for invite and reset. The token alone identifies the user: there is no email on purpose.
    public sealed record SetPasswordCommand(string Token, string NewPassword) : ICommand;
}
