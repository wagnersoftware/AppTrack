using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class ProcessedFeedItemConfiguration : IEntityTypeConfiguration<ProcessedFeedItem>
{
    public void Configure(EntityTypeBuilder<ProcessedFeedItem> builder)
    {
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.FeedItemUrl).IsRequired().HasMaxLength(2000);
        builder.HasIndex(x => new { x.UserId, x.FeedItemUrl }).IsUnique();
    }
}
