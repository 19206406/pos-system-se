namespace Identity.Application.Features.Auth.Commands.RefreshToken
{
    public sealed record RefreshTokenResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt)
    {
        public string TokenType => "Bearer";
    }
}
