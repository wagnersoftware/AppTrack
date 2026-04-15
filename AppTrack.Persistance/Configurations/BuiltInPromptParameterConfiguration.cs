using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class BuiltInPromptParameterConfiguration : IEntityTypeConfiguration<BuiltInPromptParameter>
{
    public void Configure(EntityTypeBuilder<BuiltInPromptParameter> builder)
    {
        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Value)
            .IsRequired();

        builder.HasIndex(x => new { x.AiSettingsId, x.Key }) // key must be unique per AiSettings
            .IsUnique();
    }
}
