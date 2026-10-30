namespace Identity.Application.Features.Users.Queries.GetUserById
{
    public record GetUserByIdResponse(
        Guid Id, string FullName, string? PhoneNumber, 
        string JobTitle, string Email, bool IsActive, DateTime CreatedAt); 
}
