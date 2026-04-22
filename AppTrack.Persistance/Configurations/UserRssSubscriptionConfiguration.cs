using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class UserRssSubscriptionConfiguration : IEntityTypeConfiguration<UserRssSubscription>
{
    public void Configure(EntityTypeBuilder<UserRssSubscription> builder)
    {
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => new { x.UserId, x.RssPortalId }).IsUnique();
        builder.HasOne(x => x.RssPortal)
            .WithMany()
            .HasForeignKey(x => x.RssPortalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
