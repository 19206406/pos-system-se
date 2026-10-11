namespace Identity.Application.DTOs.Responses
{
    public record LoginUserResponseDto(
        string FullName, 
        string Email,
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt, 
        bool Success); 
}
