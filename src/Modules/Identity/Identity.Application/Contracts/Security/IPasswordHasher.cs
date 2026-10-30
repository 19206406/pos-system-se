namespace Identity.Application.Contracts.Security
{
    public interface IPasswordHasher
    {
        public string Hash(string password);

        /// <summary>
        /// Returns false when <paramref name="storedHash"/> is null or malformed. With a null hash
        /// (unknown user, or user without password) it still runs a full verification against a dummy
        /// hash, so every login failure takes the same time and the cause cannot be inferred.
        /// </summary>
        public bool Verify(string password, string? storedHash);
    }
}
