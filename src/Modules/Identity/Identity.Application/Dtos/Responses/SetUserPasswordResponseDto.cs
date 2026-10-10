namespace Identity.Application.Dtos.Responses
{
    public record SetUserPasswordResponseDto(string FullName, string Token, string TokenType, DateTimeOffset UsedAt); 
}
