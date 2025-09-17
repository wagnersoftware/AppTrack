using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Identity.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "d5a11976-0c53-47d2-b1a6-927e2fcbff5f",
                Name = "Administrator",
                NormalizedName = "Administrator"
            },
            new IdentityRole
            {
                Id = "7b2f34f2-6c59-4e82-8a8f-3f6e7f05a8ad",
                Name = "User",
                NormalizedName = "User"
            });
    }
}
