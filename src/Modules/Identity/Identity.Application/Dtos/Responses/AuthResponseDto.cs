namespace Identity.Application.Dtos.Responses
{
    public record AuthResponseDto(
        string FullName, 
        string Email,
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt, 
        bool Success); 
}
