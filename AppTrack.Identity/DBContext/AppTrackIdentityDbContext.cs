using AppTrack.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AppTrack.Identity.DBContext;

public class AppTrackIdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public AppTrackIdentityDbContext(DbContextOptions<AppTrackIdentityDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        //builder.ApplyConfigurationsFromAssembly(typeof(AppTrackIdentityDbContext).Assembly);

        // roles
        var adminRoleId = "a3f1b66c-7a52-4e7a-9a31-23c6a1c7f111";
        var userRoleId = "b7d9e99a-5117-49c2-9f87-55d33c2ef222";

        builder.Entity<IdentityRole>().HasData(
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

        // user
        var adminId = "f8e1c1b9-3f1b-4c24-9b71-17d6a916e42e";
        var userId = "2b7cfb44-36a8-49c9-8a8a-6e9c85a2cf1b";
        var hasher = new PasswordHasher<ApplicationUser>();

        builder.Entity<ApplicationUser>().HasData(
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

        // userRoles
        builder.Entity<IdentityUserRole<string>>().HasData(
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
