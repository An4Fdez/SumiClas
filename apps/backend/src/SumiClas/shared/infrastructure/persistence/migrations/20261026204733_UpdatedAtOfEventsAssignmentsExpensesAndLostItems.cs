using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SumiClas.Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedAtOfEventsAssignmentsExpensesAndLostItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "LostItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ClassExpenses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CalendarEvents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Assignments",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LostItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ClassExpenses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Assignments");
        }
    }
}
