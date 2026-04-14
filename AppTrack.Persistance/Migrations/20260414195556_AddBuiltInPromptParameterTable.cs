using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddBuiltInPromptParameterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuiltInPromptParameter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AiSettingsId = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuiltInPromptParameter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuiltInPromptParameter_AiSettings_AiSettingsId",
                        column: x => x.AiSettingsId,
                        principalTable: "AiSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuiltInPromptParameter_AiSettingsId_Key",
                table: "BuiltInPromptParameter",
                columns: new[] { "AiSettingsId", "Key" },
                unique: true);

            // Migrate existing builtIn_ rows from PromptParameter to BuiltInPromptParameter
            migrationBuilder.Sql(@"
                INSERT INTO BuiltInPromptParameter ([Key], [Value], AiSettingsId, CreationDate, ModifiedDate)
                SELECT [Key], [Value], AISettingsId, CreationDate, ModifiedDate
                FROM PromptParameter
                WHERE [Key] LIKE 'builtIn_%'
            ");

            // Remove builtIn_ rows from PromptParameter
            migrationBuilder.Sql("DELETE FROM PromptParameter WHERE [Key] LIKE 'builtIn_%'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Move builtIn_ rows back to PromptParameter before dropping the table
            migrationBuilder.Sql(@"
                INSERT INTO PromptParameter ([Key], [Value], AISettingsId, CreationDate, ModifiedDate)
                SELECT [Key], [Value], AiSettingsId, CreationDate, ModifiedDate
                FROM BuiltInPromptParameter
            ");

            migrationBuilder.DropTable(
                name: "BuiltInPromptParameter");
        }
    }
}
