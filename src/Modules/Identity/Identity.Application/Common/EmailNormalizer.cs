namespace Identity.Application.Common
{
    /// <summary>
    /// Emails are stored and looked up in one canonical form so "Ana@x.com" and "ana@x.com"
    /// are the same account and the unique index on users.email holds case-insensitively.
    /// </summary>
    public static class EmailNormalizer
    {
        public static string Normalize(string email) => email.Trim().ToLowerInvariant();
    }
}
