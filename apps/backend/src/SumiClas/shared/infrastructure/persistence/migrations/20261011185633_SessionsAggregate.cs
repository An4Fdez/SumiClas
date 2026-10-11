using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SumiClas.Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SessionsAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO "Sessions" ("Id", "UserId", "CreatedAt", "RevokedAt")
                SELECT "FamilyId", (array_agg("UserId"))[1], MIN("CreatedAt"), MAX("RevokedAt")
                FROM "RefreshTokens"
                GROUP BY "FamilyId";
                """);

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "FamilyId",
                table: "RefreshTokens",
                newName: "SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_FamilyId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Sessions_SessionId",
                table: "RefreshTokens",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Sessions_SessionId",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "RefreshTokens",
                newName: "FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_SessionId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_FamilyId");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "RefreshTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RevokedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "RefreshTokens" AS t
                SET "UserId" = s."UserId", "RevokedAt" = s."RevokedAt"
                FROM "Sessions" AS s
                WHERE s."Id" = t."FamilyId";
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "RefreshTokens",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
