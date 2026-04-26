using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectScraping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID('ProcessedProjectItems', 'U') IS NOT NULL DROP TABLE [ProcessedProjectItems]");
            migrationBuilder.Sql("IF OBJECT_ID('ProjectMonitoringSettings', 'U') IS NOT NULL DROP TABLE [ProjectMonitoringSettings]");
            migrationBuilder.Sql("IF OBJECT_ID('UserPortalSubscriptions', 'U') IS NOT NULL DROP TABLE [UserPortalSubscriptions]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedProjectItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectItemUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
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
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastNotifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPolledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotificationEmail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NotificationIntervalMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 60),
                    NotifyByEmail = table.Column<bool>(type: "bit", nullable: false),
                    PollIntervalMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 60),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMonitoringSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPortalSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectPortalId = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
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
                name: "IX_UserPortalSubscriptions_ProjectPortalId",
                table: "UserPortalSubscriptions",
                column: "ProjectPortalId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPortalSubscriptions_UserId_ProjectPortalId",
                table: "UserPortalSubscriptions",
                columns: new[] { "UserId", "ProjectPortalId" },
                unique: true);
        }
    }
}
