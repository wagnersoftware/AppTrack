using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class ScrapingScheduleStateConfiguration : IEntityTypeConfiguration<ScrapingScheduleState>
{
    public void Configure(EntityTypeBuilder<ScrapingScheduleState> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.NextRunAfterUtc).IsRequired();
    }
}
