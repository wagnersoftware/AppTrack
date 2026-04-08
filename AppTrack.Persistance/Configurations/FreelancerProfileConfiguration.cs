using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class FreelancerProfileConfiguration : IEntityTypeConfiguration<FreelancerProfile>
{
    public void Configure(EntityTypeBuilder<FreelancerProfile> builder)
    {
        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.HourlyRate)
            .HasPrecision(18, 2);

        builder.Property(x => x.DailyRate)
            .HasPrecision(18, 2);

        builder.Property(x => x.Skills)
            .HasMaxLength(1000);

        builder.Property(x => x.CvBlobPath)
            .HasMaxLength(500);

        builder.Property(x => x.CvFileName)
            .HasMaxLength(255);
    }
}
