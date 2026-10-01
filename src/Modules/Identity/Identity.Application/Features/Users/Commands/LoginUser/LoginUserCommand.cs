using Identity.Application.DTOs.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.LoginUser
{
    public record LoginUserCommand(string Email, string Password) : ICommand<LoginUserResponseDto>; 
}
