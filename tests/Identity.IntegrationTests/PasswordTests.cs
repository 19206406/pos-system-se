using Identity.Application.Features.Auth.Queries.ValidatePasswordToken;
using Identity.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace Identity.IntegrationTests
{
    /// <summary> Spec section 10, "Contraseñas", plus the reset flow end to end through the outbox. </summary>
    public sealed class PasswordTests(PostgresFixture fixture)
    {
        private const string NewPassword = "brand-new-password-42";

        private readonly IdentityApiFactory _api = fixture.Api;
        private readonly CapturingEmailSender _emails = fixture.Emails;

        private Task<HttpResponseMessage> ForgotAsync(string email) =>
            _api.CreateClient().PostAsJsonAsync("/api/identity/auth/forgot-password", new { email });

        private Task<HttpResponseMessage> SetPasswordAsync(string token, string newPassword = NewPassword) =>
            _api.CreateClient().PostAsJsonAsync("/api/identity/auth/set-password", new { token, newPassword });

        private async Task<string> RequestResetTokenAsync(TestUser user, int expectedEmailCount = 1)
        {
            Assert.Equal(HttpStatusCode.Accepted, (await ForgotAsync(user.Email)).StatusCode);
            return CapturingEmailSender.ExtractToken(await _emails.WaitForEmailAsync(user.Email, expectedEmailCount));
        }

        [Fact]
        public async Task Forgot_password_answers_the_same_for_existing_and_unknown_emails_and_keeps_sessions()
        {
            var user = await TestUsers.CreateAsync(_api);
            await TestUsers.LoginAsync(_api, user);

            var existing = await ForgotAsync(user.Email);
            var unknown = await ForgotAsync($"nobody-{Guid.NewGuid():N}@test.local");

            Assert.Equal(HttpStatusCode.Accepted, existing.StatusCode);
            Assert.Equal(existing.StatusCode, unknown.StatusCode);
            Assert.Equal(await existing.Content.ReadAsStringAsync(), await unknown.Content.ReadAsStringAsync());

            await _emails.WaitForEmailAsync(user.Email);
            Assert.Equal(1, await TestUsers.CountActiveSessionsAsync(_api, user.Id));
        }

        [Fact]
        public async Task Reset_token_is_never_stored_in_plain_text()
        {
            var user = await TestUsers.CreateAsync(_api);
            var token = await RequestResetTokenAsync(user);

            var stored = await TestUsers.WithDbAsync(_api, async db => new
            {
                Hashes = await db.PasswordTokens.Where(t => t.UserId == user.Id).Select(t => t.TokenHash).ToListAsync(),
                Payloads = await db.OutboxMessages.Select(m => m.Payload).ToListAsync()
            });

            Assert.DoesNotContain(token, stored.Hashes);
            Assert.DoesNotContain(stored.Payloads, payload => payload.Contains(token));
        }

        [Fact]
        public async Task Set_password_with_a_reset_token_revokes_every_session_and_the_token_works_once()
        {
            var user = await TestUsers.CreateAsync(_api);
            await TestUsers.LoginAsync(_api, user);
            await TestUsers.LoginAsync(_api, user);
            var token = await RequestResetTokenAsync(user);

            var first = await SetPasswordAsync(token);
            var second = await SetPasswordAsync(token, "yet-another-password-7");

            Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
            Assert.Equal(0, await TestUsers.CountActiveSessionsAsync(_api, user.Id));
            await TestUsers.LoginAsync(_api, user, NewPassword);

            // "Your password changed" notification goes through the outbox too
            var notice = await _emails.WaitForEmailAsync(user.Email, expectedCount: 2);
            Assert.Contains("cambió", notice.Subject);
        }

        [Fact]
        public async Task Concurrent_set_password_requests_with_the_same_token_let_exactly_one_succeed()
        {
            var user = await TestUsers.CreateAsync(_api);
            var token = await RequestResetTokenAsync(user);

            var responses = await Task.WhenAll(Enumerable.Range(0, 6).Select(i => SetPasswordAsync(token, $"parallel-password-{i:00}")));

            Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.NoContent));
            Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.NoContent),
                r => Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode));
        }

        [Fact]
        public async Task Only_the_newest_reset_link_works()
        {
            var user = await TestUsers.CreateAsync(_api);
            var older = await RequestResetTokenAsync(user, expectedEmailCount: 1);
            var newer = await RequestResetTokenAsync(user, expectedEmailCount: 2);

            Assert.Equal(HttpStatusCode.BadRequest, (await SetPasswordAsync(older)).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await SetPasswordAsync(newer)).StatusCode);
        }

        [Fact]
        public async Task Validate_password_token_does_not_consume_it_and_hides_the_cause()
        {
            var user = await TestUsers.CreateAsync(_api);
            var token = await RequestResetTokenAsync(user);
            var client = _api.CreateClient();

            async Task<ValidatePasswordTokenResponse> ValidateAsync(string value) =>
                (await client.GetFromJsonAsync<ValidatePasswordTokenResponse>(
                    $"/api/identity/auth/password-token/validate?token={Uri.EscapeDataString(value)}"))!;

            var valid = await ValidateAsync(token);
            var stillValid = await ValidateAsync(token);
            var unknown = await ValidateAsync("never-issued-token");
            Assert.Equal(HttpStatusCode.NoContent, (await SetPasswordAsync(token)).StatusCode);
            var used = await ValidateAsync(token);

            Assert.Equal(new ValidatePasswordTokenResponse(true, "reset"), valid);
            Assert.Equal(valid, stillValid);
            Assert.Equal(ValidatePasswordTokenResponse.Invalid, unknown);
            Assert.Equal(ValidatePasswordTokenResponse.Invalid, used);
        }

        [Fact]
        public async Task Set_password_rejects_passwords_outside_the_shared_rule()
        {
            var user = await TestUsers.CreateAsync(_api);
            var token = await RequestResetTokenAsync(user);

            Assert.Equal(HttpStatusCode.BadRequest, (await SetPasswordAsync(token, "short")).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await SetPasswordAsync(token, new string('x', 129))).StatusCode);
            // The rejected attempts did not consume the token
            Assert.Equal(HttpStatusCode.NoContent, (await SetPasswordAsync(token)).StatusCode);
        }

        [Fact]
        public async Task Change_password_with_a_wrong_current_password_returns_422()
        {
            var user = await TestUsers.CreateAsync(_api);
            var login = await TestUsers.LoginAsync(_api, user);

            var response = await TestUsers.CreateClient(_api, login.AccessToken).PostAsJsonAsync(
                "/api/identity/auth/change-password", new { currentPassword = "not-the-password", newPassword = NewPassword });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task Change_password_revokes_the_other_sessions_and_keeps_the_current_one()
        {
            var user = await TestUsers.CreateAsync(_api);
            var other = await TestUsers.LoginAsync(_api, user);
            var current = await TestUsers.LoginAsync(_api, user);

            var response = await TestUsers.CreateClient(_api, current.AccessToken).PostAsJsonAsync(
                "/api/identity/auth/change-password", new { currentPassword = user.Password, newPassword = NewPassword });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.NotNull((await TestUsers.GetSessionByRefreshTokenAsync(_api, other.RefreshToken)).RevokedAt);
            Assert.Null((await TestUsers.GetSessionByRefreshTokenAsync(_api, current.RefreshToken)).RevokedAt);
            await TestUsers.LoginAsync(_api, user, NewPassword);
        }
    }
}
