namespace Identity.Application.Dtos.Authentication
{
    public sealed record GeneratedRefreshToken(string PlainText, string Hash, DateTime ExpiresAt); 
}
