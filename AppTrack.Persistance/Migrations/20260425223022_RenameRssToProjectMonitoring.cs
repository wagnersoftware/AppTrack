using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class RenameRssToProjectMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedFeedItems");

            migrationBuilder.DropTable(
                name: "RssMonitoringSettings");

            migrationBuilder.DropTable(
                name: "UserRssSubscriptions");

            migrationBuilder.DropTable(
                name: "RssPortals");

            migrationBuilder.CreateTable(
                name: "ProcessedProjectItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProjectItemUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedProjectItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMonitoringSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotificationIntervalMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 60),
                    NotifyByEmail = table.Column<bool>(type: "bit", nullable: false),
                    NotificationEmail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LastNotifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMonitoringSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPortals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ScraperType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPortals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScrapedProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectPortalId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ScrapedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapedProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrapedProjects_ProjectPortals_ProjectPortalId",
                        column: x => x.ProjectPortalId,
                        principalTable: "ProjectPortals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPortalSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProjectPortalId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPortalSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPortalSubscriptions_ProjectPortals_ProjectPortalId",
                        column: x => x.ProjectPortalId,
                        principalTable: "ProjectPortals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ProjectPortals",
                columns: new[] { "Id", "CreationDate", "IsActive", "ModifiedDate", "Name", "ScraperType", "Url" },
                values: new object[] { 1, null, true, null, "Freelancermap", "FreelancerMap", "https://www.freelancermap.de/projekte" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedProjectItems_UserId_ProjectItemUrl",
                table: "ProcessedProjectItems",
                columns: new[] { "UserId", "ProjectItemUrl" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMonitoringSettings_UserId",
                table: "ProjectMonitoringSettings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScrapedProjects_ProjectPortalId_Url",
                table: "ScrapedProjects",
                columns: new[] { "ProjectPortalId", "Url" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPortalSubscriptions_ProjectPortalId",
                table: "UserPortalSubscriptions",
                column: "ProjectPortalId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPortalSubscriptions_UserId_ProjectPortalId",
                table: "UserPortalSubscriptions",
                columns: new[] { "UserId", "ProjectPortalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedProjectItems");

            migrationBuilder.DropTable(
                name: "ProjectMonitoringSettings");

            migrationBuilder.DropTable(
                name: "ScrapedProjects");

            migrationBuilder.DropTable(
                name: "UserPortalSubscriptions");

            migrationBuilder.DropTable(
                name: "ProjectPortals");

            migrationBuilder.CreateTable(
                name: "ProcessedFeedItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FeedItemUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
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
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotificationEmail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NotifyByEmail = table.Column<bool>(type: "bit", nullable: false),
                    PollIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
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
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParserType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
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
                    RssPortalId = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastPolledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
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
                values: new object[] { 1, null, true, null, "Freelancermap", "FreelancerMap", "https://freelancermap.de" });

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
    }
}
