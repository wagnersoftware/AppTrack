using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTrack.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class RenamePromptNameToPromptKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PromptName",
                table: "JobApplicationAiTexts",
                newName: "PromptKey");

            migrationBuilder.RenameIndex(
                name: "IX_JobApplicationAiTexts_JobApplicationId_PromptName",
                table: "JobApplicationAiTexts",
                newName: "IX_JobApplicationAiTexts_JobApplicationId_PromptKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PromptKey",
                table: "JobApplicationAiTexts",
                newName: "PromptName");

            migrationBuilder.RenameIndex(
                name: "IX_JobApplicationAiTexts_JobApplicationId_PromptKey",
                table: "JobApplicationAiTexts",
                newName: "IX_JobApplicationAiTexts_JobApplicationId_PromptName");
        }
    }
}
