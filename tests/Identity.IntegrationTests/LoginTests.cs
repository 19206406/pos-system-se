using Identity.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Identity.IntegrationTests
{
    /// <summary> Spec section 10, "Login y sesiones". </summary>
    public sealed class LoginTests(PostgresFixture fixture)
    {
        private readonly IdentityApiFactory _api = fixture.Api;

        [Fact]
        public async Task Login_with_valid_credentials_returns_tokens_and_stores_only_the_refresh_token_hash()
        {
            var user = await TestUsers.CreateAsync(_api);

            var tokens = await TestUsers.LoginAsync(_api, user);

            Assert.False(string.IsNullOrEmpty(tokens.AccessToken));
            Assert.False(string.IsNullOrEmpty(tokens.RefreshToken));

            var session = await TestUsers.GetSessionByRefreshTokenAsync(_api, tokens.RefreshToken);
            Assert.Equal(user.Id, session.UserId);
            Assert.NotEqual(tokens.RefreshToken, session.TokenHash);
            Assert.Equal(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tokens.RefreshToken))), session.TokenHash);
        }

        [Fact]
        public async Task Login_response_is_not_cacheable()
        {
            var user = await TestUsers.CreateAsync(_api);

            var response = await _api.CreateClient().PostAsJsonAsync(
                "/api/identity/auth/login", new { email = user.Email, password = user.Password });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.CacheControl?.NoStore);
        }

        [Fact]
        public async Task Login_failures_are_indistinguishable_whatever_the_cause()
        {
            var withPassword = await TestUsers.CreateAsync(_api);
            var withoutPassword = await TestUsers.CreateAsync(_api, password: null);
            var inactive = await TestUsers.CreateAsync(_api, isActive: false);
            var client = _api.CreateClient();

            async Task<ProblemResponse> Attempt(string email, string password) =>
                await ProblemResponse.ReadAsync(await client.PostAsJsonAsync(
                    "/api/identity/auth/login", new { email, password }));

            var wrongPassword = await Attempt(withPassword.Email, "not-the-password");
            var unknownEmail = await Attempt($"nobody-{Guid.NewGuid():N}@test.local", "whatever-password");
            var noPassword = await Attempt(withoutPassword.Email, "whatever-password");
            var inactiveUser = await Attempt(inactive.Email, TestUsers.DefaultPassword);

            Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.Status);
            Assert.Equal(wrongPassword, unknownEmail);
            Assert.Equal(wrongPassword, noPassword);
            Assert.Equal(wrongPassword, inactiveUser);
        }

        [Fact]
        public async Task Login_email_is_case_insensitive()
        {
            var user = await TestUsers.CreateAsync(_api);

            var response = await _api.CreateClient().PostAsJsonAsync(
                "/api/identity/auth/login", new { email = user.Email.ToUpperInvariant(), password = user.Password });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Health_check_is_anonymous()
        {
            var response = await _api.CreateClient().GetAsync("/health");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
