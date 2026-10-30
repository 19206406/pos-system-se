namespace Identity.Application.Features.Auth.Commands.LoginUser
{
    public record LoginUserResponse(
        string FullName, 
        string Email,
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt, 
        bool Success); 
}
