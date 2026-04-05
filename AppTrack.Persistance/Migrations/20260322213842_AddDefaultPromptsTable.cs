using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultPromptsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DefaultPrompts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PromptTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultPrompts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DefaultPrompts",
                columns: new[] { "Id", "CreationDate", "Language", "ModifiedDate", "Name", "PromptTemplate" },
                values: new object[,]
                {
                    { 1, null, "de", null, "Anschreiben", "Schreibe ein professionelles Anschreiben für die Stelle {Position} bei {Company}. Stellenbeschreibung: {JobDescription}" },
                    { 2, null, "de", null, "LinkedIn Nachricht", "Schreibe eine kurze LinkedIn-Nachricht an {ContactPerson} bezüglich der Stelle {Position} bei {Company}." },
                    { 3, null, "de", null, "Vorstellung", "Stelle mich in ein paar Sätzen als Bewerber für die Stelle {Position} bei {Company} vor." },
                    { 4, null, "de", null, "Nachfassen", "Schreibe eine kurze Follow-up-E-Mail an {ContactPerson} bezüglich meiner Bewerbung für die Stelle {Position} bei {Company}." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefaultPrompts_Name_Language",
                table: "DefaultPrompts",
                columns: new[] { "Name", "Language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefaultPrompts");
        }
    }
}
