namespace Identity.Application.Dtos.Responses
{
    public record SetPasswordResponseDto(
        string FullName, string Token, 
        string TokenType, DateTimeOffset UsedAt); 
}
