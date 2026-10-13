using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalJobPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddJobVacancyCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FilledVacancies",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalVacancies",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilledVacancies",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "TotalVacancies",
                table: "Jobs");
        }
    }
}