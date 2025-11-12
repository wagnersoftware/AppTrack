using AppTrack.Domain;
using AppTrack.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.DatabaseContext;

public class AppTrackDatabaseContext : DbContext
{
    public AppTrackDatabaseContext(DbContextOptions<AppTrackDatabaseContext> options) : base(options)
    {

    }

    public DbSet<JobApplication> JobApplications { get; set; }

    public DbSet<JobApplicationDefaults> JobApplicationDefaults { get; set; }

    public DbSet<AiSettings> AiSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppTrackDatabaseContext).Assembly);

        modelBuilder.Entity<AiSettings>()
            .HasMany(s => s.PromptParameter)
            .WithOne(p => p.AISettings)
            .HasForeignKey(p => p.AISettingsId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JobApplication>()
            .Property(a => a.CreationDate)
            .HasConversion(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

        modelBuilder.Entity<JobApplication>()
            .Property(a => a.ModifiedDate)
            .HasConversion(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

        modelBuilder.Entity<JobApplication>()
            .Property(a => a.StartDate)
            .HasConversion(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );


        base.OnModelCreating(modelBuilder);

    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in base.ChangeTracker.Entries<BaseEntity>().Where((entry => entry.State == EntityState.Added || entry.State == EntityState.Modified)))
        {
            entry.Entity.ModifiedDate = DateTime.Now;

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreationDate = DateTime.Now;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
