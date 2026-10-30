namespace Identity.Application.Dtos.Users
{
    public sealed record UserProfileDto(
        Guid Id,
        string FullName,
        string Email,
        string? PhoneNumber,
        string JobTitle,
        bool IsActive,
        DateTime CreatedAt);
}
