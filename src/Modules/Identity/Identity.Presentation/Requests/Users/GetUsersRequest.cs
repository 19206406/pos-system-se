namespace Identity.Presentation.Requests.Users
{
    /// <param name="Search"> Text to look for in the full name or the email, case insensitive. Optional. </param>
    /// <param name="IsActive"> true for active users only, false for inactive only; omit it to list both. </param>
    /// <param name="Page"> Page number, starting at 1. Defaults to 1. </param>
    /// <param name="PageSize"> Users per page, from 1 to 100. Defaults to 20. </param>
    public sealed record GetUsersRequest(string? Search, bool? IsActive, int Page = 1, int PageSize = 20);
}
