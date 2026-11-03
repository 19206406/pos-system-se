using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Identity.IntegrationTests.Infrastructure
{
    /// <summary> Hand-made access tokens for the cases the real generator never produces (expired, wrong audience, other algorithm). </summary>
    public static class TestTokens
    {
        public const string Issuer = "PosSystem.Api";
        public const string Audience = "PosSystem.Clients";

        public static string Create(
            string audience = Audience,
            string algorithm = SecurityAlgorithms.HmacSha256,
            DateTime? expiresAt = null,
            bool signed = true,
            params string[] permissions)
        {
            var now = DateTime.UtcNow;
            var expires = expiresAt ?? now.AddMinutes(10);

            var claims = new List<Claim>
            {
                new("sub", Guid.NewGuid().ToString()),
                new("sid", Guid.NewGuid().ToString())
            };
            claims.AddRange(permissions.Select(p => new Claim("permission", p)));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = Issuer,
                Audience = audience,
                IssuedAt = expires.AddMinutes(-15),
                NotBefore = expires.AddMinutes(-15),
                Expires = expires,
                SigningCredentials = signed
                    ? new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(IdentityApiFactory.SigningKey)), algorithm)
                    : null
            };

            return new JsonWebTokenHandler().CreateToken(descriptor);
        }
    }
}
