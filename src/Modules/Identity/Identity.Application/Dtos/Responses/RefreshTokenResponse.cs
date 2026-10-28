namespace Identity.Application.Dtos.Responses
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
