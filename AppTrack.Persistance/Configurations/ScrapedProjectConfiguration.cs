using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class ScrapedProjectConfiguration : IEntityTypeConfiguration<ScrapedProject>
{
    public void Configure(EntityTypeBuilder<ScrapedProject> builder)
    {
        builder.ToTable("ScrapedProjects");
        builder.Property(x => x.Title).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Url).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.CompanyName).HasMaxLength(300);
        builder.HasIndex(x => new { x.ProjectPortalId, x.Url }).IsUnique();
        builder.HasOne(x => x.ProjectPortal)
            .WithMany()
            .HasForeignKey(x => x.ProjectPortalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
