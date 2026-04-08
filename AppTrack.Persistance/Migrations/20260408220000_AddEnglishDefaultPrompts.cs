using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddEnglishDefaultPrompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DefaultPrompts",
                columns: new[] { "Id", "Language", "Name", "PromptTemplate" },
                values: new object[,]
                {
                    { 5, "en", "Default_Cover_Letter", "Write a professional cover letter for the {Position} position at {Company}. Job description: {JobDescription}" },
                    { 6, "en", "Default_LinkedIn_Message", "Write a short LinkedIn message to {ContactPerson} regarding the {Position} position at {Company}." },
                    { 7, "en", "Default_Introduction", "Introduce me in a few sentences as an applicant for the {Position} position at {Company}." },
                    { 8, "en", "Default_Follow_Up", "Write a short follow-up email to {ContactPerson} regarding my application for the {Position} position at {Company}." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValues: new object[] { 5, 6, 7, 8 });
        }
    }
}
