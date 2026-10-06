using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.SetUserPassword
{
    public record SetUserPasswordCommand(string Password, string VerificationPassword, string Email)
        : ICommand<SetUserPasswordResponseDto>; 
}
