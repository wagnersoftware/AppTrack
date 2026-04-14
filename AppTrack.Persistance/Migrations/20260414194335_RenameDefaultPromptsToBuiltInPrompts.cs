using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class RenameDefaultPromptsToBuiltInPrompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DefaultPrompts",
                table: "DefaultPrompts");

            migrationBuilder.RenameTable(
                name: "DefaultPrompts",
                newName: "BuiltInPrompts");

            migrationBuilder.RenameIndex(
                name: "IX_DefaultPrompts_Name",
                table: "BuiltInPrompts",
                newName: "IX_BuiltInPrompts_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BuiltInPrompts",
                table: "BuiltInPrompts",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BuiltInPrompts",
                table: "BuiltInPrompts");

            migrationBuilder.RenameTable(
                name: "BuiltInPrompts",
                newName: "DefaultPrompts");

            migrationBuilder.RenameIndex(
                name: "IX_BuiltInPrompts_Name",
                table: "DefaultPrompts",
                newName: "IX_DefaultPrompts_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DefaultPrompts",
                table: "DefaultPrompts",
                column: "Id");
        }
    }
}
