namespace Identity.Presentation.Authentication
{
    /// <summary> Permits per window for each IdentityRateLimitPolicies entry, partitioned by client IP. </summary>
    public sealed class IdentityRateLimitOptions
    {
        public const string SectionName = "IdentityRateLimits";

        public int CredentialsPermitLimit { get; init; } = 5;
        public int CredentialsWindowSeconds { get; init; } = 60;

        public int PasswordTokensPermitLimit { get; init; } = 5;
        public int PasswordTokensWindowSeconds { get; init; } = 900;

        public int RefreshPermitLimit { get; init; } = 30;
        public int RefreshWindowSeconds { get; init; } = 60;
    }
}
