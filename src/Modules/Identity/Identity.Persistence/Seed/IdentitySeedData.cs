using Identity.Domain.Constants;
using Identity.Domain.Entities;

namespace Identity.Persistence.Seed
{
    /// <summary>
    /// Reference data created by migrations: only the Admin role, whose id is fixed so code can reference it.
    /// Permissions are not seeded here: each module declares them in its IModulePermissionCatalog and
    /// PermissionCatalogSynchronizer writes them at startup, also granting all of them to Admin.
    /// </summary>
    internal static class IdentitySeedData
    {
        public static readonly Role[] Roles =
        [
            new Role
            {
                Id = SystemRoles.AdminId,
                RoleName = SystemRoles.AdminName,
                RoleDescription = "Administrador con todos los permisos de todos los módulos."
            }
        ];
    }
}
