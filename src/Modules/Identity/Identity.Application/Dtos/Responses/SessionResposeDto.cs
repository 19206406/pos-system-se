namespace Identity.Application.Dtos.Responses
{
    public sealed record SessionResposeDto(
        Guid Id,
        Guid UserId,
        string? DeviceInfo,
        string? IpAddress,
        DateTime CreatedAt,
        DateTime ExpiresAt);
}
