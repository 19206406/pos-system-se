using Identity.Application.Authorization;
using Identity.Application.Features.Auth.Commands.RefreshToken;
using Identity.Application.Features.Permissions.Queries.GetModulePermissions;
using Identity.Application.Features.Permissions.Queries.GetPermissions;
using Identity.Application.Features.Roles.Commands.CreateRole;
using Identity.Application.Features.Roles.Queries.GetRoleById;
using Identity.Application.Features.Roles.Queries.GetRoles;
using Identity.Application.Features.Users.Queries.GetUserRoles;
using Identity.Domain.Constants;
using Identity.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using Codes = Identity.Application.Authorization.IdentityPermissionCodes;

namespace Identity.IntegrationTests
{
    /// <summary> Roles, the permission catalog and the roles of users (spec 6.4). </summary>
    public sealed class RoleAdministrationTests(PostgresFixture fixture)
    {
        private readonly IdentityApiFactory _api = fixture.Api;

        private async Task<HttpClient> AdminClientAsync(IdentityApiFactory? api = null)
        {
            api ??= _api;
            var admin = await TestUsers.CreateAsync(api, isAdmin: true);
            return TestUsers.CreateClient(api, (await TestUsers.LoginAsync(api, admin)).AccessToken);
        }

        private static async Task<CreateRoleResponse> CreateRoleAsync(HttpClient admin, params string[] permissionCodes)
        {
            var response = await admin.PostAsJsonAsync("/api/identity/roles", new
            {
                roleName = $"Role {Guid.NewGuid():N}",
                roleDescription = "Created by a test",
                permissionCodes
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<CreateRoleResponse>())!;
        }

        private static async Task<IReadOnlyList<string>> GetRolePermissionCodesAsync(HttpClient admin, Guid roleId)
        {
            var role = await admin.GetFromJsonAsync<GetRoleByIdResponse>($"/api/identity/roles/{roleId}");
            return role!.Modules.SelectMany(m => m.Permissions).Select(p => p.Code).Order().ToList();
        }

        [Fact]
        public async Task The_permission_catalog_is_grouped_by_module_and_holds_every_declared_permission()
        {
            var admin = await AdminClientAsync();

            var catalog = await admin.GetFromJsonAsync<GetPermissionsResponse>("/api/identity/permissions");

            var identity = Assert.Single(catalog!.Modules, m => m.ModuleCode == "identity");
            Assert.Equal("Identity", identity.ModuleName);

            var declared = new IdentityPermissionCatalog().Permissions;
            Assert.Equal(declared.Select(p => p.Code).Order(), identity.Permissions.Select(p => p.Code).Order());
            Assert.All(identity.Permissions, p => Assert.False(string.IsNullOrWhiteSpace(p.Name)));
        }

        [Fact]
        public async Task The_permissions_of_one_module_are_returned_by_its_code()
        {
            var admin = await AdminClientAsync();

            var identity = await admin.GetFromJsonAsync<GetModulePermissionsResponse>("/api/identity/permissions/modules/Identity");
            var unknown = await admin.GetAsync("/api/identity/permissions/modules/no-such-module");

            Assert.Equal("identity", identity!.ModuleCode);
            Assert.Equal("Identity", identity.ModuleName);
            Assert.Equal(
                new IdentityPermissionCatalog().Permissions.Select(p => p.Code).Order(StringComparer.Ordinal),
                identity.Permissions.Select(p => p.Code));
            Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        }

        [Fact]
        public async Task A_role_is_created_with_its_permissions_and_shown_in_detail_and_in_the_list()
        {
            var admin = await AdminClientAsync();

            var created = await CreateRoleAsync(admin, Codes.Users.Read, Codes.Sessions.Read, Codes.Users.Read);

            Assert.Equal([Codes.Sessions.Read, Codes.Users.Read], created.PermissionCodes);
            Assert.Equal([Codes.Sessions.Read, Codes.Users.Read], await GetRolePermissionCodesAsync(admin, created.Id));

            var list = await admin.GetFromJsonAsync<GetRolesResponse>("/api/identity/roles");
            var item = Assert.Single(list!.Items, r => r.Id == created.Id);
            Assert.Equal(2, item.PermissionCount);
            Assert.Equal(0, item.UserCount);
            Assert.False(item.IsSystemRole);
            Assert.True(Assert.Single(list.Items, r => r.Id == SystemRoles.AdminId).IsSystemRole);
        }

        [Fact]
        public async Task Creating_a_role_is_validated_and_rejects_unknown_permissions_and_duplicate_names()
        {
            var admin = await AdminClientAsync();
            var existing = await CreateRoleAsync(admin);

            var unknownPermission = await admin.PostAsJsonAsync("/api/identity/roles",
                new { roleName = $"Role {Guid.NewGuid():N}", roleDescription = "x", permissionCodes = new[] { "nothing.here" } });
            var duplicateName = await admin.PostAsJsonAsync("/api/identity/roles",
                new { roleName = existing.RoleName.ToUpperInvariant(), roleDescription = "x", permissionCodes = Array.Empty<string>() });
            var emptyName = await admin.PostAsJsonAsync("/api/identity/roles",
                new { roleName = "", roleDescription = "x", permissionCodes = Array.Empty<string>() });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, unknownPermission.StatusCode);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicateName.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, emptyName.StatusCode);
        }

        [Fact]
        public async Task Updating_a_role_replaces_its_data_and_synchronizes_its_permissions()
        {
            var admin = await AdminClientAsync();
            var role = await CreateRoleAsync(admin, Codes.Users.Read, Codes.Sessions.Read);
            var newName = $"Renamed {Guid.NewGuid():N}";

            var response = await admin.PutAsJsonAsync($"/api/identity/roles/{role.Id}", new
            {
                roleName = newName,
                roleDescription = "Updated",
                permissionCodes = new[] { Codes.Sessions.Read, Codes.Roles.Read }
            });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var detail = await admin.GetFromJsonAsync<GetRoleByIdResponse>($"/api/identity/roles/{role.Id}");
            Assert.Equal(newName, detail!.RoleName);
            Assert.Equal("Updated", detail.RoleDescription);
            Assert.Equal([Codes.Roles.Read, Codes.Sessions.Read], await GetRolePermissionCodesAsync(admin, role.Id));

            var missing = await admin.PutAsJsonAsync($"/api/identity/roles/{Guid.NewGuid()}",
                new { roleName = "x", roleDescription = "x", permissionCodes = Array.Empty<string>() });
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        }

        [Fact]
        public async Task The_admin_role_cannot_be_modified_or_deleted()
        {
            var admin = await AdminClientAsync();

            var update = await admin.PutAsJsonAsync($"/api/identity/roles/{SystemRoles.AdminId}",
                new { roleName = "Admin", roleDescription = "x", permissionCodes = Array.Empty<string>() });
            var delete = await admin.DeleteAsync($"/api/identity/roles/{SystemRoles.AdminId}");

            Assert.Equal(HttpStatusCode.UnprocessableEntity, update.StatusCode);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, delete.StatusCode);
        }

        [Fact]
        public async Task A_role_can_be_deleted_only_while_no_user_holds_it()
        {
            var admin = await AdminClientAsync();
            var unassigned = await CreateRoleAsync(admin, Codes.Users.Read);
            var assigned = await CreateRoleAsync(admin, Codes.Users.Read);
            var holder = await TestUsers.CreateAsync(_api);
            await TestUsers.AssignRoleAsync(_api, holder.Id, assigned.Id);

            var deleted = await admin.DeleteAsync($"/api/identity/roles/{unassigned.Id}");
            var refused = await admin.DeleteAsync($"/api/identity/roles/{assigned.Id}");
            var missing = await admin.DeleteAsync($"/api/identity/roles/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/api/identity/roles/{unassigned.Id}")).StatusCode);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
            Assert.True(await TestUsers.WithDbAsync(_api, db => db.UserRoles.AnyAsync(ur => ur.RoleId == assigned.Id)));
        }

        [Fact]
        public async Task The_database_refuses_to_delete_a_role_that_users_hold()
        {
            var admin = await AdminClientAsync();
            var role = await CreateRoleAsync(admin);
            var holder = await TestUsers.CreateAsync(_api);
            await TestUsers.AssignRoleAsync(_api, holder.Id, role.Id);

            // Bypasses the handler: fk_users_roles_roles is ON DELETE RESTRICT, so the last guard is the schema itself
            await Assert.ThrowsAnyAsync<Exception>(() => TestUsers.WithDbAsync(_api,
                db => db.Roles.Where(r => r.Id == role.Id).ExecuteDeleteAsync()));

            Assert.True(await TestUsers.WithDbAsync(_api, db => db.UserRoles.AnyAsync(ur => ur.RoleId == role.Id)));
        }

        [Fact]
        public async Task The_roles_of_a_user_are_replaced_as_a_whole()
        {
            var admin = await AdminClientAsync();
            var cashier = await CreateRoleAsync(admin, Codes.Users.Read);
            var waiter = await CreateRoleAsync(admin, Codes.Sessions.Read);
            var user = await TestUsers.CreateAsync(_api);

            var assign = await admin.PutAsJsonAsync($"/api/identity/users/{user.Id}/roles", new { roleIds = new[] { cashier.Id, waiter.Id } });
            Assert.Equal(HttpStatusCode.NoContent, assign.StatusCode);
            var afterAssign = await admin.GetFromJsonAsync<GetUserRolesResponse>($"/api/identity/users/{user.Id}/roles");
            Assert.Equal(new[] { cashier.Id, waiter.Id }.Order(), afterAssign!.Roles.Select(r => r.Id).Order());

            var replace = await admin.PutAsJsonAsync($"/api/identity/users/{user.Id}/roles", new { roleIds = new[] { waiter.Id } });
            Assert.Equal(HttpStatusCode.NoContent, replace.StatusCode);
            var afterReplace = await admin.GetFromJsonAsync<GetUserRolesResponse>($"/api/identity/users/{user.Id}/roles");
            Assert.Equal(waiter.Id, Assert.Single(afterReplace!.Roles).Id);

            var revokeAll = await admin.PutAsJsonAsync($"/api/identity/users/{user.Id}/roles", new { roleIds = Array.Empty<Guid>() });
            Assert.Equal(HttpStatusCode.NoContent, revokeAll.StatusCode);
            Assert.Empty((await admin.GetFromJsonAsync<GetUserRolesResponse>($"/api/identity/users/{user.Id}/roles"))!.Roles);
        }

        [Fact]
        public async Task Assigning_roles_rejects_unknown_roles_and_unknown_users()
        {
            var admin = await AdminClientAsync();
            var user = await TestUsers.CreateAsync(_api);

            var unknownRole = await admin.PutAsJsonAsync($"/api/identity/users/{user.Id}/roles", new { roleIds = new[] { Guid.NewGuid() } });
            var unknownUser = await admin.PutAsJsonAsync($"/api/identity/users/{Guid.NewGuid()}/roles", new { roleIds = Array.Empty<Guid>() });
            var readUnknownUser = await admin.GetAsync($"/api/identity/users/{Guid.NewGuid()}/roles");

            Assert.Equal(HttpStatusCode.UnprocessableEntity, unknownRole.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, unknownUser.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, readUnknownUser.StatusCode);
        }

        [Fact]
        public async Task Role_administration_requires_its_permissions()
        {
            var admin = await AdminClientAsync();
            var readOnly = await CreateRoleAsync(admin, Codes.Roles.Read);
            var user = await TestUsers.CreateAsync(_api);
            await TestUsers.AssignRoleAsync(_api, user.Id, readOnly.Id);
            var client = TestUsers.CreateClient(_api, (await TestUsers.LoginAsync(_api, user)).AccessToken);

            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/identity/roles")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/identity/permissions")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteAsync($"/api/identity/roles/{readOnly.Id}")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.PutAsJsonAsync(
                $"/api/identity/users/{user.Id}/roles", new { roleIds = new[] { SystemRoles.AdminId } })).StatusCode);
        }

        [Fact]
        public async Task A_change_to_the_permissions_of_a_role_reaches_the_user_on_the_next_refresh()
        {
            var admin = await AdminClientAsync();
            var role = await CreateRoleAsync(admin, Codes.Users.Read);
            var user = await TestUsers.CreateAsync(_api);
            await TestUsers.AssignRoleAsync(_api, user.Id, role.Id);
            var login = await TestUsers.LoginAsync(_api, user);

            var revoke = await admin.PutAsJsonAsync($"/api/identity/roles/{role.Id}",
                new { roleName = role.RoleName, roleDescription = role.RoleDescription, permissionCodes = Array.Empty<string>() });
            Assert.Equal(HttpStatusCode.NoContent, revoke.StatusCode);

            // The current access token keeps its permissions until it expires (spec section 5)
            Assert.Equal(HttpStatusCode.OK, (await TestUsers.CreateClient(_api, login.AccessToken).GetAsync("/api/identity/users")).StatusCode);

            var refresh = await _api.CreateClient().PostAsJsonAsync("/api/identity/auth/refresh", new { refreshToken = login.RefreshToken });
            var refreshed = (await refresh.Content.ReadFromJsonAsync<RefreshTokenResponse>())!;
            Assert.Equal(HttpStatusCode.Forbidden, (await TestUsers.CreateClient(_api, refreshed.AccessToken).GetAsync("/api/identity/users")).StatusCode);
        }

        [Fact]
        public async Task The_admin_role_cannot_be_revoked_from_its_last_holder()
        {
            // An empty database of its own: the shared one has admins created by every other test
            var connectionString = await fixture.CreateMigratedDatabaseAsync($"last_admin_{Guid.NewGuid():N}");
            await using var api = fixture.CreateApi(new Dictionary<string, string?>(), connectionString);
            await TestUsers.SynchronizePermissionsAsync(api);

            var onlyAdmin = await TestUsers.CreateAsync(api, isAdmin: true);
            var admin = TestUsers.CreateClient(api, (await TestUsers.LoginAsync(api, onlyAdmin)).AccessToken);

            var refused = await admin.PutAsJsonAsync($"/api/identity/users/{onlyAdmin.Id}/roles", new { roleIds = Array.Empty<Guid>() });
            Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);

            var secondAdmin = await TestUsers.CreateAsync(api, isAdmin: true);
            var allowed = await admin.PutAsJsonAsync($"/api/identity/users/{onlyAdmin.Id}/roles", new { roleIds = Array.Empty<Guid>() });
            Assert.Equal(HttpStatusCode.NoContent, allowed.StatusCode);

            var lastOne = await admin.PutAsJsonAsync($"/api/identity/users/{secondAdmin.Id}/roles", new { roleIds = Array.Empty<Guid>() });
            Assert.Equal(HttpStatusCode.UnprocessableEntity, lastOne.StatusCode);
        }
    }
}
