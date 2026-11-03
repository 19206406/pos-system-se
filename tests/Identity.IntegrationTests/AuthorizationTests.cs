using Identity.IntegrationTests.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Identity.Application.Authorization;
using System.Net;

namespace Identity.IntegrationTests
{
    /// <summary> Spec section 10, "Autorización" (token validation, permission policies and the fallback policy). </summary>
    public sealed class AuthorizationTests(PostgresFixture fixture)
    {
        private readonly IdentityApiFactory _api = fixture.Api;

        [Fact]
        public async Task Without_token_is_401_without_permission_is_403_and_with_permission_succeeds()
        {
            var plainUser = await TestUsers.CreateAsync(_api);
            var admin = await TestUsers.CreateAsync(_api, isAdmin: true);
            var plainLogin = await TestUsers.LoginAsync(_api, plainUser);
            var adminLogin = await TestUsers.LoginAsync(_api, admin);

            var anonymous = await _api.CreateClient().GetAsync("/api/identity/users");
            var forbidden = await TestUsers.CreateClient(_api, plainLogin.AccessToken).GetAsync("/api/identity/users");
            var allowed = await TestUsers.CreateClient(_api, adminLogin.AccessToken).GetAsync("/api/identity/users");

            Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
            Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);

            // 401 and 403 from the middleware come out as ProblemDetails like every other error
            Assert.Equal("application/problem+json", anonymous.Content.Headers.ContentType?.MediaType);
            Assert.Equal("application/problem+json", forbidden.Content.Headers.ContentType?.MediaType);
        }

        public static TheoryData<string> InvalidTokens => new()
        {
            { TestTokens.Create(expiresAt: DateTime.UtcNow.AddMinutes(-5), permissions: IdentityPermissionCodes.Users.Read) },
            { TestTokens.Create(audience: "Some.Other.Audience", permissions: IdentityPermissionCodes.Users.Read) },
            { TestTokens.Create(algorithm: SecurityAlgorithms.HmacSha512, permissions: IdentityPermissionCodes.Users.Read) },
            { TestTokens.Create(signed: false, permissions: IdentityPermissionCodes.Users.Read) }
        };

        [Theory]
        [MemberData(nameof(InvalidTokens))]
        public async Task Expired_foreign_audience_other_algorithm_or_unsigned_tokens_get_401(string token)
        {
            var response = await TestUsers.CreateClient(_api, token).GetAsync("/api/identity/users");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task A_hand_made_token_with_the_right_key_and_permission_is_accepted()
        {
            // Control case for the theory above: same builder, valid values
            var token = TestTokens.Create(permissions: IdentityPermissionCodes.Users.Read);

            var response = await TestUsers.CreateClient(_api, token).GetAsync("/api/identity/users");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task An_endpoint_without_attributes_requires_authentication()
        {
            var user = await TestUsers.CreateAsync(_api);
            var login = await TestUsers.LoginAsync(_api, user);

            var anonymous = await _api.CreateClient().GetAsync("/test/probe");
            var authenticated = await TestUsers.CreateClient(_api, login.AccessToken).GetAsync("/test/probe");

            Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
            Assert.Equal(HttpStatusCode.OK, authenticated.StatusCode);
        }
    }
}
