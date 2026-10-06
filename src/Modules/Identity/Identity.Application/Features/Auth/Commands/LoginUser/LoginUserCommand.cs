using Identity.Application.DTOs.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Auth.Commands.LoginUser
{
    public record LoginUserCommand(string Email, string Password) : ICommand<LoginUserResponseDto>; 
}
