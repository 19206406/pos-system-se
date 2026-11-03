using Identity.Application.Features.Auth.Queries.GetMySessions;
using Identity.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Identity.IntegrationTests
{
    /// <summary> Spec section 10, "Logout y revocaciones". </summary>
    public sealed class SessionTests(PostgresFixture fixture)
    {
        private readonly IdentityApiFactory _api = fixture.Api;

        [Fact]
        public async Task Logout_is_idempotent_and_revokes_the_session()
        {
            var user = await TestUsers.CreateAsync(_api);
            var login = await TestUsers.LoginAsync(_api, user);
            var client = _api.CreateClient();

            var first = await client.PostAsJsonAsync("/api/identity/auth/logout", new { refreshToken = login.RefreshToken });
            var second = await client.PostAsJsonAsync("/api/identity/auth/logout", new { refreshToken = login.RefreshToken });
            var unknown = await client.PostAsJsonAsync("/api/identity/auth/logout", new { refreshToken = "never-issued" });

            Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, unknown.StatusCode);
            Assert.NotNull((await TestUsers.GetSessionByRefreshTokenAsync(_api, login.RefreshToken)).RevokedAt);
        }

        [Fact]
        public async Task Logout_all_leaves_no_active_session()
        {
            var user = await TestUsers.CreateAsync(_api);
            var current = await TestUsers.LoginAsync(_api, user);
            await TestUsers.LoginAsync(_api, user);
            await TestUsers.LoginAsync(_api, user);

            var response = await TestUsers.CreateClient(_api, current.AccessToken).PostAsync("/api/identity/auth/logout-all", null);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(0, await TestUsers.CountActiveSessionsAsync(_api, user.Id));
        }

        [Fact]
        public async Task Revoking_a_session_of_another_user_returns_404_and_leaves_it_active()
        {
            var attacker = await TestUsers.CreateAsync(_api);
            var victim = await TestUsers.CreateAsync(_api);
            var attackerLogin = await TestUsers.LoginAsync(_api, attacker);
            var victimLogin = await TestUsers.LoginAsync(_api, victim);
            var victimSession = await TestUsers.GetSessionByRefreshTokenAsync(_api, victimLogin.RefreshToken);
            var attackerClient = TestUsers.CreateClient(_api, attackerLogin.AccessToken);

            var foreign = await attackerClient.DeleteAsync($"/api/identity/auth/sessions/{victimSession.Id}");
            var missing = await attackerClient.DeleteAsync($"/api/identity/auth/sessions/{Guid.NewGuid()}");
            var ownSession = await TestUsers.GetSessionByRefreshTokenAsync(_api, attackerLogin.RefreshToken);
            await attackerClient.DeleteAsync($"/api/identity/auth/sessions/{ownSession.Id}");
            var alreadyRevoked = await attackerClient.DeleteAsync($"/api/identity/auth/sessions/{ownSession.Id}");

            Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
            Assert.Equal(await ProblemResponse.ReadAsync(missing), await ProblemResponse.ReadAsync(foreign));
            Assert.Equal(await ProblemResponse.ReadAsync(missing), await ProblemResponse.ReadAsync(alreadyRevoked));
            Assert.Null((await TestUsers.GetSessionByRefreshTokenAsync(_api, victimLogin.RefreshToken)).RevokedAt);
        }

        [Fact]
        public async Task Revoking_my_own_session_works()
        {
            var user = await TestUsers.CreateAsync(_api);
            var current = await TestUsers.LoginAsync(_api, user);
            var other = await TestUsers.LoginAsync(_api, user);
            var otherSession = await TestUsers.GetSessionByRefreshTokenAsync(_api, other.RefreshToken);

            var response = await TestUsers.CreateClient(_api, current.AccessToken)
                .DeleteAsync($"/api/identity/auth/sessions/{otherSession.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.NotNull((await TestUsers.GetSessionByRefreshTokenAsync(_api, other.RefreshToken)).RevokedAt);
        }

        [Fact]
        public async Task Get_my_sessions_flags_the_current_one_and_never_exposes_token_hashes()
        {
            var user = await TestUsers.CreateAsync(_api);
            var first = await TestUsers.LoginAsync(_api, user);
            var current = await TestUsers.LoginAsync(_api, user);
            var currentSession = await TestUsers.GetSessionByRefreshTokenAsync(_api, current.RefreshToken);
            var firstSession = await TestUsers.GetSessionByRefreshTokenAsync(_api, first.RefreshToken);

            var response = await TestUsers.CreateClient(_api, current.AccessToken).GetAsync("/api/identity/auth/sessions");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var raw = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("hash", raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(currentSession.TokenHash, raw);
            Assert.DoesNotContain(firstSession.TokenHash, raw);

            var body = (await response.Content.ReadFromJsonAsync<GetMySessionsResponse>())!;
            Assert.Equal(2, body.Sessions.Count);
            Assert.True(body.Sessions.Single(s => s.Id == currentSession.Id).IsCurrent);
            Assert.False(body.Sessions.Single(s => s.Id == firstSession.Id).IsCurrent);
        }
    }
}
