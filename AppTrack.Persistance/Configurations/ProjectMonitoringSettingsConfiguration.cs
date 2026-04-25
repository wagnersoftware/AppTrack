using System.Text.Json;
using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class ProjectMonitoringSettingsConfiguration : IEntityTypeConfiguration<ProjectMonitoringSettings>
{
    public void Configure(EntityTypeBuilder<ProjectMonitoringSettings> builder)
    {
        builder.ToTable("ProjectMonitoringSettings");
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.Property(x => x.Keywords)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
        builder.Property(x => x.NotificationEmail).IsRequired().HasMaxLength(500);
        builder.Property(x => x.NotificationIntervalMinutes).IsRequired().HasDefaultValue(60);
        builder.Property(x => x.PollIntervalMinutes).IsRequired().HasDefaultValue(60);
        builder.Property(x => x.LastPolledAt).HasColumnType("datetime2");
    }
}
