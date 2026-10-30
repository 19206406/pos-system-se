using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                    table.CheckConstraint("chk_outbox_messages_attempts", "attempts >= 0");
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "identifier", "permission_description", "permission_name" },
                values: new object[,]
                {
                    { new Guid("1a0a94a4-e6b7-406c-9a41-3af1b5041eee"), "users.create", "Crear usuarios y reenviar invitaciones.", "Crear usuarios" },
                    { new Guid("329bf5b9-e1df-49cd-9f42-27f523a1eb1c"), "sessions.read", "Consultar las sesiones activas de otros usuarios.", "Ver sesiones" },
                    { new Guid("7ae90271-e5e3-4573-adca-3b55657e7c2a"), "sessions.revoke", "Cerrar todas las sesiones de otro usuario.", "Revocar sesiones" },
                    { new Guid("82f841fc-d993-4c8f-9463-700fe5e8f944"), "users.update", "Desactivar y reactivar usuarios.", "Modificar usuarios" },
                    { new Guid("9769750d-a668-463d-a7b1-91dae751524f"), "users.read", "Consultar la lista de usuarios y su detalle.", "Ver usuarios" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "roles",
                columns: new[] { "id", "role_description", "role_name" },
                values: new object[] { new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11"), "Administrador con todos los permisos del módulo Identity.", "Admin" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "roles_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("1a0a94a4-e6b7-406c-9a41-3af1b5041eee"), new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11") },
                    { new Guid("329bf5b9-e1df-49cd-9f42-27f523a1eb1c"), new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11") },
                    { new Guid("7ae90271-e5e3-4573-adca-3b55657e7c2a"), new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11") },
                    { new Guid("82f841fc-d993-4c8f-9463-700fe5e8f944"), new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11") },
                    { new Guid("9769750d-a668-463d-a7b1-91dae751524f"), new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11") }
                });

            migrationBuilder.CreateIndex(
                name: "idx_outbox_messages_pending",
                schema: "identity",
                table: "outbox_messages",
                column: "occurred_at",
                filter: "processed_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "identity");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "roles_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("1a0a94a4-e6b7-406c-9a41-3af1b5041eee"), new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11") });

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "roles_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("329bf5b9-e1df-49cd-9f42-27f523a1eb1c"), new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11") });

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "roles_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("7ae90271-e5e3-4573-adca-3b55657e7c2a"), new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11") });

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "roles_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("82f841fc-d993-4c8f-9463-700fe5e8f944"), new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11") });

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "roles_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("9769750d-a668-463d-a7b1-91dae751524f"), new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11") });

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("1a0a94a4-e6b7-406c-9a41-3af1b5041eee"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("329bf5b9-e1df-49cd-9f42-27f523a1eb1c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("7ae90271-e5e3-4573-adca-3b55657e7c2a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("82f841fc-d993-4c8f-9463-700fe5e8f944"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("9769750d-a668-463d-a7b1-91dae751524f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("8f3c2a1e-5b7d-4c9a-9e21-3d4f6a8b0c11"));
        }
    }
}
