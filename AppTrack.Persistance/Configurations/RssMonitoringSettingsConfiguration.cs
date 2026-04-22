using System.Text.Json;
using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class RssMonitoringSettingsConfiguration : IEntityTypeConfiguration<RssMonitoringSettings>
{
    public void Configure(EntityTypeBuilder<RssMonitoringSettings> builder)
    {
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.Property(x => x.Keywords)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
        builder.Property(x => x.NotificationEmail).IsRequired().HasMaxLength(500);
    }
}
