using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddRssFeedMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedFeedItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FeedItemUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedFeedItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RssMonitoringSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PollIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    NotificationEmail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RssMonitoringSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RssPortals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ParserType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RssPortals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRssSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RssPortalId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastPolledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRssSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRssSubscriptions_RssPortals_RssPortalId",
                        column: x => x.RssPortalId,
                        principalTable: "RssPortals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "RssPortals",
                columns: new[] { "Id", "CreationDate", "IsActive", "ModifiedDate", "Name", "ParserType", "Url" },
                values: new object[] { 1, null, true, null, "Stepstone", "Stepstone", "https://www.stepstone.de/rss/stellenangebote.html" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedFeedItems_UserId_FeedItemUrl",
                table: "ProcessedFeedItems",
                columns: new[] { "UserId", "FeedItemUrl" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RssMonitoringSettings_UserId",
                table: "RssMonitoringSettings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRssSubscriptions_RssPortalId",
                table: "UserRssSubscriptions",
                column: "RssPortalId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRssSubscriptions_UserId_RssPortalId",
                table: "UserRssSubscriptions",
                columns: new[] { "UserId", "RssPortalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedFeedItems");

            migrationBuilder.DropTable(
                name: "RssMonitoringSettings");

            migrationBuilder.DropTable(
                name: "UserRssSubscriptions");

            migrationBuilder.DropTable(
                name: "RssPortals");
        }
    }
}
