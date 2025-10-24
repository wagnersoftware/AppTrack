using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedPromptParameterConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PromptParameter_AISettingsId",
                table: "PromptParameter");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PromptParameter",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_PromptParameter_AISettingsId_Key",
                table: "PromptParameter",
                columns: new[] { "AISettingsId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PromptParameter_AISettingsId_Key",
                table: "PromptParameter");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PromptParameter",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_PromptParameter_AISettingsId",
                table: "PromptParameter",
                column: "AISettingsId");
        }
    }
}
