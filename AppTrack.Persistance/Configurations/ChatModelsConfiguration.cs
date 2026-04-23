using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class ChatModelsConfiguration : IEntityTypeConfiguration<ChatModel>
{

    public void Configure(EntityTypeBuilder<ChatModel> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ApiModelName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(250);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        // add models
        builder.HasData(
            new ChatModel
            {
                Id = 1,
                Name = "ChatGPT 3.5",
                ApiModelName = "gpt-3.5-turbo",
                Description = "Legacy model. Not recommended for CV analysis or job matching due to limited reasoning capabilities.",
                IsActive = false
            },
            new ChatModel
            {
                Id = 2,
                Name = "ChatGPT 4",
                ApiModelName = "gpt-4",
                Description = "High-quality model for complex analysis, CV parsing, job matching and advanced reasoning. Recommended for accurate evaluations and structured outputs.",
                IsActive = false
            },
            new ChatModel
            {
                Id = 3,
                Name = "GPT-4o",
                ApiModelName = "gpt-4o",
                Description = "Best balance of quality and speed, ideal for CV analysis, job matching and complex reasoning",
                IsActive = true
            },
            new ChatModel
            {
                Id = 4,
                Name = "ChatGPT 4 Mini",
                ApiModelName = "gpt-4o-mini",
                Description = "Fast and cost-efficient model for simple tasks like rephrasing, summaries, and draft generation. Not suitable for deep analysis.",
                IsActive = true
            },

            new ChatModel
            {
                Id = 5,
                Name = "GPT-5",
                ApiModelName = "gpt-5",
                Description = "Top-tier reasoning model for critical evaluations and high-stakes decisions",
                IsActive = true
            });
        }
}
