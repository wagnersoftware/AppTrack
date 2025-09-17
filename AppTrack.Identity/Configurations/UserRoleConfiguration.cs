using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Identity.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        builder.HasData(
            new IdentityUserRole<string> //Admin
            {
                RoleId = "d5a11976 - 0c53 - 47d2 - b1a6 - 927e2fcbff5f",
                UserId = "f8e1c1b9-3f1b-4c24-9b71-17d6a916e42e"
            },
            new IdentityUserRole<string> //User
            {
                RoleId = "7b2f34f2-6c59-4e82-8a8f-3f6e7f05a8ad",
                UserId = "2b7cfb44-36a8-49c9-8a8a-6e9c85a2cf1b"
            }
            );
    }
}
