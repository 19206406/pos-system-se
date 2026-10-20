using Identity.Application.DTOs.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.Login
{
    public record LoginUserCommand(string Email, string Password) : ICommand<AuthResponseDto>; 
}
