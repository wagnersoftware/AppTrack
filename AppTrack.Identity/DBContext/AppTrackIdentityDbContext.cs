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
        builder.ApplyConfigurationsFromAssembly(typeof(AppTrackIdentityDbContext).Assembly);
    }
}
