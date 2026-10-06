namespace Identity.Application.Dtos.Responses
{
    public record GetPasswordTokenResponseDto(
        Guid Id, 
        string TokenType, 
        DateTimeOffset CreatedAt, 
        DateTimeOffset ExpiresAt); 
}
