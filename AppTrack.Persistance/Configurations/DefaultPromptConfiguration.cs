using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class DefaultPromptConfiguration : IEntityTypeConfiguration<DefaultPrompt>
{
    public void Configure(EntityTypeBuilder<DefaultPrompt> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PromptTemplate)
            .IsRequired();

        builder.Property(x => x.Language)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(x => new { x.Name, x.Language })
            .IsUnique();

        builder.HasData(
            Seed(1, "Anschreiben",
                "Schreibe ein professionelles Anschreiben für die Stelle {Position} bei {Company}. Stellenbeschreibung: {JobDescription}"),
            Seed(2, "LinkedIn Nachricht",
                "Schreibe eine kurze LinkedIn-Nachricht an {ContactPerson} bezüglich der Stelle {Position} bei {Company}."),
            Seed(3, "Vorstellung",
                "Stelle mich in ein paar Sätzen als Bewerber für die Stelle {Position} bei {Company} vor."),
            Seed(4, "Nachfassen",
                "Schreibe eine kurze Follow-up-E-Mail an {ContactPerson} bezüglich meiner Bewerbung für die Stelle {Position} bei {Company}.")
        );
    }

    private static DefaultPrompt Seed(int id, string name, string promptTemplate)
    {
        var p = DefaultPrompt.Create(name, promptTemplate, "de");
        p.Id = id;
        return p;
    }
}
