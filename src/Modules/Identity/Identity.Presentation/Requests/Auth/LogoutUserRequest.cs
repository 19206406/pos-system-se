namespace Identity.Presentation.Requests.Auth
{
    // Nullable on purpose: [ApiController] would turn a missing non-nullable string into a 400, and logout always answers 204
    public sealed record LogoutUserRequest(string? RefreshToken);
}
