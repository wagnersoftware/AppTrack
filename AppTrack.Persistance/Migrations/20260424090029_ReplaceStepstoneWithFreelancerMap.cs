using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceStepstoneWithFreelancerMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "RssPortals",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "ParserType", "Url" },
                values: new object[] { "Freelancermap", "FreelancerMap", "https://freelancermap.de" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "RssPortals",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "ParserType", "Url" },
                values: new object[] { "Stepstone", "Stepstone", "https://www.stepstone.de/rss/stellenangebote.html" });
        }
    }
}
