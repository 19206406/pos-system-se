namespace Identity.Application.Dtos.Users
{
    public sealed record UserPageDto(IReadOnlyList<UserListItemDto> Items, int TotalCount);
}
