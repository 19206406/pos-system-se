using Identity.Application.Authorization;
using Identity.Domain.Constants;
using Identity.Domain.Entities;
using Identity.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Authorization;

namespace Identity.IntegrationTests
{
    /// <summary> Permission synchronization from the module catalogs and validation of those catalogs (spec 9.1). </summary>
    public sealed class PermissionSyncTests(PostgresFixture fixture)
    {
        // Each test gets an empty database: they insert and change permission rows the other tests rely on
        private async Task<IdentityApiFactory> CreateIsolatedApiAsync()
        {
            var connectionString = await fixture.CreateMigratedDatabaseAsync($"permissions_{Guid.NewGuid():N}");
            return fixture.CreateApi(new Dictionary<string, string?>(), connectionString);
        }

        [Fact]
        public async Task The_migration_keeps_the_seeded_permissions_with_their_ids_and_assigns_them_to_identity()
        {
            await using var api = await CreateIsolatedApiAsync();

            var usersRead = await TestUsers.WithDbAsync(api, db => db.Permissions.AsNoTracking()
                .SingleAsync(p => p.Identifier == IdentityPermissionCodes.Users.Read));

            Assert.Equal(Guid.Parse("9769750d-a668-463d-a7b1-91dae751524f"), usersRead.Id);
            Assert.Equal("identity", usersRead.Module);
        }

        [Fact]
        public async Task Synchronization_writes_every_declared_permission_grants_them_to_admin_and_is_idempotent()
        {
            await using var api = await CreateIsolatedApiAsync();

            await TestUsers.SynchronizePermissionsAsync(api);
            var secondRun = await TestUsers.SynchronizePermissionsAsync(api);

            Assert.Equal(0, secondRun.WrittenPermissions);
            Assert.Equal(0, secondRun.GrantedToAdmin);
            Assert.Empty(secondRun.UndeclaredCodes);

            var declared = new IdentityPermissionCatalog().Permissions.Select(p => p.Code).Order().ToList();
            var stored = await TestUsers.WithDbAsync(api, db => db.Permissions.Select(p => p.Identifier).ToListAsync());
            var adminCodes = await TestUsers.WithDbAsync(api, db => db.RolePermissions
                .Where(rp => rp.RoleId == SystemRoles.AdminId).Select(rp => rp.Permission.Identifier).ToListAsync());

            Assert.Equal(declared, stored.Order());
            Assert.Equal(declared, adminCodes.Order());
        }

        [Fact]
        public async Task Synchronization_restores_changed_texts_keeps_undeclared_permissions_and_leaves_other_roles_alone()
        {
            await using var api = await CreateIsolatedApiAsync();
            await TestUsers.SynchronizePermissionsAsync(api);

            var customRoleId = Guid.CreateVersion7();
            await TestUsers.WithDbAsync(api, async db =>
            {
                var usersRead = await db.Permissions.SingleAsync(p => p.Identifier == IdentityPermissionCodes.Users.Read);
                usersRead.PermissionDescription = "Edited by hand";

                var legacy = new Permission
                {
                    Id = Guid.CreateVersion7(),
                    Identifier = "legacy.thing",
                    Module = "identity",
                    PermissionName = "Legacy",
                    PermissionDescription = "No module declares it anymore"
                };
                db.Permissions.Add(legacy);

                var role = new Role { Id = customRoleId, RoleName = $"Custom {Guid.NewGuid():N}", RoleDescription = "Test" };
                role.ReplacePermissions([usersRead.Id, legacy.Id]);
                db.Roles.Add(role);

                await db.SaveChangesAsync();
            });

            // No assertion on WrittenPermissions: PermissionSyncService of this host may restore the text first.
            // The state of the table is what matters, whichever run wrote it.
            var result = await TestUsers.SynchronizePermissionsAsync(api);

            Assert.Equal(["legacy.thing"], result.UndeclaredCodes);

            var description = await TestUsers.WithDbAsync(api, db => db.Permissions
                .Where(p => p.Identifier == IdentityPermissionCodes.Users.Read).Select(p => p.PermissionDescription).SingleAsync());
            var expected = new IdentityPermissionCatalog().Permissions.Single(p => p.Code == IdentityPermissionCodes.Users.Read).Description;
            Assert.Equal(expected, description);

            // The custom role keeps exactly what it had, including the undeclared permission
            var customCodes = await TestUsers.WithDbAsync(api, db => db.RolePermissions
                .Where(rp => rp.RoleId == customRoleId).Select(rp => rp.Permission.Identifier).ToListAsync());
            Assert.Equal(["legacy.thing", IdentityPermissionCodes.Users.Read], customCodes.Order());
        }

        [Fact]
        public async Task Concurrent_synchronizations_do_not_fail_or_duplicate_rows()
        {
            await using var api = await CreateIsolatedApiAsync();

            await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => TestUsers.SynchronizePermissionsAsync(api)));

            var declaredCount = new IdentityPermissionCatalog().Permissions.Count;
            Assert.Equal(declaredCount, await TestUsers.WithDbAsync(api, db => db.Permissions.CountAsync()));
            Assert.Equal(declaredCount, await TestUsers.WithDbAsync(api, db => db.RolePermissions.CountAsync(rp => rp.RoleId == SystemRoles.AdminId)));
        }

        [Fact]
        public void A_catalog_with_a_code_declared_twice_is_rejected()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                new PermissionCatalog([new IdentityPermissionCatalog(), new FakeCatalog("sales", "users.read")]));

            Assert.Contains("users.read", exception.Message);
        }

        [Theory]
        [InlineData("Sales", "orders.read")]
        [InlineData("sales", "orders")]
        [InlineData("sales", "Orders.Read")]
        public void A_catalog_with_an_invalid_module_or_permission_code_is_rejected(string moduleCode, string permissionCode)
        {
            Assert.Throws<InvalidOperationException>(() => new PermissionCatalog([new FakeCatalog(moduleCode, permissionCode)]));
        }

        [Fact]
        public void Catalogs_of_several_modules_are_combined()
        {
            var catalog = new PermissionCatalog([new IdentityPermissionCatalog(), new FakeCatalog("sales", "orders.read")]);

            Assert.Contains("orders.read", catalog.AllCodes);
            Assert.Contains(IdentityPermissionCodes.Users.Read, catalog.AllCodes);
            Assert.Equal("Sales", catalog.GetModuleName("sales"));
        }

        private sealed class FakeCatalog(string moduleCode, string permissionCode) : IModulePermissionCatalog
        {
            public string ModuleCode => moduleCode;
            public string ModuleName => "Sales";
            public IReadOnlyList<PermissionDefinition> Permissions => [new(permissionCode, "Name", "Description")];
        }
    }
}
