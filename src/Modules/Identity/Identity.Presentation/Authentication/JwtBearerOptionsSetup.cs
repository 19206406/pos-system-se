using Identity.Application.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Constants.Claims;
using System.Text;

namespace Identity.Presentation.Authentication
{
    /// <summary>
    /// Validates access tokens with the same JwtOptions the generator signs with (spec section 3):
    /// issuer, audience, signature, expiration and a pinned HS256 algorithm, without touching the database.
    /// </summary>
    internal sealed class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly JwtOptions _jwtOptions;

        public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        public void Configure(string? name, JwtBearerOptions options)
        {
            if (name != JwtBearerDefaults.AuthenticationScheme) return;
            Configure(options);
        }

        public void Configure(JwtBearerOptions options)
        {
            // Claims keep their original names ("sub", "sid", "role", "permission")
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey)),
                // Pinned: a token signed with any other algorithm (including "none") is rejected
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                RequireSignedTokens = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.FromSeconds(_jwtOptions.ClockSkewSeconds),
                NameClaimType = "name",
                RoleClaimType = AuthClaimTypes.Role
            };
        }
    }
}
