using AppTrack.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Identity.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        var hasher = new PasswordHasher<ApplicationUser>();
        builder.HasData(
            new ApplicationUser()
            {
                Id = "f8e1c1b9-3f1b-4c24-9b71-17d6a916e42e",
                Email = "admin@localhost.com",
                NormalizedEmail = "ADMIN@LOCALHOST.COM",
                FirstName = "System",
                LastName = "Admin",
                UserName = "admin@localhost.com",
                NormalizedUserName = "ADMIN@LOCALHOST.COM",
                //PasswordHash = hasher.HashPassword(null, "P@ssword1"),
                PasswordHash = "AQAAAAEAACcQAAAAEHN5z7FhDkT8QXG3J5J8t5g3gZl5V4kH2+XnH0+zE7R6b6Q7L8Z3yYdT0M9fP4YfA==",
                SecurityStamp = "STATIC-SECURITYSTAMP-ADMIN",
                ConcurrencyStamp = "STATIC-CONCURRENCYSTAMP-ADMIN",
                EmailConfirmed = true
            },
            new ApplicationUser()
            {
                Id = "2b7cfb44-36a8-49c9-8a8a-6e9c85a2cf1b",
                Email = "user@localhost.com",
                NormalizedEmail = "USER@LOCALHOST.COM",
                FirstName = "System",
                LastName = "User",
                UserName = "user@localhost.com",
                NormalizedUserName = "USER@LOCALHOST.COM",
                //PasswordHash = hasher.HashPassword(null, "P@ssword1"),
                PasswordHash = "AQAAAAEAACcQAAAAEHN5z7FhDkT8QXG3J5J8t5g3gZl5V4kH2+XnH0+zE7R6b6Q7L8Z3yYdT0M9fP4YfA==",
                SecurityStamp = "STATIC-SECURITYSTAMP-USER",
                ConcurrencyStamp = "STATIC-CONCURRENCYSTAMP-USER",
                EmailConfirmed = true
            });
    }
}
