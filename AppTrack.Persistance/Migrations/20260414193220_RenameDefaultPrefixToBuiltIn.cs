using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class RenameDefaultPrefixToBuiltIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "builtIn_Cover_Letter");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "builtIn_LinkedIn_Message");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "builtIn_Introduction");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "builtIn_Follow_Up");

            // Ids 5–8 were inserted by AddEnglishDefaultPrompts (raw InsertData, not HasData),
            // so EF does not auto-generate diffs for them — rename them here manually.
            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "builtIn_Cover_Letter_en");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "builtIn_LinkedIn_Message_en");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "builtIn_Introduction_en");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "builtIn_Follow_Up_en");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Default_Cover_Letter");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Default_LinkedIn_Message");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Default_Introduction");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Default_Follow_Up");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Default_Cover_Letter");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Default_LinkedIn_Message");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Default_Introduction");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Default_Follow_Up");
        }
    }
}
