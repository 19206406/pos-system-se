using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity  ");

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "identity  ",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    permission_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    permission_description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    identifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "identity  ",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    role_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role_description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity  ",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    job_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    hash_password = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles_permissions",
                schema: "identity  ",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles_permissions", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_roles_permissions_permissions",
                        column: x => x.permission_id,
                        principalSchema: "identity  ",
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_roles_permissions_roles",
                        column: x => x.role_id,
                        principalSchema: "identity  ",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_tokens",
                schema: "identity  ",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    token_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_password_tokens", x => x.id);
                    table.CheckConstraint("chk_password_tokens_token_type", "token_type IN ('invite', 'reset')");
                    table.ForeignKey(
                        name: "fk_password_tokens_users",
                        column: x => x.user_id,
                        principalSchema: "identity  ",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                schema: "identity  ",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    device_info = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_sessions_replaced_by",
                        column: x => x.replaced_by_id,
                        principalSchema: "identity  ",
                        principalTable: "sessions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_sessions_users",
                        column: x => x.user_id,
                        principalSchema: "identity  ",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_roles",
                schema: "identity  ",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_users_roles_roles",
                        column: x => x.role_id,
                        principalSchema: "identity  ",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_users_roles_users",
                        column: x => x.user_id,
                        principalSchema: "identity  ",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_password_tokens_user_id",
                schema: "identity  ",
                table: "password_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_password_tokens_token_hash",
                schema: "identity  ",
                table: "password_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_permissions_identifier",
                schema: "identity  ",
                table: "permissions",
                column: "identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_roles_role_name",
                schema: "identity  ",
                table: "roles",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_permissions_permission_id",
                schema: "identity  ",
                table: "roles_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "idx_sessions_user_id",
                schema: "identity  ",
                table: "sessions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_replaced_by_id",
                schema: "identity  ",
                table: "sessions",
                column: "replaced_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_user_id",
                schema: "identity  ",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_sessions_token_hash",
                schema: "identity  ",
                table: "sessions",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_users_email",
                schema: "identity  ",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_roles_role_id",
                schema: "identity  ",
                table: "users_roles",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "password_tokens",
                schema: "identity  ");

            migrationBuilder.DropTable(
                name: "roles_permissions",
                schema: "identity  ");

            migrationBuilder.DropTable(
                name: "sessions",
                schema: "identity  ");

            migrationBuilder.DropTable(
                name: "users_roles",
                schema: "identity  ");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "identity  ");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "identity  ");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity  ");
        }
    }
}
