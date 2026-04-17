using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class JobApplicationAiTextConfiguration : IEntityTypeConfiguration<JobApplicationAiText>
{
    public void Configure(EntityTypeBuilder<JobApplicationAiText> builder)
    {
        builder.ToTable("JobApplicationAiTexts");

        builder.Property(x => x.PromptKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.GeneratedText)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.GeneratedAt)
            .IsRequired();

        builder.HasOne(x => x.JobApplication)
            .WithMany(x => x.AiTextHistory)
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.JobApplicationId, x.PromptKey });
    }
}
