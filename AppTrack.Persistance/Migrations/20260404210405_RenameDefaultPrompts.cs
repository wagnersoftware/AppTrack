using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class RenameDefaultPrompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "Default_Cover_Letter", "Write a professional cover letter for the {Position} position at {Company}. Job description: {JobDescription}" });

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "Default_LinkedIn_Message", "Write a short LinkedIn message to {ContactPerson} regarding the {Position} position at {Company}." });

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "Default_Introduction", "Introduce me in a few sentences as an applicant for the {Position} position at {Company}." });

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "Default_Follow_Up", "Write a short follow-up email to {ContactPerson} regarding my application for the {Position} position at {Company}." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "Anschreiben", "Schreibe ein professionelles Anschreiben für die Stelle {Position} bei {Company}. Stellenbeschreibung: {JobDescription}" });

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "LinkedIn Nachricht", "Schreibe eine kurze LinkedIn-Nachricht an {ContactPerson} bezüglich der Stelle {Position} bei {Company}." });

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "Vorstellung", "Stelle mich in ein paar Sätzen als Bewerber für die Stelle {Position} bei {Company} vor." });

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "Nachfassen", "Schreibe eine kurze Follow-up-E-Mail an {ContactPerson} bezüglich meiner Bewerbung für die Stelle {Position} bei {Company}." });
        }
    }
}
