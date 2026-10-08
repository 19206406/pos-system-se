using Identity.Application.Contracts.Authentication;
using Identity.Application.Dtos.Authentication;
using Identity.Application.Options;
using Microsoft.Extensions.Options;
using System.Buffers.Text;
using System.Security.Cryptography;

namespace Identity.Infrastructure.Security
{
    internal sealed class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        private readonly JwtOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly Sha256TokenHasher _tokenHasher;
        private const int TokenSizeInBytes = 64;

        public RefreshTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider, Sha256TokenHasher tokenHasher)
        {
            _options = options.Value;
            _timeProvider = timeProvider;
            _tokenHasher = tokenHasher;
        }

        public GeneratedRefreshToken Generate()
        {
            var plainText = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenSizeInBytes));
            var expiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(_options.SessionAbsoluteLifetimeDays);

            return new GeneratedRefreshToken(plainText, _tokenHasher.Hash(plainText), expiresAt); 
        }
    }
}
