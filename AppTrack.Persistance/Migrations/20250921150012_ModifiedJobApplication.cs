using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedJobApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "JobApplications");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "JobApplications",
                newName: "URL");

            migrationBuilder.RenameColumn(
                name: "FollowUpDate",
                table: "JobApplications",
                newName: "ModifiedDate");

            migrationBuilder.RenameColumn(
                name: "Client",
                table: "JobApplications",
                newName: "ClientName");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AppliedDate",
                table: "JobApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "JobApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "JobApplications",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApplicationText", "AppliedDate", "CreationDate", "Position", "Status", "URL" },
                values: new object[] { "ApplicationText1", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Developer1", 2, "www.testURL.de" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "JobApplications");

            migrationBuilder.RenameColumn(
                name: "URL",
                table: "JobApplications",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "ModifiedDate",
                table: "JobApplications",
                newName: "FollowUpDate");

            migrationBuilder.RenameColumn(
                name: "ClientName",
                table: "JobApplications",
                newName: "Client");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AppliedDate",
                table: "JobApplications",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "JobApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "JobApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "JobApplications",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApplicationText", "AppliedDate", "DateCreated", "DateModified", "Notes", "Position", "Status", "Title" },
                values: new object[] { "", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "", 0, "" });
        }
    }
}
