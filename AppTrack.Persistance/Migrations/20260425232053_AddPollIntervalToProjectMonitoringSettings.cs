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
            // ProjectMonitoringSettings was dropped by AddProjectScraping and recreated
            // (without these columns) by AddUserProjectMatch. If the table no longer exists
            // — which is the case when this migration is applied out-of-order relative to
            // AddProjectScraping — we skip the ALTER TABLE to avoid a runtime failure.
            migrationBuilder.Sql("""
                IF OBJECT_ID('ProjectMonitoringSettings', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ProjectMonitoringSettings') AND name = N'LastPolledAt')
                        ALTER TABLE [ProjectMonitoringSettings] ADD [LastPolledAt] datetime2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ProjectMonitoringSettings') AND name = N'PollIntervalMinutes')
                        ALTER TABLE [ProjectMonitoringSettings] ADD [PollIntervalMinutes] int NOT NULL DEFAULT 60;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID('ProjectMonitoringSettings', 'U') IS NOT NULL
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ProjectMonitoringSettings') AND name = N'LastPolledAt')
                        ALTER TABLE [ProjectMonitoringSettings] DROP COLUMN [LastPolledAt];
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ProjectMonitoringSettings') AND name = N'PollIntervalMinutes')
                        ALTER TABLE [ProjectMonitoringSettings] DROP COLUMN [PollIntervalMinutes];
                END
                """);
        }
    }
}
