using Identity.Application.Contracts.Security;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Identity.Infrastructure.Security
{
    public sealed class Argon2PasswordHasher : IPasswordHasher
    {
        private const string Algorithm = "argon2id";
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 4;
        private const int KbMemory = 65536;
        private const int Parallelism = 2;

        // Same parameters as a real hash, so verifying against it costs the same as a real verification
        private static readonly Lazy<string> DummyHash = new(() => HashCore(Convert.ToHexString(RandomNumberGenerator.GetBytes(32))));

        public string Hash(string password) => HashCore(password);

        public bool Verify(string password, string? storedHash)
        {
            if (storedHash is null || !TryParse(storedHash, out var stored))
            {
                VerifyCore(password, ParseDummy());
                return false;
            }

            return VerifyCore(password, stored);
        }

        private static string HashCore(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Calculate(password, salt, Iterations, KbMemory, Parallelism);

            return $"{Algorithm}${Iterations}${KbMemory}${Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        private static bool VerifyCore(string password, StoredHash stored)
        {
            var current = Calculate(password, stored.Salt, stored.Iterations, stored.KbMemory, stored.Parallelism);
            return CryptographicOperations.FixedTimeEquals(current, stored.Hash);
        }

        private static StoredHash ParseDummy() =>
            TryParse(DummyHash.Value, out var dummy)
                ? dummy
                : throw new InvalidOperationException("The dummy Argon2 hash could not be parsed.");

        /// <summary> Format: argon2id$iterations$memoryKb$parallelism$saltBase64$hashBase64 </summary>
        private static bool TryParse(string storedHash, out StoredHash stored)
        {
            stored = default;
            var parts = storedHash.Split('$');

            if (parts.Length != 6 || parts[0] != Algorithm
                || !int.TryParse(parts[1], out var iterations)
                || !int.TryParse(parts[2], out var kbMemory)
                || !int.TryParse(parts[3], out var parallelism))
                return false;

            var salt = new byte[parts[4].Length];
            var hash = new byte[parts[5].Length];
            if (!Convert.TryFromBase64String(parts[4], salt, out var saltLength)
                || !Convert.TryFromBase64String(parts[5], hash, out var hashLength))
                return false;

            stored = new StoredHash(iterations, kbMemory, parallelism, salt[..saltLength], hash[..hashLength]);
            return true;
        }

        private static byte[] Calculate(string password, byte[] salt, int iter, int memKb, int parallelism)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                Iterations = iter,
                MemorySize = memKb,
                DegreeOfParallelism = parallelism
            };

            return argon2.GetBytes(HashSize);
        }

        private readonly record struct StoredHash(int Iterations, int KbMemory, int Parallelism, byte[] Salt, byte[] Hash);
    }
}
