using Identity.Application.Features.Auth.Commands.RefreshToken;
using Identity.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace Identity.IntegrationTests
{
    /// <summary> Spec section 10, "Refresh". </summary>
    public sealed class RefreshTokenTests(PostgresFixture fixture)
    {
        private readonly IdentityApiFactory _api = fixture.Api;

        private Task<HttpResponseMessage> RefreshAsync(string? refreshToken) =>
            _api.CreateClient().PostAsJsonAsync("/api/identity/auth/refresh", new { refreshToken });

        [Fact]
        public async Task Refresh_rotates_the_token_and_the_new_session_keeps_the_absolute_expiration()
        {
            var user = await TestUsers.CreateAsync(_api);
            var login = await TestUsers.LoginAsync(_api, user);

            var response = await RefreshAsync(login.RefreshToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.CacheControl?.NoStore);
            var refreshed = (await response.Content.ReadFromJsonAsync<RefreshTokenResponse>())!;
            Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);

            var oldSession = await TestUsers.GetSessionByRefreshTokenAsync(_api, login.RefreshToken);
            var newSession = await TestUsers.GetSessionByRefreshTokenAsync(_api, refreshed.RefreshToken);

            Assert.NotNull(oldSession.RevokedAt);
            Assert.Equal(newSession.Id, oldSession.ReplacedById);
            Assert.Null(newSession.RevokedAt);
            Assert.Equal(oldSession.ExpiresAt, newSession.ExpiresAt);
        }

        [Fact]
        public async Task Reusing_a_rotated_token_is_rejected_and_revokes_every_session_of_the_user()
        {
            var user = await TestUsers.CreateAsync(_api);
            var deviceA = await TestUsers.LoginAsync(_api, user);
            await TestUsers.LoginAsync(_api, user); // device B
            Assert.Equal(HttpStatusCode.OK, (await RefreshAsync(deviceA.RefreshToken)).StatusCode);

            var reuse = await RefreshAsync(deviceA.RefreshToken);

            Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
            Assert.Equal(0, await TestUsers.CountActiveSessionsAsync(_api, user.Id));
        }

        [Fact]
        public async Task Using_a_token_revoked_by_logout_is_rejected_but_other_sessions_stay_active()
        {
            var user = await TestUsers.CreateAsync(_api);
            var deviceA = await TestUsers.LoginAsync(_api, user);
            var deviceB = await TestUsers.LoginAsync(_api, user);

            var logout = await _api.CreateClient().PostAsJsonAsync("/api/identity/auth/logout", new { refreshToken = deviceA.RefreshToken });
            Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

            Assert.Equal(HttpStatusCode.Unauthorized, (await RefreshAsync(deviceA.RefreshToken)).StatusCode);
            Assert.Equal(1, await TestUsers.CountActiveSessionsAsync(_api, user.Id));
            Assert.Equal(HttpStatusCode.OK, (await RefreshAsync(deviceB.RefreshToken)).StatusCode);
        }

        [Fact]
        public async Task Expired_unknown_empty_and_inactive_user_tokens_get_the_same_401()
        {
            var expiredUser = await TestUsers.CreateAsync(_api);
            var expired = await TestUsers.LoginAsync(_api, expiredUser);
            await TestUsers.WithDbAsync(_api, db => db.Sessions
                .Where(s => s.UserId == expiredUser.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.ExpiresAt, DateTime.UtcNow.AddMinutes(-1))));

            var inactiveUser = await TestUsers.CreateAsync(_api);
            var inactive = await TestUsers.LoginAsync(_api, inactiveUser);
            await TestUsers.WithDbAsync(_api, db => db.Users
                .Where(u => u.Id == inactiveUser.Id)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.IsActive, false)));

            var expiredResult = await ProblemResponse.ReadAsync(await RefreshAsync(expired.RefreshToken));
            var unknownResult = await ProblemResponse.ReadAsync(await RefreshAsync("not-a-token-we-ever-issued"));
            var emptyResult = await ProblemResponse.ReadAsync(await RefreshAsync(""));
            var missingResult = await ProblemResponse.ReadAsync(await RefreshAsync(null));
            var inactiveResult = await ProblemResponse.ReadAsync(await RefreshAsync(inactive.RefreshToken));

            Assert.Equal(HttpStatusCode.Unauthorized, expiredResult.Status);
            Assert.Equal(expiredResult, unknownResult);
            Assert.Equal(expiredResult, emptyResult);
            Assert.Equal(expiredResult, missingResult);
            Assert.Equal(expiredResult, inactiveResult);
        }

        [Fact]
        public async Task Concurrent_refreshes_with_the_same_token_let_exactly_one_succeed()
        {
            var user = await TestUsers.CreateAsync(_api);
            var login = await TestUsers.LoginAsync(_api, user);

            var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => RefreshAsync(login.RefreshToken)));

            Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.OK));
            Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.OK),
                r => Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode));

            // The old session was replaced exactly once
            var replacedBy = await TestUsers.WithDbAsync(_api, db => db.Sessions
                .Where(s => s.UserId == user.Id && s.ReplacedById != null)
                .CountAsync());
            Assert.Equal(1, replacedBy);
        }
    }
}
