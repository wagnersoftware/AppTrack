using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class BuiltInPromptConfiguration : IEntityTypeConfiguration<BuiltInPrompt>
{
    public void Configure(EntityTypeBuilder<BuiltInPrompt> builder)
    {
        builder.ToTable("BuiltInPrompts");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PromptTemplate)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(
            Seed(1, "builtIn_Application",
                "Here is my resume. Using the information you gather from it, please write a brief and professional email for the following job posting that explains why I am the perfect freelancer for this position. Only reference skills and experience explicitly listed in my CV and skills below. Do not add any skills from the job posting that are not in my profile.\n\nMy personal data:\nMy CV: {{builtIn_CvText}}\nMy Name: {{builtIn_FirstName}} {{builtIn_LastName}}\nMy Hourly Rate: {{builtIn_HourlyRate}}\nMy Daily Rate: {{builtIn_DailyRate}}\nMy Skills: {{builtIn_Skills}}\nMy Work Mode: {{builtIn_WorkMode}}\nAvailable From: {{builtIn_AvailableFrom}}\n\nJob posting:\nJob Description: {{JobDescription}}\nContact Person: {{ContactPerson}}\nPosition: {{Position}}"),
            Seed(2, "builtIn_LinkedIn_Message",
                "Write a short LinkedIn message to {{ContactPerson}} regarding the {{Position}} position at {{Company}}."),
            Seed(3, "builtIn_Introduction",
                "Introduce me in a few sentences as an applicant for the {{Position}} position at {{Company}}."),
            Seed(4, "builtIn_Follow_Up",
                "Write a short follow-up email to {{ContactPerson}} regarding my application for the {{Position}} position at {{Company}}.")
        );
    }

    private static BuiltInPrompt Seed(int id, string name, string promptTemplate)
    {
        var p = BuiltInPrompt.Create(name, promptTemplate);
        p.Id = id;
        return p;
    }
}
