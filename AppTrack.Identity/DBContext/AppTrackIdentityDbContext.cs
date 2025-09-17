using AppTrack.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Identity.DBContext;

public class AppTrackIdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public AppTrackIdentityDbContext(DbContextOptions<AppTrackIdentityDbContext> options) : base(options)
    {
        
    }
}
