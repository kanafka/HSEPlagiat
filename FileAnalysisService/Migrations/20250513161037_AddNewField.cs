using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileAnalisysService.Migrations
{
    /// <inheritdoc />
    public partial class AddNewField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Files");

            migrationBuilder.AddColumn<double>(
                name: "Similarities",
                table: "Files",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Similarities",
                table: "Files");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
