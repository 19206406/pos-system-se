using Identity.Application.DTOs.Responses;
using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.RegisterUser
{
    public record RegisterUserCommand(string FullName, string Email, string PhoneNumber, string Position) 
        : ICommand<RegisterUserResponseDto>; 
}
