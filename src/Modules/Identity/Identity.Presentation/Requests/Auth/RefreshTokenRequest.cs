namespace Identity.Presentation.Requests.Auth
{
    // Nullable on purpose: a missing token must get the same generic 401 as any other refresh failure, not a 400
    public sealed record RefreshTokenRequest(string? RefreshToken);
}
