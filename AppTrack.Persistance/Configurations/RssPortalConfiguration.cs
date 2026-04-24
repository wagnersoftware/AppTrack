using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class RssPortalConfiguration : IEntityTypeConfiguration<RssPortal>
{
    public void Configure(EntityTypeBuilder<RssPortal> builder)
    {
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Url).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.ParserType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasData(
            new RssPortal
            {
                Id = 1,
                Name = "Freelancermap",
                Url = "https://freelancermap.de",
                ParserType = RssParserType.FreelancerMap,
                IsActive = true
            });
    }
}
