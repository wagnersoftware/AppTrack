using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddPollIntervalToProjectMonitoringSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPolledAt",
                table: "ProjectMonitoringSettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PollIntervalMinutes",
                table: "ProjectMonitoringSettings",
                type: "int",
                nullable: false,
                defaultValue: 60);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPolledAt",
                table: "ProjectMonitoringSettings");

            migrationBuilder.DropColumn(
                name: "PollIntervalMinutes",
                table: "ProjectMonitoringSettings");
        }
    }
}
