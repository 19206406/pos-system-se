namespace Identity.Application.Features.Auth.Commands.LoginUser
{
    public sealed record LoginUserResponse(
        string FullName,
        string Email,
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt)
    {
        public string TokenType => "Bearer";
    }
}
