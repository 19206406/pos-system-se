namespace Identity.Application.Features.Users.Commands.UpdateUser
{
    public sealed record UpdateUserResponse(
        Guid Id, string FullName, string JobTitle,
        string? PhoneNumber, DateTime UpdatedAt); 
}
