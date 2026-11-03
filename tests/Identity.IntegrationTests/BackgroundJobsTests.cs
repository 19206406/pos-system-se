using Identity.Application.Contracts.Persistence;
using Identity.Domain.Constants;
using Identity.Domain.Entities;
using Identity.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Identity.IntegrationTests
{
    /// <summary> Outbox retries, first-admin bootstrap and retention purge (spec section 9). </summary>
    public sealed class BackgroundJobsTests(PostgresFixture fixture)
    {
        private readonly IdentityApiFactory _api = fixture.Api;
        private readonly CapturingEmailSender _emails = fixture.Emails;

        [Fact]
        public async Task A_failed_delivery_is_retried_and_only_the_token_actually_sent_stays_valid()
        {
            var user = await TestUsers.CreateAsync(_api);
            _emails.FailNextDeliveries(user.Email, count: 2);

            var forgot = await _api.CreateClient().PostAsJsonAsync("/api/identity/auth/forgot-password", new { email = user.Email });
            Assert.Equal(HttpStatusCode.Accepted, forgot.StatusCode);

            var email = await _emails.WaitForEmailAsync(user.Email);
            var token = CapturingEmailSender.ExtractToken(email);

            // payload is jsonb (no LIKE in SQL), so the user's message is picked in memory
            var resetMessages = await TestUsers.WithDbAsync(_api, db => db.OutboxMessages.AsNoTracking()
                .Where(m => m.Type == "identity.password-reset-requested").ToListAsync());
            var message = Assert.Single(resetMessages, m => m.Payload.Contains(user.Id.ToString()));
            Assert.Equal(2, message.Attempts);
            Assert.NotNull(message.ProcessedAt);

            // Each failed attempt issued a token that never reached the user; all of them were invalidated
            var pendingTokens = await TestUsers.WithDbAsync(_api, db => db.PasswordTokens
                .CountAsync(t => t.UserId == user.Id && t.UsedAt == null));
            Assert.Equal(1, pendingTokens);

            var setPassword = await _api.CreateClient().PostAsJsonAsync(
                "/api/identity/auth/set-password", new { token, newPassword = "after-retries-password" });
            Assert.Equal(HttpStatusCode.NoContent, setPassword.StatusCode);
        }

        [Fact]
        public async Task Bootstrap_gives_the_admin_role_to_the_configured_email_and_sends_an_invite()
        {
            // An empty database of its own: the shared one already has admins created by other tests
            var connectionString = await fixture.CreateMigratedDatabaseAsync($"bootstrap_{Guid.NewGuid():N}");
            var adminEmail = $"First.Admin-{Guid.NewGuid():N}@Test.Local";
            await using var api = fixture.CreateApi(
                new Dictionary<string, string?> { ["Bootstrap:AdminEmail"] = adminEmail }, connectionString);
            _ = api.Services; // starts the host and its background services

            var invite = await _emails.WaitForEmailAsync(adminEmail.ToLowerInvariant());
            var token = CapturingEmailSender.ExtractToken(invite);

            var isAdmin = await TestUsers.WithDbAsync(api, db => db.UserRoles
                .AnyAsync(ur => ur.RoleId == SystemRoles.AdminId && ur.User.Email == adminEmail.ToLowerInvariant()));
            Assert.True(isAdmin);

            var setPassword = await api.CreateClient().PostAsJsonAsync(
                "/api/identity/auth/set-password", new { token, newPassword = "first-admin-password" });
            Assert.Equal(HttpStatusCode.NoContent, setPassword.StatusCode);

            var adminUser = await TestUsers.WithDbAsync(api, db => db.Users.SingleAsync(u => u.Email == adminEmail.ToLowerInvariant()));
            var login = await TestUsers.LoginAsync(api, new TestUser(adminUser.Id, adminUser.Email, "first-admin-password"));
            var users = await TestUsers.CreateClient(api, login.AccessToken).GetAsync("/api/identity/users");
            Assert.Equal(HttpStatusCode.OK, users.StatusCode);
        }

        [Fact]
        public async Task Retention_purge_deletes_only_old_rows_including_whole_rotation_chains()
        {
            var user = await TestUsers.CreateAsync(_api);
            var now = DateTime.UtcNow;
            var old = now.AddDays(-60);

            var chainNewest = new Session { Id = Guid.CreateVersion7(), UserId = user.Id, TokenHash = Hash(), CreatedAt = old, ExpiresAt = old.AddDays(7) };
            var chainOldest = new Session
            {
                Id = Guid.CreateVersion7(), UserId = user.Id, TokenHash = Hash(), CreatedAt = old, ExpiresAt = old.AddDays(7),
                RevokedAt = old.AddHours(1), ReplacedById = chainNewest.Id
            };
            var active = new Session { Id = Guid.CreateVersion7(), UserId = user.Id, TokenHash = Hash(), CreatedAt = now, ExpiresAt = now.AddDays(7) };
            var oldToken = new PasswordToken { UserId = user.Id, TokenHash = Hash(), TokenType = PasswordTokenTypes.Reset, CreatedAt = old, ExpiresAt = old.AddMinutes(30), UsedAt = old.AddMinutes(5) };
            var freshToken = new PasswordToken { UserId = user.Id, TokenHash = Hash(), TokenType = PasswordTokenTypes.Reset, CreatedAt = now, ExpiresAt = now.AddMinutes(30) };
            var oldDelivered = new OutboxMessage { Type = "test.old-delivered", Payload = "{}", OccurredAt = old, ProcessedAt = old };
            var oldUndelivered = new OutboxMessage { Type = "test.old-undelivered", Payload = "{}", OccurredAt = old, Attempts = 5 };

            await TestUsers.WithDbAsync(_api, async db =>
            {
                db.Sessions.AddRange(chainNewest, chainOldest, active);
                db.PasswordTokens.AddRange(oldToken, freshToken);
                db.OutboxMessages.AddRange(oldDelivered, oldUndelivered);
                await db.SaveChangesAsync();
            });

            using (var scope = _api.Services.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IIdentityDataRetention>().PurgeAsync(now.AddDays(-30), CancellationToken.None);
            }

            await TestUsers.WithDbAsync(_api, async db =>
            {
                var sessionIds = await db.Sessions.Where(s => s.UserId == user.Id).Select(s => s.Id).ToListAsync();
                Assert.Equal([active.Id], sessionIds);

                var tokenIds = await db.PasswordTokens.Where(t => t.UserId == user.Id).Select(t => t.Id).ToListAsync();
                Assert.Equal([freshToken.Id], tokenIds);

                Assert.False(await db.OutboxMessages.AnyAsync(m => m.Id == oldDelivered.Id));
                Assert.True(await db.OutboxMessages.AnyAsync(m => m.Id == oldUndelivered.Id));
            });
        }

        private static string Hash() => Convert.ToHexString(Guid.NewGuid().ToByteArray()) + Convert.ToHexString(Guid.NewGuid().ToByteArray());
    }
}
