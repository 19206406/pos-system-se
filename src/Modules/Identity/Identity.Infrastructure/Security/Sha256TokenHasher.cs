using Identity.Application.Contracts.Security;
using System.Security.Cryptography;
using System.Text;

namespace Identity.Infrastructure.Security
{
    public sealed class Sha256TokenHasher : ITokenHasher
    {
        public string Hash(string plainText)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(plainText);

            var inputBytes = Encoding.UTF8.GetBytes(plainText);
            var hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes); 
        }

        public bool Verify(string plainText, string expectedHash)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
            ArgumentException.ThrowIfNullOrWhiteSpace(expectedHash);

            var computedHash = Hash(plainText);

            var computedBytes = Convert.FromHexString(computedHash);
            var expectedBytes = Convert.FromHexString(expectedHash);

            return CryptographicOperations.FixedTimeEquals(computedBytes, expectedBytes); 
        }
    }
}
