using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class ProcessedProjectItemConfiguration : IEntityTypeConfiguration<ProcessedProjectItem>
{
    public void Configure(EntityTypeBuilder<ProcessedProjectItem> builder)
    {
        builder.ToTable("ProcessedProjectItems");
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ProjectItemUrl).IsRequired().HasMaxLength(2000);
        builder.HasIndex(x => new { x.UserId, x.ProjectItemUrl }).IsUnique();
    }
}
