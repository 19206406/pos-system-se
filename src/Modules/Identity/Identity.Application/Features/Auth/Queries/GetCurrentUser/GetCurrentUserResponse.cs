namespace Identity.Application.Features.Auth.Queries.GetCurrentUser
{
    /// <summary> Roles and Permissions let the frontend decide what to show without decoding the JWT. </summary>
    public sealed record GetCurrentUserResponse(
        Guid Id,
        string FullName,
        string Email,
        string? PhoneNumber,
        string JobTitle,
        IReadOnlyCollection<string> Roles,
        IReadOnlyCollection<string> Permissions);
}
