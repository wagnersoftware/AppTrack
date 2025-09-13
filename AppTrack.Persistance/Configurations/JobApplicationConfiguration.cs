using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.HasData(
            new JobApplication()
            {
                Id = 1,
                Client = "TestClient1",
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
            });

        builder.Property(x => x.Client)
            .IsRequired()
            .HasMaxLength(50);
    }
}
