using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class UserPortalSubscriptionConfiguration : IEntityTypeConfiguration<UserPortalSubscription>
{
    public void Configure(EntityTypeBuilder<UserPortalSubscription> builder)
    {
        builder.ToTable("UserPortalSubscriptions");
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => new { x.UserId, x.ProjectPortalId }).IsUnique();
        builder.HasOne(x => x.ProjectPortal)
            .WithMany()
            .HasForeignKey(x => x.ProjectPortalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
