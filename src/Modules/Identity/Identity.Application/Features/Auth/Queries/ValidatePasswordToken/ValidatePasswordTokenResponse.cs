namespace Identity.Application.Features.Auth.Queries.ValidatePasswordToken
{
    public record ValidatePasswordTokenResponse(
        Guid Id, 
        string TokenType, 
        DateTimeOffset CreatedAt, 
        DateTimeOffset ExpiresAt); 
}
