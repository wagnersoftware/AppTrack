using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProjectMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScrapedAt",
                table: "ScrapedProjects");

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

            migrationBuilder.CreateTable(
                name: "UserProjectMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ScrapedProjectId = table.Column<int>(type: "int", nullable: false),
                    IsNotified = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjectMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProjectMatches_ScrapedProjects_ScrapedProjectId",
                        column: x => x.ScrapedProjectId,
                        principalTable: "ScrapedProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateIndex(
                name: "IX_UserProjectMatches_ScrapedProjectId",
                table: "UserProjectMatches",
                column: "ScrapedProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjectMatches_UserId_ScrapedProjectId",
                table: "UserProjectMatches",
                columns: new[] { "UserId", "ScrapedProjectId" },
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
                name: "UserPortalSubscriptions");

            migrationBuilder.DropTable(
                name: "UserProjectMatches");

            migrationBuilder.AddColumn<DateTime>(
                name: "ScrapedAt",
                table: "ScrapedProjects",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
