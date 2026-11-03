using Shared.Authorization;
using Codes = Identity.Application.Authorization.IdentityPermissionCodes;

namespace Identity.Application.Authorization
{
    /// <summary> The permissions the Identity module declares. Adding one here is enough: no migration is needed. </summary>
    public sealed class IdentityPermissionCatalog : IModulePermissionCatalog
    {
        public string ModuleCode => "identity";

        public string ModuleName => "Identity";

        public IReadOnlyList<PermissionDefinition> Permissions { get; } =
        [
            new(Codes.Users.Read, "Ver usuarios", "Consultar la lista de usuarios, su detalle y sus roles."),
            new(Codes.Users.Create, "Crear usuarios", "Crear usuarios y reenviar invitaciones."),
            new(Codes.Users.Update, "Modificar usuarios", "Desactivar y reactivar usuarios."),
            new(Codes.Sessions.Read, "Ver sesiones", "Consultar las sesiones activas de otros usuarios."),
            new(Codes.Sessions.Revoke, "Revocar sesiones", "Cerrar todas las sesiones de otro usuario."),
            new(Codes.Roles.Read, "Ver roles", "Consultar los roles, sus permisos y el catálogo de permisos."),
            new(Codes.Roles.Create, "Crear roles", "Crear roles y elegir sus permisos."),
            new(Codes.Roles.Update, "Modificar roles", "Cambiar el nombre, la descripción y los permisos de un rol."),
            new(Codes.Roles.Delete, "Eliminar roles", "Eliminar roles que ningún usuario tenga asignados."),
            new(Codes.Roles.Assign, "Asignar roles", "Asignar y revocar los roles de los usuarios.")
        ];
    }
}
