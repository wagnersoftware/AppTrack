using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddedChatModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ApiModelName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatModels", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ChatModels",
                columns: new[] { "Id", "ApiModelName", "CreationDate", "Description", "IsActive", "ModifiedDate", "Name" },
                values: new object[,]
                {
                    { 1, "gpt-3.5-turbo", null, "Fast model, suitable for short text snippets and suggestions", true, null, "ChatGPT 3.5" },
                    { 2, "gpt-4", null, "High-precision model, ideal for complex cover letters and refined writing", true, null, "ChatGPT 4" }
                });

            migrationBuilder.InsertData(
                table: "ChatModels",
                columns: new[] { "Id", "ApiModelName", "CreationDate", "Description", "ModifiedDate", "Name" },
                values: new object[,]
                {
                    { 3, "gpt-4-32k", null, "Handles long documents, perfect for extensive resumes or detailed cover letters", null, "ChatGPT 4 (32k)" },
                    { 4, "gpt-4o-mini", null, "Lightweight model for quick suggestions or interactive text generation", null, "ChatGPT 4 Mini" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatModels");
        }
    }
}
