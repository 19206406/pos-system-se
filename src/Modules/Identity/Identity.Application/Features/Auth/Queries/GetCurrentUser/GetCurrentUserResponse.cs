namespace Identity.Application.Features.Auth.Queries.GetCurrentUser
{
    public sealed record GetCurrentUserResponse(
        Guid Id,
        string FullName,
        string Email,
        string? PhoneNumber,
        string JobTitle);
}
