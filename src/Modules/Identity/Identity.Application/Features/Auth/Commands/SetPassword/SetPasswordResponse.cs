namespace Identity.Application.Features.Auth.Commands.SetPassword
{
    public record SetPasswordResponse(
        string FullName, string Token, 
        string TokenType, DateTimeOffset UsedAt); 
}
