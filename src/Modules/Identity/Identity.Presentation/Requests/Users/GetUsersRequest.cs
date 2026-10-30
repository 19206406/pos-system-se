namespace Identity.Presentation.Requests.Users
{
    public sealed record GetUsersRequest(string? Search, bool? IsActive, int Page = 1, int PageSize = 20);
}
