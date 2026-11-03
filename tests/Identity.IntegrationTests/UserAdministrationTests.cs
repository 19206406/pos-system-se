using Identity.Application.Features.Auth.Queries.GetCurrentUser;
using Identity.Application.Features.Users.Commands.CreateUser;
using Identity.Application.Features.Users.Queries.GetUsers;
using Identity.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Identity.Application.Authorization;
using System.Net;
using System.Net.Http.Json;

namespace Identity.IntegrationTests
{
    /// <summary> User administration (spec 6.2 and 6.3) and the invite flow end to end through the outbox. </summary>
    public sealed class UserAdministrationTests(PostgresFixture fixture)
    {
        private readonly IdentityApiFactory _api = fixture.Api;
        private readonly CapturingEmailSender _emails = fixture.Emails;

        private async Task<HttpClient> AdminClientAsync()
        {
            var admin = await TestUsers.CreateAsync(_api, isAdmin: true);
            return TestUsers.CreateClient(_api, (await TestUsers.LoginAsync(_api, admin)).AccessToken);
        }

        [Fact]
        public async Task A_created_user_receives_an_invite_sets_a_password_and_logs_in()
        {
            var admin = await AdminClientAsync();
            var email = $"New.Waiter-{Guid.NewGuid():N}@Test.Local";

            var created = await admin.PostAsJsonAsync("/api/identity/users",
                new { fullName = "New Waiter", email, phoneNumber = "3001234567", jobTitle = "Mesero" });

            Assert.Equal(HttpStatusCode.OK, created.StatusCode);
            var user = (await created.Content.ReadFromJsonAsync<CreateUserResponse>())!;
            Assert.Equal(email.ToLowerInvariant(), user.Email);
            Assert.Equal("3001234567", user.PhoneNumber);
            Assert.Null(await TestUsers.WithDbAsync(_api, db => db.Users.Where(u => u.Id == user.Id).Select(u => u.HashPassword).SingleAsync()));

            var invite = await _emails.WaitForEmailAsync(user.Email);
            var token = CapturingEmailSender.ExtractToken(invite);

            var setPassword = await _api.CreateClient().PostAsJsonAsync(
                "/api/identity/auth/set-password", new { token, newPassword = "first-own-password" });
            Assert.Equal(HttpStatusCode.NoContent, setPassword.StatusCode);

            await TestUsers.LoginAsync(_api, new TestUser(user.Id, email, "first-own-password"));
        }

        [Fact]
        public async Task Creating_a_user_with_an_existing_email_is_a_business_error()
        {
            var admin = await AdminClientAsync();
            var existing = await TestUsers.CreateAsync(_api);

            var response = await admin.PostAsJsonAsync("/api/identity/users",
                new { fullName = "Duplicate", email = existing.Email.ToUpperInvariant(), phoneNumber = "300", jobTitle = "Cajero" });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task Resend_invite_is_refused_for_a_user_who_already_has_a_password()
        {
            var admin = await AdminClientAsync();
            var withPassword = await TestUsers.CreateAsync(_api);
            var pending = await TestUsers.CreateAsync(_api, password: null);

            var refused = await admin.PostAsync($"/api/identity/users/{withPassword.Id}/resend-invite", null);
            var accepted = await admin.PostAsync($"/api/identity/users/{pending.Id}/resend-invite", null);
            var missing = await admin.PostAsync($"/api/identity/users/{Guid.NewGuid()}/resend-invite", null);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, accepted.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
            await _emails.WaitForEmailAsync(pending.Email);
        }

        [Fact]
        public async Task Deactivating_a_user_revokes_their_sessions_and_reactivating_does_not_restore_them()
        {
            var admin = await AdminClientAsync();
            var user = await TestUsers.CreateAsync(_api);
            var login = await TestUsers.LoginAsync(_api, user);

            var deactivate = await admin.PostAsync($"/api/identity/users/{user.Id}/deactivate", null);

            Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);
            Assert.Equal(0, await TestUsers.CountActiveSessionsAsync(_api, user.Id));
            var refresh = await _api.CreateClient().PostAsJsonAsync("/api/identity/auth/refresh", new { refreshToken = login.RefreshToken });
            Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);

            Assert.Equal(HttpStatusCode.NoContent, (await admin.PostAsync($"/api/identity/users/{user.Id}/reactivate", null)).StatusCode);
            Assert.Equal(0, await TestUsers.CountActiveSessionsAsync(_api, user.Id));
            await TestUsers.LoginAsync(_api, user);
        }

        [Fact]
        public async Task An_admin_cannot_deactivate_their_own_account()
        {
            var admin = await TestUsers.CreateAsync(_api, isAdmin: true);
            var client = TestUsers.CreateClient(_api, (await TestUsers.LoginAsync(_api, admin)).AccessToken);

            var response = await client.PostAsync($"/api/identity/users/{admin.Id}/deactivate", null);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task Revoke_user_sessions_closes_them_without_deactivating_and_404s_for_unknown_users()
        {
            var admin = await AdminClientAsync();
            var user = await TestUsers.CreateAsync(_api);
            await TestUsers.LoginAsync(_api, user);

            var revoke = await admin.DeleteAsync($"/api/identity/users/{user.Id}/sessions");
            var unknown = await admin.DeleteAsync($"/api/identity/users/{Guid.NewGuid()}/sessions");

            Assert.Equal(HttpStatusCode.NoContent, revoke.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
            Assert.Equal(0, await TestUsers.CountActiveSessionsAsync(_api, user.Id));
            await TestUsers.LoginAsync(_api, user);
        }

        [Fact]
        public async Task Get_users_paginates_and_treats_like_wildcards_as_literal_text()
        {
            var admin = await AdminClientAsync();
            var marker = Guid.NewGuid().ToString("N")[..8];
            for (var i = 0; i < 3; i++)
            {
                var user = await TestUsers.CreateAsync(_api);
                var fullName = $"Paged {marker} {i}";
                await TestUsers.WithDbAsync(_api, db => db.Users.Where(u => u.Id == user.Id)
                    .ExecuteUpdateAsync(u => u.SetProperty(x => x.FullName, fullName)));
            }

            var firstPage = await admin.GetFromJsonAsync<GetUsersResponse>($"/api/identity/users?search={marker}&page=1&pageSize=2");
            var secondPage = await admin.GetFromJsonAsync<GetUsersResponse>($"/api/identity/users?search={marker}&page=2&pageSize=2");
            var wildcard = await admin.GetFromJsonAsync<GetUsersResponse>("/api/identity/users?search=%25&pageSize=100");
            var tooLarge = await admin.GetAsync("/api/identity/users?pageSize=101");

            Assert.Equal(3, firstPage!.TotalCount);
            Assert.Equal(2, firstPage.TotalPages);
            Assert.Equal(2, firstPage.Items.Count);
            Assert.Single(secondPage!.Items);
            Assert.Empty(firstPage.Items.Select(u => u.Id).Intersect(secondPage.Items.Select(u => u.Id)));
            Assert.Equal(0, wildcard!.TotalCount); // "%" is searched literally, not as "match everything"
            Assert.Equal(HttpStatusCode.BadRequest, tooLarge.StatusCode);
        }

        [Fact]
        public async Task Get_current_user_reads_roles_and_permissions_from_the_database()
        {
            var admin = await TestUsers.CreateAsync(_api, isAdmin: true);
            var login = await TestUsers.LoginAsync(_api, admin);

            var me = await TestUsers.CreateClient(_api, login.AccessToken).GetFromJsonAsync<GetCurrentUserResponse>("/api/identity/auth/me");

            Assert.Equal(admin.Id, me!.Id);
            Assert.Contains("Admin", me.Roles);
            // Admin holds every permission declared by the catalogs (granted by the permission synchronization)
            var declaredCodes = new IdentityPermissionCatalog().Permissions.Select(p => p.Code);
            Assert.Equal(declaredCodes.Order(), me.Permissions.Order());
        }
    }
}
