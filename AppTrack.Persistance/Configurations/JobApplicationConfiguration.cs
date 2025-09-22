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
                Name = "TestClient1",
                Position = "Developer1",
                ApplicationText = "ApplicationText1",
                Status = Domain.Enums.JobApplicationStatus.Rejected,
                URL = "www.testURL.de"
            });

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}
