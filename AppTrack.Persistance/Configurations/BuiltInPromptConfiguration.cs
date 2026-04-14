using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class BuiltInPromptConfiguration : IEntityTypeConfiguration<BuiltInPrompt>
{
    public void Configure(EntityTypeBuilder<BuiltInPrompt> builder)
    {
        // Keep the existing SQL table name to avoid a breaking schema migration.
        builder.ToTable("DefaultPrompts");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PromptTemplate)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(
            Seed(1, "Default_Cover_Letter",
                "Write a professional cover letter for the {Position} position at {Company}. Job description: {JobDescription}"),
            Seed(2, "Default_LinkedIn_Message",
                "Write a short LinkedIn message to {ContactPerson} regarding the {Position} position at {Company}."),
            Seed(3, "Default_Introduction",
                "Introduce me in a few sentences as an applicant for the {Position} position at {Company}."),
            Seed(4, "Default_Follow_Up",
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
