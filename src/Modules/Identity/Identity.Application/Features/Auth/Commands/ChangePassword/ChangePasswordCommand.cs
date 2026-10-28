using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.ChangePassword
{
    internal sealed record ChangePasswordCommand(
        Guid UserId, 
        Guid CurrentSessionId, 
        string CurrentPassword, 
        string NewPassword): ICommand;
}
