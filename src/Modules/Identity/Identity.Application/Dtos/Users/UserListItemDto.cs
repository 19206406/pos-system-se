namespace Identity.Application.Dtos.Users
{
    public sealed record UserListItemDto(
        Guid Id,
        string FullName,
        string Email,
        string JobTitle,
        bool IsActive,
        DateTime CreatedAt);
}
