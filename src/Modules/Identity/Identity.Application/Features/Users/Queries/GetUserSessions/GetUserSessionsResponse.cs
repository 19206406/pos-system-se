namespace Identity.Application.Features.Users.Queries.GetUserSessions
{
    public sealed record GetUserSessionsResponse(IReadOnlyList<UserSessionItem> Sessions);

    // Every rotation creates a new row, so CreatedAt of the active session is the last login or refresh of that device
    public sealed record UserSessionItem(
        Guid Id,
        string? DeviceInfo,
        string? IpAddress,
        DateTime LastRefreshedAt,
        DateTime ExpiresAt);
}
