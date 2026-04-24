using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifyByEmailToRssMonitoringSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotifyByEmail",
                table: "RssMonitoringSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyByEmail",
                table: "RssMonitoringSettings");
        }
    }
}
