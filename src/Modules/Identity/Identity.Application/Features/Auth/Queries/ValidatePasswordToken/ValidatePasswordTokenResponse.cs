namespace Identity.Application.Features.Auth.Queries.ValidatePasswordToken
{
    /// <summary> TokenType ("invite" or "reset") is null when IsValid is false. No user data on purpose. </summary>
    public sealed record ValidatePasswordTokenResponse(bool IsValid, string? TokenType)
    {
        public static ValidatePasswordTokenResponse Invalid { get; } = new(false, null);
    }
}
