using Identity.Application.Contracts.Security;
using System.Security.Cryptography;

namespace Identity.Infrastructure.Security
{
    public sealed class RandomTokenGenerator : ISecureTokenGenerator
    {
        private const int TokenSizeInBytes = 32; 

        public string GenerateToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);
            return Convert.ToHexString(bytes); 
        }
    }
}
