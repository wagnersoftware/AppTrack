using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedChatModelsSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ChatModels",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "ChatModels",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiModelName", "Description", "Name" },
                values: new object[] { "gpt-4o-mini", "Lightweight model for quick suggestions or interactive text generation", "ChatGPT 4 Mini" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ChatModels",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiModelName", "Description", "Name" },
                values: new object[] { "gpt-4-32k", "Handles long documents, perfect for extensive resumes or detailed cover letters", "ChatGPT 4 (32k)" });

            migrationBuilder.InsertData(
                table: "ChatModels",
                columns: new[] { "Id", "ApiModelName", "CreationDate", "Description", "ModifiedDate", "Name" },
                values: new object[] { 4, "gpt-4o-mini", null, "Lightweight model for quick suggestions or interactive text generation", null, "ChatGPT 4 Mini" });
        }
    }
}
