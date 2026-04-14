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
            Seed(1, "builtIn_Cover_Letter",
                "Write a professional cover letter for the {Position} position at {Company}. Job description: {JobDescription}"),
            Seed(2, "builtIn_LinkedIn_Message",
                "Write a short LinkedIn message to {ContactPerson} regarding the {Position} position at {Company}."),
            Seed(3, "builtIn_Introduction",
                "Introduce me in a few sentences as an applicant for the {Position} position at {Company}."),
            Seed(4, "builtIn_Follow_Up",
                "Write a short follow-up email to {ContactPerson} regarding my application for the {Position} position at {Company}.")
        );
    }

    private static BuiltInPrompt Seed(int id, string name, string promptTemplate)
    {
        var p = BuiltInPrompt.Create(name, promptTemplate);
        p.Id = id;
        return p;
    }
}
