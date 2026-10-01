using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.SetUserPassword
{
    public record SetUserPasswordCommand(string Password, string VerificationPassword, string Email)
        : ICommand<SetUserPasswordResponseDto>; 
}
