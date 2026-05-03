using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class UserProjectMatchConfiguration : IEntityTypeConfiguration<UserProjectMatch>
{
    public void Configure(EntityTypeBuilder<UserProjectMatch> builder)
    {
        builder.ToTable("UserProjectMatches");
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(450);
        builder.HasIndex(x => new { x.UserId, x.ScrapedProjectId }).IsUnique();
        builder.HasOne(x => x.ScrapedProject)
            .WithMany()
            .HasForeignKey(x => x.ScrapedProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
