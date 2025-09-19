using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Identity.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            var adminRoleId = "a3f1b66c-7a52-4e7a-9a31-23c6a1c7f111";
            var userRoleId = "b7d9e99a-5117-49c2-9f87-55d33c2ef222";

            builder.HasData(
                new IdentityRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "STATIC-CONCURRENCYSTAMP-ROLE-ADMIN"
                },
                new IdentityRole
                {
                    Id = userRoleId,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "STATIC-CONCURRENCYSTAMP-ROLE-USER"
                }
            );
        }
    }
}
