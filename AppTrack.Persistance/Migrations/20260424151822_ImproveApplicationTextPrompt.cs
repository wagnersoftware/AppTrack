using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class ImproveApplicationTextPrompt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 1,
                column: "PromptTemplate",
                value: "Write a concise, authentic application email for the following freelance position.\n\nRules:\n- Sound professional but genuine — avoid clichés and generic phrases like \"I am the perfect fit\", \"highly motivated\", \"results-oriented\", \"passionate about\", or similar hollow filler\n- Connect my profile directly to the specific requirements in the job description — do NOT just list my skills, but explain HOW they are relevant to this role\n- Reference specific aspects of the job posting (required technologies, project type, challenges mentioned) and show how my experience addresses them\n- Keep it brief: 3–4 short paragraphs at most\n- Only use skills and experience explicitly listed in my CV and skills — do not invent skills\n- Begin with a strong, specific opening sentence that references this position concretely — not a generic \"I am applying for...\"\n\nMy profile:\nMy CV: {{builtIn_CvText}}\nMy Name: {{builtIn_FirstName}} {{builtIn_LastName}}\nMy Hourly Rate: {{builtIn_HourlyRate}}\nMy Daily Rate: {{builtIn_DailyRate}}\nMy Skills: {{builtIn_Skills}}\nMy Work Mode: {{builtIn_WorkMode}}\nAvailable From: {{builtIn_AvailableFrom}}\n\nJob posting:\nJob Description: {{JobDescription}}\nContact Person: {{ContactPerson}}\nPosition: {{Position}}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 1,
                column: "PromptTemplate",
                value: "Here is my resume. Using the information you gather from it, please write a brief and professional email for the following job posting that explains why I am the perfect freelancer for this position. Only reference skills and experience explicitly listed in my CV and skills below. Do not add any skills from the job posting that are not in my profile.\n\nMy personal data:\nMy CV: {{builtIn_CvText}}\nMy Name: {{builtIn_FirstName}} {{builtIn_LastName}}\nMy Hourly Rate: {{builtIn_HourlyRate}}\nMy Daily Rate: {{builtIn_DailyRate}}\nMy Skills: {{builtIn_Skills}}\nMy Work Mode: {{builtIn_WorkMode}}\nAvailable From: {{builtIn_AvailableFrom}}\n\nJob posting:\nJob Description: {{JobDescription}}\nContact Person: {{ContactPerson}}\nPosition: {{Position}}");
        }
    }
}
