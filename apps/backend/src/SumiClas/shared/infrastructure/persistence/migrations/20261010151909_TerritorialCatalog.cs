using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SumiClas.Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TerritorialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LocalityId",
                table: "Schools",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Schools",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Schools",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostalCodeId",
                table: "Schools",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Localities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProvinceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostalCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character(5)", fixedLength: true, maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schools_LocalityId_NormalizedName",
                table: "Schools",
                columns: new[] { "LocalityId", "NormalizedName" });

            migrationBuilder.CreateIndex(
                name: "IX_Schools_OfficialCode",
                table: "Schools",
                column: "OfficialCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Localities_ProvinceId_NormalizedName",
                table: "Localities",
                columns: new[] { "ProvinceId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostalCodes_LocalityId_Code",
                table: "PostalCodes",
                columns: new[] { "LocalityId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_NormalizedName",
                table: "Provinces",
                column: "NormalizedName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Localities");

            migrationBuilder.DropTable(
                name: "PostalCodes");

            migrationBuilder.DropTable(
                name: "Provinces");

            migrationBuilder.DropIndex(
                name: "IX_Schools_LocalityId_NormalizedName",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Schools_OfficialCode",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "LocalityId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "PostalCodeId",
                table: "Schools");
        }
    }
}
