using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFreelancermapSortUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [ProjectPortals]
                SET [Url] = N'https://www.freelancermap.de/projekte?countries%5B%5D=1&sort=1&pagenr=1'
                WHERE [Name] = N'Freelancermap' AND [Url] = N'https://www.freelancermap.de/projekte';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [ProjectPortals]
                SET [Url] = N'https://www.freelancermap.de/projekte'
                WHERE [Name] = N'Freelancermap' AND [Url] = N'https://www.freelancermap.de/projekte?countries%5B%5D=1&sort=1&pagenr=1';
                """);
        }
    }
}
