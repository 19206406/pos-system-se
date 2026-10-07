namespace Identity.Application.Dtos.Authentication
{
    public sealed record AuthTokensDto(
        string AccessToken, DateTime AccessTokenExpiresAt,
        string RefreshToken, DateTime RefreshTokenExpiresAt); 
}
