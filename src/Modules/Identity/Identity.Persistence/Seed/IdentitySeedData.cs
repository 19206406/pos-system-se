using Identity.Domain.Constants;
using Identity.Domain.Entities;
using Shared.Constants.Claims;

namespace Identity.Persistence.Seed
{
    /// <summary>
    /// Reference data created by migrations: one permission per code in PermissionCodes.All and the Admin role
    /// holding all of them. Ids are fixed so later migrations can reference the rows. Adding a permission code
    /// means adding it here too and generating a migration.
    /// </summary>
    internal static class IdentitySeedData
    {
        public static readonly Permission[] Permissions =
        [
            Permission("9769750d-a668-463d-a7b1-91dae751524f", PermissionCodes.Users.Read, "Ver usuarios", "Consultar la lista de usuarios y su detalle."),
            Permission("1a0a94a4-e6b7-406c-9a41-3af1b5041eee", PermissionCodes.Users.Create, "Crear usuarios", "Crear usuarios y reenviar invitaciones."),
            Permission("82f841fc-d993-4c8f-9463-700fe5e8f944", PermissionCodes.Users.Update, "Modificar usuarios", "Desactivar y reactivar usuarios."),
            Permission("329bf5b9-e1df-49cd-9f42-27f523a1eb1c", PermissionCodes.Sessions.Read, "Ver sesiones", "Consultar las sesiones activas de otros usuarios."),
            Permission("7ae90271-e5e3-4573-adca-3b55657e7c2a", PermissionCodes.Sessions.Revoke, "Revocar sesiones", "Cerrar todas las sesiones de otro usuario.")
        ];

        public static readonly Role[] Roles =
        [
            new Role
            {
                Id = SystemRoles.AdminId,
                RoleName = SystemRoles.AdminName,
                RoleDescription = "Administrador con todos los permisos del módulo Identity."
            }
        ];

        public static readonly RolePermission[] RolePermissions =
            Permissions.Select(p => new RolePermission { RoleId = SystemRoles.AdminId, PermissionId = p.Id }).ToArray();

        private static Permission Permission(string id, string identifier, string name, string description) => new()
        {
            Id = Guid.Parse(id),
            Identifier = identifier,
            PermissionName = name,
            PermissionDescription = description
        };
    }
}
