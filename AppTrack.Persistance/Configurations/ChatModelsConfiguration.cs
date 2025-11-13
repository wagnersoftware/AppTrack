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
                Description = "Fast model, suitable for short text snippets and suggestions",
                IsActive = true
            },
            new ChatModel
            {
                Id = 2,
                Name = "ChatGPT 4",
                ApiModelName = "gpt-4",
                Description = "High-precision model, ideal for complex cover letters and refined writing",
                IsActive = true
            },
            new ChatModel
            {
                Id = 3,
                Name = "ChatGPT 4 (32k)",
                ApiModelName = "gpt-4-32k",
                Description = "Handles long documents, perfect for extensive resumes or detailed cover letters",
                IsActive = false
            },
            new ChatModel
            {
                Id = 4,
                Name = "ChatGPT 4 Mini",
                ApiModelName = "gpt-4o-mini",
                Description = "Lightweight model for quick suggestions or interactive text generation",
                IsActive = false
            });
        }
}
