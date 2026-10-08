using Identity.Application.Contracts.Authentication;
using Identity.Application.Dtos.Authentication;
using Identity.Application.Options;
using Identity.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shared.Constants.Claims;
using System.Security.Claims;
using System.Text;

namespace Identity.Infrastructure.Security
{
    public sealed class AccessTokenGenerator : IAccessTokenGenerator
    {
        private readonly JwtOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly JsonWebTokenHandler _handler = new();
        private readonly SigningCredentials _credentials; 

        public AccessTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider)
        {
            _options = options.Value;
            _timeProvider = timeProvider;
            _credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)), SecurityAlgorithms.HmacSha256); 
        }

        public AccessToken Generate(User user, Guid sessionId, UserAccessDto access)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var expiresAt = now.AddMinutes(_options.AccessTokeLifetimeMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Name, user.FullName),
                new(AuthClaimTypes.SessionsId, sessionId.ToString())
            };

            claims.AddRange(access.Roles.Select(r => new Claim(AuthClaimTypes.Role, r)));
            claims.AddRange(access.Permissions.Select(p => new Claim(AuthClaimTypes.Permission, p));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                IssuedAt = now,
                NotBefore = now,
                Expires = expiresAt,
                SigningCredentials = _credentials
            };

            return new AccessToken(_handler.CreateToken(descriptor), expiresAt); 
        }
    }
}
