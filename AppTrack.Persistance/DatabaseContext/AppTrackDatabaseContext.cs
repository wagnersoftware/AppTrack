using AppTrack.Domain;
using AppTrack.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace AppTrack.Persistance.DatabaseContext;

public class AppTrackDatabaseContext : DbContext
{
    public AppTrackDatabaseContext(DbContextOptions<AppTrackDatabaseContext> options) : base(options)
    {
       
    }

    public DbSet<JobApplication> JobApplications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppTrackDatabaseContext).Assembly);
        base.OnModelCreating(builder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach(var entry in base.ChangeTracker.Entries<BaseEntity>().Where((entry => entry.State == EntityState.Added || entry.State == EntityState.Modified )))
        {
            entry.Entity.DateModified = DateTime.Now;

            if(entry.State == EntityState.Added)
            {
                entry.Entity.DateCreated = DateTime.Now;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
