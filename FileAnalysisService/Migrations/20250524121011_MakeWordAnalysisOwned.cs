using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileAnalisysService.Migrations
{
    /// <inheritdoc />
    public partial class MakeWordAnalysisOwned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WordAnalysis_CharacterCount",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WordAnalysis_ParagraphCount",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WordAnalysis_WordCount",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WordAnalysis_CharacterCount",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "WordAnalysis_ParagraphCount",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "WordAnalysis_WordCount",
                table: "Files");
        }
    }
}
