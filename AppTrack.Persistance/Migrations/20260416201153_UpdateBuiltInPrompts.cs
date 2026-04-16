using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBuiltInPrompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "builtIn_GenerateApplicationText");

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "builtIn_JobMatchAnalysis", "Analyze how well my profile matches the following job posting. Be honest and direct — do NOT inflate the match score or soften weaknesses. If I am missing required skills, say so explicitly.\n\nYour response must cover:\n1. Match score: A percentage (0–100) with a one-sentence justification\n2. Skills I have that match: Reference my CV and skills list specifically\n3. Skills or experience I am missing: Be specific — what does the job require that I lack?\n4. Work mode / location fit: Is {{Location}} compatible with my preferred work mode ({{builtIn_WorkMode}})?\n5. Verdict: Should I apply? What are the main risks or gaps I would need to address?\n\nMy profile:\nCV: {{builtIn_CvText}}\nSkills: {{builtIn_Skills}}\nWork Mode Preference: {{builtIn_WorkMode}}\n\nJob posting:\nPosition: {{Position}}\nLocation: {{Location}}\nDuration: {{DurationInMonths}} months\nStart Date: {{StartDate}}\nJob Description: {{JobDescription}}" });

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "builtIn_InterviewPrep", "Prepare me for an interview for the {{Position}} role. Base all questions strictly on the job description — do not generate generic filler questions.\n\nProvide:\n1. 5–7 role-specific or technical questions the interviewer is likely to ask based on the actual requirements\n2. 2–3 behavioral questions relevant to this type of role\n3. For each question: a suggested answer approach based on my CV and skills. If my experience in a relevant area is weak, say so explicitly and suggest how to address it honestly rather than inventing strengths\n4. 2 questions I should ask the interviewer to evaluate whether this project is genuinely worth taking\n\nMy profile:\nMy Name: {{builtIn_FirstName}} {{builtIn_LastName}}\nMy CV: {{builtIn_CvText}}\nMy Skills: {{builtIn_Skills}}\n\nJob posting:\nPosition: {{Position}}\nLocation: {{Location}}\nDuration: {{DurationInMonths}} months\nStart Date: {{StartDate}}\nJob Description: {{JobDescription}}" });

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "builtIn_RateAnalysis", "Based on the job description and my CV, analyze what daily and hourly rate I can realistically charge for this role.\n\nYour response must include:\n1. Recommended rate range: A realistic daily rate range (minimum / target / optimal) I can ask for given my qualifications and the role requirements\n2. Justification: Why this range — reference specific skills from my CV that match or strengthen my position, and be honest about any gaps that would weaken it\n3. Minimum rate: The lowest daily rate I should accept for this role without underselling myself — explain why going below this would not be appropriate\n4. Rate risk factors: Are there aspects of my profile (e.g. missing skills, overqualification, location/remote constraints) that could affect what I can realistically negotiate?\n\nBe honest and direct. If my qualifications do not fully match the role, reflect that in a lower range rather than inflating the recommendation.\n\nMy profile:\nMy CV: {{builtIn_CvText}}\nMy Skills: {{builtIn_Skills}}\nMy Current Daily Rate: {{builtIn_DailyRate}}\nMy Current Hourly Rate: {{builtIn_HourlyRate}}\nWork Mode: {{builtIn_WorkMode}}\n\nJob posting:\nPosition: {{Position}}\nLocation: {{Location}}\nDuration: {{DurationInMonths}} months\nStart Date: {{StartDate}}\nJob Description: {{JobDescription}}" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "builtIn_Application");

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "builtIn_LinkedIn_Message", "Write a short LinkedIn message to {{ContactPerson}} regarding the {{Position}} position at {{Company}}." });

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "builtIn_Introduction", "Introduce me in a few sentences as an applicant for the {{Position}} position at {{Company}}." });

            migrationBuilder.UpdateData(
                table: "BuiltInPrompts",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "PromptTemplate" },
                values: new object[] { "builtIn_Follow_Up", "Write a short follow-up email to {{ContactPerson}} regarding my application for the {{Position}} position at {{Company}}." });
        }
    }
}
