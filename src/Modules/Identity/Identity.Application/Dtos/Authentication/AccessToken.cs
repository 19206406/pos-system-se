namespace Identity.Application.Dtos.Authentication
{
    public sealed record AccessToken(string Value, DateTime ExpiresAt); 
}
