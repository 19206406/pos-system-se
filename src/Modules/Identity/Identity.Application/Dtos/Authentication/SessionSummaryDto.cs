namespace Identity.Application.Dtos.Authentication
{
    public sealed record SessionSummaryDto(
        Guid Id,
        string? DeviceInfo,
        string? IpAddress,
        DateTime CreatedAt,
        DateTime ExpiresAt);
}
