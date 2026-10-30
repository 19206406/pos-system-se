using Identity.Application.Contracts.Security;
using Identity.Application.Dtos.Authentication;
using Identity.Application.Options;
using Identity.Domain.Constants;
using Identity.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Security
{
    internal sealed class PasswordTokenIssuer : IPasswordTokenIssuer
    {
        private readonly ISecureTokenGenerator _tokenGenerator;
        private readonly ITokenHasher _tokenHasher;
        private readonly PasswordTokenOptions _options;

        public PasswordTokenIssuer(ISecureTokenGenerator tokenGenerator, ITokenHasher tokenHasher, IOptions<PasswordTokenOptions> options)
        {
            _tokenGenerator = tokenGenerator;
            _tokenHasher = tokenHasher;
            _options = options.Value;
        }

        public IssuedPasswordToken IssueInvite(DateTime nowUtc) =>
            Issue(PasswordTokenTypes.Invite, nowUtc, nowUtc.AddHours(_options.InviteLifetimeHours));

        public IssuedPasswordToken IssueReset(DateTime nowUtc) =>
            Issue(PasswordTokenTypes.Reset, nowUtc, nowUtc.AddMinutes(_options.ResetLifetimeMinutes));

        private IssuedPasswordToken Issue(string tokenType, DateTime nowUtc, DateTime expiresAtUtc)
        {
            var plainText = _tokenGenerator.GenerateToken();

            var entity = new PasswordToken
            {
                TokenHash = _tokenHasher.Hash(plainText),
                TokenType = tokenType,
                CreatedAt = nowUtc,
                ExpiresAt = expiresAtUtc
            };

            return new IssuedPasswordToken(entity, plainText);
        }
    }
}
