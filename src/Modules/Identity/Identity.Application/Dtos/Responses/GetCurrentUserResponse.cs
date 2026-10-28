namespace Identity.Application.Dtos.Responses
{
    public sealed record GetCurrentUserResponse(
        Guid Id,
        string FullName,
        string Email,
        string? PhoneNumber,
        string JobTitle);
}
