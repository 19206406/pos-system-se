namespace Identity.Presentation.Requests.Users
{
    public sealed record UpdateUserRequest(string FullName, string JobTitle, string? PhoneNumber);
}
