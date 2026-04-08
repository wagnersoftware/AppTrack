using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class MoveLanguageToAiSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "FreelancerProfiles");

            migrationBuilder.AddColumn<int>(
                name: "Language",
                table: "AiSettings",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "AiSettings");

            migrationBuilder.AddColumn<int>(
                name: "Language",
                table: "FreelancerProfiles",
                type: "int",
                nullable: true);
        }
    }
}
