using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChatModelsSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ChatModels",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "IsActive" },
                values: new object[] { "Legacy model. Not recommended for CV analysis or job matching due to limited reasoning capabilities.", false });

            migrationBuilder.UpdateData(
                table: "ChatModels",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "IsActive" },
                values: new object[] { "High-quality model for complex analysis, CV parsing, job matching and advanced reasoning. Recommended for accurate evaluations and structured outputs.", false });

            migrationBuilder.UpdateData(
                table: "ChatModels",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiModelName", "Description", "IsActive", "Name" },
                values: new object[] { "gpt-4o", "Best balance of quality and speed, ideal for CV analysis, job matching and complex reasoning", true, "GPT-4o" });

            migrationBuilder.InsertData(
                table: "ChatModels",
                columns: new[] { "Id", "ApiModelName", "CreationDate", "Description", "IsActive", "ModifiedDate", "Name" },
                values: new object[,]
                {
                    { 4, "gpt-4o-mini", null, "Fast and cost-efficient model for simple tasks like rephrasing, summaries, and draft generation. Not suitable for deep analysis.", true, null, "ChatGPT 4 Mini" },
                    { 5, "gpt-5", null, "Top-tier reasoning model for critical evaluations and high-stakes decisions", true, null, "GPT-5" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ChatModels",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ChatModels",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "ChatModels",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "IsActive" },
                values: new object[] { "Fast model, suitable for short text snippets and suggestions", true });

            migrationBuilder.UpdateData(
                table: "ChatModels",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "IsActive" },
                values: new object[] { "High-precision model, ideal for complex cover letters and refined writing", true });

            migrationBuilder.UpdateData(
                table: "ChatModels",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiModelName", "Description", "IsActive", "Name" },
                values: new object[] { "gpt-4o-mini", "Lightweight model for quick suggestions or interactive text generation", false, "ChatGPT 4 Mini" });
        }
    }
}
