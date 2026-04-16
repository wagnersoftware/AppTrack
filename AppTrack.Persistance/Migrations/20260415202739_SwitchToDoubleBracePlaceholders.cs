using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class SwitchToDoubleBracePlaceholders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [Prompt]
                SET [PromptTemplate] = REPLACE(REPLACE([PromptTemplate], '{', '{{'), '}', '}}')
                WHERE [PromptTemplate] LIKE '%{%}%'
            ");

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 1,
                column: "PromptTemplate",
                value: "Here is my resume. Using the information you gather from it, please write a brief and professional application for the following job posting that explains why I am the perfect freelancer for this position.\n\nMy personal data:\nMy CV: {{builtIn_CvText}}\nMy Name: {{builtIn_FirstName}} {{builtIn_LastName}}\nMy Hourly Rate: {{builtIn_HourlyRate}}\nMy Daily Rate: {{builtIn_DailyRate}}\nMy Skills: {{builtIn_Skills}}\nMy Work Mode: {{builtIn_WorkMode}}\nAvailable From: {{builtIn_AvailableFrom}}\n\nJob posting:\nJob Description: {{JobDescription}}\nContact Person: {{ContactPerson}}\nPosition: {{Position}}");

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 2,
                column: "PromptTemplate",
                value: "Write a short LinkedIn message to {{ContactPerson}} regarding the {{Position}} position at {{Company}}.");

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 3,
                column: "PromptTemplate",
                value: "Introduce me in a few sentences as an applicant for the {{Position}} position at {{Company}}.");

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 4,
                column: "PromptTemplate",
                value: "Write a short follow-up email to {{ContactPerson}} regarding my application for the {{Position}} position at {{Company}}.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 1,
                column: "PromptTemplate",
                value: "Here is my resume. Using the information you gather from it, please write a brief and professional application for the following job posting that explains why I am the perfect freelancer for this position.\n\nMy personal data:\nMy CV: {builtIn_CvText}\nMy Name: {builtIn_FirstName} {builtIn_LastName}\nMy Hourly Rate: {builtIn_HourlyRate}\nMy Daily Rate: {builtIn_DailyRate}\nMy Skills: {builtIn_Skills}\nMy Work Mode: {builtIn_WorkMode}\nAvailable From: {builtIn_AvailableFrom}\n\nJob posting:\nJob Description: {JobDescription}\nContact Person: {ContactPerson}\nPosition: {Position}");

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 2,
                column: "PromptTemplate",
                value: "Write a short LinkedIn message to {ContactPerson} regarding the {Position} position at {Company}.");

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 3,
                column: "PromptTemplate",
                value: "Introduce me in a few sentences as an applicant for the {Position} position at {Company}.");

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 4,
                column: "PromptTemplate",
                value: "Write a short follow-up email to {ContactPerson} regarding my application for the {Position} position at {Company}.");

            migrationBuilder.Sql(@"
                UPDATE [Prompt]
                SET [PromptTemplate] = REPLACE(REPLACE([PromptTemplate], '{{', '{'), '}}', '}')
                WHERE [PromptTemplate] LIKE '%{{%}}%'
            ");
        }
    }
}
