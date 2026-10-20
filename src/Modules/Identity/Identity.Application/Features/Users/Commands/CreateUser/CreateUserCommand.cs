using Identity.Application.DTOs.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.CreateUser
{
    public record CreateUserCommand(string FullName, string Email, string PhoneNumber, string Position) 
        : ICommand<RegisterUserResponseDto>; 
}
