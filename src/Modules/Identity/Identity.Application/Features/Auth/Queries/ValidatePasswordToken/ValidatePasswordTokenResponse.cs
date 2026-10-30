namespace Identity.Application.Features.Auth.Queries.ValidatePasswordToken
{
    public record ValidatePasswordTokenResponse(
        Guid Id, 
        string TokenType, 
        DateTime CreatedAt, 
        DateTime ExpiresAt); 
}
