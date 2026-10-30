namespace Identity.Presentation.Authentication
{
    /// <summary> Rate limiting policy names used with [EnableRateLimiting] on the sensitive public endpoints. </summary>
    public static class IdentityRateLimitPolicies
    {
        /// <summary> Login, set-password, change-password: guessing a password or a token. </summary>
        public const string Credentials = "identity-credentials";

        /// <summary> Forgot-password and token validation: account and token probing, email flooding. </summary>
        public const string PasswordTokens = "identity-password-tokens";

        /// <summary> Refresh: legitimate clients call it every ~15 minutes per device. </summary>
        public const string Refresh = "identity-refresh";
    }
}
