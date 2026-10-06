using Identity.Application.Dtos.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.SetUserPassword
{
    public record SetUserPasswordCommand(string Email, string Password, string Token)
        : ICommand<SetUserPasswordResponseDto>; 
}
