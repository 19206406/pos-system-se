using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.SetPassword
{
    public sealed record SetPasswordCommand(string Email, string Password, string Token)
        : ICommand<SetPasswordResponseDto>; 
}
