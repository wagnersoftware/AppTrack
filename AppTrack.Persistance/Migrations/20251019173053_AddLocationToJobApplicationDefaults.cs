using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToJobApplicationDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "JobApplicationDefaults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "JobApplicationDefaults");
        }
    }
}
