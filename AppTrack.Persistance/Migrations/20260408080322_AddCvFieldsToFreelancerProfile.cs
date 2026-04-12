using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddCvFieldsToFreelancerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CvBlobPath",
                table: "FreelancerProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvFileName",
                table: "FreelancerProfiles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvText",
                table: "FreelancerProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvBlobPath",
                table: "FreelancerProfiles");

            migrationBuilder.DropColumn(
                name: "CvFileName",
                table: "FreelancerProfiles");

            migrationBuilder.DropColumn(
                name: "CvText",
                table: "FreelancerProfiles");
        }
    }
}
