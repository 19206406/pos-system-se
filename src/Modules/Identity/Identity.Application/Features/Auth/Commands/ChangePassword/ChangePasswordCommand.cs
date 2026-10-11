using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.ChangePassword
{
    internal sealed record ChangePasswordCommand(string Email, string Password, string Token)
        : ICommand<SetPasswordResponseDto>;
}
