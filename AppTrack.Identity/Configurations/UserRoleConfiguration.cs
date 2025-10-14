using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Identity.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        var adminId = "f8e1c1b9-3f1b-4c24-9b71-17d6a916e42e";
        var userId = "2b7cfb44-36a8-49c9-8a8a-6e9c85a2cf1b";

        var adminRoleId = "a3f1b66c-7a52-4e7a-9a31-23c6a1c7f111";
        var userRoleId = "b7d9e99a-5117-49c2-9f87-55d33c2ef222";

        builder.HasData(
            new IdentityUserRole<string> // admin
            {
                UserId = adminId,
                RoleId = adminRoleId
            },
            new IdentityUserRole<string> // user
            {
                UserId = userId,
                RoleId = userRoleId
            }
        );
    }
}
