namespace Identity.Application.Features.Users.Commands.CreateUser
{
    public sealed record CreateUserResponse(
        Guid Id, string FullName, string JobTitle,
        string? PhoneNumber, string Email, DateTime CreatedAt);
}
