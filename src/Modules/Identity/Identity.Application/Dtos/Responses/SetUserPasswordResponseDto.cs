namespace Identity.Application.Dtos.Responses
{
    public record SetUserPasswordResponseDto(string FullName, string TokenType, DateTimeOffset UsedAt); 
}
