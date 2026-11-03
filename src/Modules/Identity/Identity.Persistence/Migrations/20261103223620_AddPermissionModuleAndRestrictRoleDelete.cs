using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Persistence.Migrations
{
    /// <summary>
    /// Classifies permissions by module and makes deleting a role that users hold fail instead of cascading.
    /// Edited by hand after scaffolding: removing HasData made EF delete the five seeded permissions (and, by cascade,
    /// every role assignment of them). The rows are kept instead; from now on PermissionCatalogSynchronizer owns them,
    /// matching them by identifier.
    /// </summary>
    public partial class AddPermissionModuleAndRestrictRoleDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_roles_roles",
                schema: "identity",
                table: "users_roles");

            // Nullable first, filled, then NOT NULL: avoids leaving a permanent DEFAULT '' on the column.
            // Every permission that exists today belongs to Identity.
            migrationBuilder.AddColumn<string>(
                name: "module",
                schema: "identity",
                table: "permissions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("UPDATE identity.permissions SET module = 'identity' WHERE module IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "module",
                schema: "identity",
                table: "permissions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.UpdateData(
                schema: "identity",
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11"),
                column: "role_description",
                value: "Administrador con todos los permisos de todos los módulos.");

            migrationBuilder.CreateIndex(
                name: "idx_permissions_module",
                schema: "identity",
                table: "permissions",
                column: "module");

            migrationBuilder.AddForeignKey(
                name: "fk_users_roles_roles",
                schema: "identity",
                table: "users_roles",
                column: "role_id",
                principalSchema: "identity",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_roles_roles",
                schema: "identity",
                table: "users_roles");

            migrationBuilder.DropIndex(
                name: "idx_permissions_module",
                schema: "identity",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "module",
                schema: "identity",
                table: "permissions");

            // The seeded permission rows were never deleted by Up, so there is nothing to insert back

            migrationBuilder.UpdateData(
                schema: "identity",
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11"),
                column: "role_description",
                value: "Administrador con todos los permisos del módulo Identity.");

            migrationBuilder.AddForeignKey(
                name: "fk_users_roles_roles",
                schema: "identity",
                table: "users_roles",
                column: "role_id",
                principalSchema: "identity",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
