using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class AiSettingsConfiguration : IEntityTypeConfiguration<AiSettings>
{
    public void Configure(EntityTypeBuilder<AiSettings> builder)
    {
        builder.Property(x => x.ApiKey)
        .HasMaxLength(200);

        builder.HasMany(s => s.PromptParameter)
            .WithOne(p => p.AISettings)
            .HasForeignKey(p => p.AISettingsId)
            .OnDelete(DeleteBehavior.Cascade);
        }
}
