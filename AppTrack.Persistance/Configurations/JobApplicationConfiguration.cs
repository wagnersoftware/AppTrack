using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.URL)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ContactPerson)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.StartDate)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(x => x.DurationInMonths)
            .HasMaxLength(10);
    }
}
