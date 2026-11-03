using Identity.Application.Contracts.Security;
using Identity.Application.Features.Auth.Commands.LoginUser;
using Identity.Application.Services;
using Identity.Domain.Constants;
using Identity.Domain.Entities;
using Identity.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Identity.IntegrationTests.Infrastructure
{
    public sealed record TestUser(Guid Id, string Email, string? Password);

    /// <summary> Seeds users straight into the database and drives the HTTP API on their behalf. </summary>
    public static class TestUsers
    {
        public const string DefaultPassword = "correct-horse-battery";

        public static async Task<TestUser> CreateAsync(
            IdentityApiFactory api, string? password = DefaultPassword, bool isActive = true, bool isAdmin = false)
        {
            using var scope = api.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var now = DateTime.UtcNow;

            var user = new User
            {
                Id = Guid.CreateVersion7(),
                FullName = "Test User",
                Email = $"user-{Guid.NewGuid():N}@test.local",
                JobTitle = "Tester",
                IsActive = isActive,
                HashPassword = password is null ? null : hasher.Hash(password),
                CreatedAt = now,
                UpdatedAt = now
            };

            if (isAdmin)
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = SystemRoles.AdminId });

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return new TestUser(user.Id, user.Email, password);
        }

        public static async Task<LoginUserResponse> LoginAsync(IdentityApiFactory api, TestUser user, string? password = null)
        {
            var response = await api.CreateClient().PostAsJsonAsync(
                "/api/identity/auth/login", new { email = user.Email, password = password ?? user.Password });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<LoginUserResponse>())!;
        }

        public static HttpClient CreateClient(IdentityApiFactory api, string accessToken)
        {
            var client = api.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return client;
        }

        public static async Task<Session> GetSessionByRefreshTokenAsync(IdentityApiFactory api, string refreshToken)
        {
            using var scope = api.Services.CreateScope();
            var hash = scope.ServiceProvider.GetRequiredService<ITokenHasher>().Hash(refreshToken);
            return await scope.ServiceProvider.GetRequiredService<IdentityDbContext>()
                .Sessions.AsNoTracking().SingleAsync(s => s.TokenHash == hash);
        }

        public static async Task<int> CountActiveSessionsAsync(IdentityApiFactory api, Guid userId)
        {
            using var scope = api.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IdentityDbContext>()
                .Sessions.CountAsync(s => s.UserId == userId && s.RevokedAt == null);
        }

        /// <summary>
        /// Runs the permission synchronization now instead of waiting for PermissionSyncService, so permissions declared
        /// after the seed migration (roles.*) are in the database before a test logs in. Safe to run concurrently with it.
        /// </summary>
        public static async Task<PermissionSyncResult> SynchronizePermissionsAsync(IdentityApiFactory api)
        {
            using var scope = api.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<PermissionCatalogSynchronizer>()
                .SynchronizeAsync(CancellationToken.None);
        }

        public static async Task AssignRoleAsync(IdentityApiFactory api, Guid userId, Guid roleId)
        {
            using var scope = api.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
            await db.SaveChangesAsync();
        }

        public static async Task WithDbAsync(IdentityApiFactory api, Func<IdentityDbContext, Task> action)
        {
            using var scope = api.Services.CreateScope();
            await action(scope.ServiceProvider.GetRequiredService<IdentityDbContext>());
        }

        public static async Task<T> WithDbAsync<T>(IdentityApiFactory api, Func<IdentityDbContext, Task<T>> action)
        {
            using var scope = api.Services.CreateScope();
            return await action(scope.ServiceProvider.GetRequiredService<IdentityDbContext>());
        }
    }
}
