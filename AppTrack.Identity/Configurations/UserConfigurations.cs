using AppTrack.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Identity.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        var adminId = "f8e1c1b9-3f1b-4c24-9b71-17d6a916e42e";
        var userId = "2b7cfb44-36a8-49c9-8a8a-6e9c85a2cf1b";
        var hasher = new PasswordHasher<ApplicationUser>();

        builder.HasData(
            new ApplicationUser
            {
                Id = adminId,
                UserName = "admin@localhost.com",
                NormalizedUserName = "ADMIN@LOCALHOST.COM",
                Email = "admin@localhost.com",
                NormalizedEmail = "ADMIN@LOCALHOST.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEONXKAOgk5TYispSOWr01crXs7n+NCe/sj+xSXJDAqPP+h0E+ZoUE440rEsfvH9wwA==", //Password1!
                SecurityStamp = "STATIC-SECURITYSTAMP-ADMIN",
                ConcurrencyStamp = "STATIC-CONCURRENCYSTAMP-ADMIN",
                FirstName = "System",
                LastName = "Admin",
                AccessFailedCount = 0,
                LockoutEnabled = false,
                LockoutEnd = null,
                PhoneNumber = null,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false
            },
            new ApplicationUser
            {
                Id = userId,
                UserName = "user@localhost.com",
                NormalizedUserName = "USER@LOCALHOST.COM",
                Email = "user@localhost.com",
                NormalizedEmail = "USER@LOCALHOST.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEIQ9SN1FNoOq38KmHYBolcXPW3nqc5eXMGmzGJ7KnNCFLJH/7Y1B6vHdCvbk/J+TwA==", //Password1!
                SecurityStamp = "STATIC-SECURITYSTAMP-USER",
                ConcurrencyStamp = "STATIC-CONCURRENCYSTAMP-USER",
                FirstName = "System",
                LastName = "User",
                AccessFailedCount = 0,
                LockoutEnabled = false,
                LockoutEnd = null,
                PhoneNumber = null,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false
            }
        );
    }
}
