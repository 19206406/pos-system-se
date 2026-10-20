namespace Identity.Application.Dtos.Responses
{
    internal record SessionResposeDto(
        Guid Id,
        Guid UserId,
        string? DeviceInfo,
        string? IpAddress,
        DateTime CreatedAt,
        DateTime ExpiresAt); 
}
