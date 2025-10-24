using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

internal class JobApplicationDefaultsConfiguration : IEntityTypeConfiguration<JobApplicationDefaults>
{
    public void Configure(EntityTypeBuilder<JobApplicationDefaults> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Location)
            .IsRequired()
            .HasMaxLength(200);
    }
}
