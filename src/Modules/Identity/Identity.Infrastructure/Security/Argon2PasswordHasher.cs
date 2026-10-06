using Identity.Application.Contracts.Security;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Identity.Infrastructure.Security
{
    public class Argon2PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 4;
        private const int KbMemory = 65536;
        private const int Parallelism = 2;

        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Calculate(password, salt, Iterations, KbMemory, Parallelism);

            return $"argon2id${Iterations}${KbMemory}${Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string storedHash)
        {
            var p = storedHash.Split("$");
            var salt = Convert.FromBase64String(p[4]);
            var expected = Convert.FromBase64String(p[5]);
            var current = Calculate(password, salt, int.Parse(p[1]), int.Parse(p[2]), int.Parse(p[3]));

            return CryptographicOperations.FixedTimeEquals(current, expected);
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
    }
}

