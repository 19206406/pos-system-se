using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.UpdateUser
{
    public sealed record UpdateUserCommand(
        Guid Id, 
        string FullName, 
        string JobTitle, 
        string? PhoneNumber) : ICommand<UpdateUserResponse>; 
}
