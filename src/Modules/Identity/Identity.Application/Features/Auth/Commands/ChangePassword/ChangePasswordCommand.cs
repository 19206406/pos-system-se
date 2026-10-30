using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.ChangePassword
{
    public sealed record ChangePasswordCommand(
        Guid UserId,
        Guid CurrentSessionId,
        string CurrentPassword,
        string NewPassword) : ICommand;
}
