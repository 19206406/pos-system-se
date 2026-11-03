using Identity.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Identity.IntegrationTests
{
    /// <summary> Spec section 9.2: sensitive public endpoints are rate limited and answer 429 as ProblemDetails. </summary>
    public sealed class RateLimitingTests(PostgresFixture fixture)
    {
        [Fact]
        public async Task Login_beyond_the_limit_gets_429_with_retry_after()
        {
            await using var api = fixture.CreateApi(new Dictionary<string, string?>
            {
                ["IdentityRateLimits:CredentialsPermitLimit"] = "2",
                ["IdentityRateLimits:CredentialsWindowSeconds"] = "60"
            });
            var client = api.CreateClient();
            var body = new { email = "not-an-email", password = "x" };

            var first = await client.PostAsJsonAsync("/api/identity/auth/login", body);
            var second = await client.PostAsJsonAsync("/api/identity/auth/login", body);
            var third = await client.PostAsJsonAsync("/api/identity/auth/login", body);

            Assert.Equal(HttpStatusCode.BadRequest, first.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
            Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
            Assert.True(third.Headers.Contains("Retry-After"));
            Assert.Equal("Too many requests.", (await ProblemResponse.ReadAsync(third)).Title);
        }
    }
}
