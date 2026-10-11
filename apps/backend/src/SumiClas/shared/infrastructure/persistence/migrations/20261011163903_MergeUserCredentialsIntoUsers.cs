using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SumiClas.Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MergeUserCredentialsIntoUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PasswordChangedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Users" AS u
                SET "PasswordHash" = c."PasswordHash",
                    "PasswordChangedAt" = c."ChangedAt"
                FROM "UserCredentials" AS c
                WHERE c."Id" = u."Id";
                """);

            // A user without credential cannot log in and a hash is now required: stop here with an explicit message
            // instead of failing later on the NOT NULL change, and never invent a hash.
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "Users" WHERE "PasswordHash" IS NULL) THEN
                        RAISE EXCEPTION 'Hay usuarios sin credencial en Users. Eliminalos o registralos de nuevo antes de aplicar la migracion MergeUserCredentialsIntoUsers.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "PasswordChangedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.DropTable(
                name: "UserCredentials");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCredentials", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO "UserCredentials" ("Id", "PasswordHash", "CreatedAt", "ChangedAt")
                SELECT "Id", "PasswordHash", "PasswordChangedAt", "PasswordChangedAt"
                FROM "Users";
                """);

            migrationBuilder.DropColumn(
                name: "PasswordChangedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");
        }
    }
}
