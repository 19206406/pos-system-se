using Shared.CQRS;

namespace Identity.Application.Features.Users.Commands.CreateUser
{
    // No password: the user sets it through the invite token
    public sealed record CreateUserCommand(string FullName, string Email, string PhoneNumber, string JobTitle)
        : ICommand<CreateUserResponse>;
}
