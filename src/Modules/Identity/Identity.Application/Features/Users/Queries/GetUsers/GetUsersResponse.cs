namespace Identity.Application.Features.Users.Queries.GetUsers
{
    public sealed record GetUsersResponse(
        IReadOnlyList<GetUsersItem> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);

    public sealed record GetUsersItem(
        Guid Id,
        string FullName,
        string Email,
        string JobTitle,
        bool IsActive,
        DateTime CreatedAt);
}
