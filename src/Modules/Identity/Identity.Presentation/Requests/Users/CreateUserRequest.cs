namespace Identity.Presentation.Requests.Users
{
    public sealed record CreateUserRequest(
        string FullName,
        string Email,
        string PhoneNumber,
        string JobTitle);
}
