using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class ProjectPortalConfiguration : IEntityTypeConfiguration<ProjectPortal>
{
    public void Configure(EntityTypeBuilder<ProjectPortal> builder)
    {
        builder.ToTable("ProjectPortals");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Url).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.ScraperType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasData(
            new ProjectPortal
            {
                Id = 1,
                Name = "Freelancermap",
                Url = "https://www.freelancermap.de/projekte",
                ScraperType = ScraperType.FreelancerMap,
                IsActive = true
            });
    }
}
