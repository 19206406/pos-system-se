using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxCorrelationAndInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "correlation_id",
                schema: "identity",
                table: "outbox_messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "identity",
                columns: table => new
                {
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    handler = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => new { x.message_id, x.handler });
                });

            migrationBuilder.CreateIndex(
                name: "idx_inbox_messages_processed_at",
                schema: "identity",
                table: "inbox_messages",
                column: "processed_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "identity");

            migrationBuilder.DropColumn(
                name: "correlation_id",
                schema: "identity",
                table: "outbox_messages");
        }
    }
}
