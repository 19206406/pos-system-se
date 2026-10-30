namespace Identity.Application.Features.Auth.Queries.GetMySessions
{
    public sealed record GetMySessionsResponse(IReadOnlyList<MySessionItem> Sessions);

    // LastRefreshedAt: cada rotación crea una fila nueva, así que CreatedAt de la sesión activa
    // es el momento del último login o refresh de ese dispositivo.
    public sealed record MySessionItem(
        Guid Id,
        string? DeviceInfo,
        string? IpAddress,
        DateTime LastRefreshedAt,
        DateTime ExpiresAt,
        bool IsCurrent);
}
