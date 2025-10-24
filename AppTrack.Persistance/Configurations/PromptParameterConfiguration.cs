using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class PromptParameterConfiguration : IEntityTypeConfiguration<PromptParameter>
{
    public void Configure(EntityTypeBuilder<PromptParameter> builder)
    {
        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Value)
            .IsRequired();

        builder.HasIndex(x => new { x.AISettingsId, x.Key }) // key must be unique per AISettings
            .IsUnique();
    }
}
