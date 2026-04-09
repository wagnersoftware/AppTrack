using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLanguageFromDefaultPrompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DefaultPrompts_Name_Language",
                table: "DefaultPrompts");

            migrationBuilder.DeleteData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "Language",
                table: "DefaultPrompts");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultPrompts_Name",
                table: "DefaultPrompts",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DefaultPrompts_Name",
                table: "DefaultPrompts");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "DefaultPrompts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Language",
                value: "de");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Language",
                value: "de");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 3,
                column: "Language",
                value: "de");

            migrationBuilder.UpdateData(
                table: "DefaultPrompts",
                keyColumn: "Id",
                keyValue: 4,
                column: "Language",
                value: "de");

            migrationBuilder.InsertData(
                table: "DefaultPrompts",
                columns: new[] { "Id", "CreationDate", "Language", "ModifiedDate", "Name", "PromptTemplate" },
                values: new object[,]
                {
                    { 5, null, "en", null, "Default_Cover_Letter", "Write a professional cover letter for the {Position} position at {Company}. Job description: {JobDescription}" },
                    { 6, null, "en", null, "Default_LinkedIn_Message", "Write a short LinkedIn message to {ContactPerson} regarding the {Position} position at {Company}." },
                    { 7, null, "en", null, "Default_Introduction", "Introduce me in a few sentences as an applicant for the {Position} position at {Company}." },
                    { 8, null, "en", null, "Default_Follow_Up", "Write a short follow-up email to {ContactPerson} regarding my application for the {Position} position at {Company}." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefaultPrompts_Name_Language",
                table: "DefaultPrompts",
                columns: new[] { "Name", "Language" },
                unique: true);
        }
    }
}
