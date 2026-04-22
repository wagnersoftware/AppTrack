using AppTrack.Domain;
using AppTrack.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.DatabaseContext;

public class AppTrackDatabaseContext : DbContext
{
    public AppTrackDatabaseContext(DbContextOptions<AppTrackDatabaseContext> options) : base(options)
    {
    }

    public DbSet<JobApplication> JobApplications { get; set; }

    public DbSet<JobApplicationAiText> JobApplicationAiTexts { get; set; }

    public DbSet<JobApplicationDefaults> JobApplicationDefaults { get; set; }

    public DbSet<AiSettings> AiSettings { get; set; }

    public DbSet<ChatModel> ChatModels { get; set; }

    public DbSet<BuiltInPrompt> BuiltInPrompts { get; set; }

    public DbSet<FreelancerProfile> FreelancerProfiles { get; set; }

    public DbSet<BuiltInPromptParameter> BuiltInPromptParameter { get; set; }

    public DbSet<RssPortal> RssPortals { get; set; }
    public DbSet<UserRssSubscription> UserRssSubscriptions { get; set; }
    public DbSet<RssMonitoringSettings> RssMonitoringSettings { get; set; }
    public DbSet<ProcessedFeedItem> ProcessedFeedItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppTrackDatabaseContext).Assembly);

        base.OnModelCreating(modelBuilder);

    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        base.ChangeTracker
            .Entries<BaseEntity>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .ToList()
            .ForEach(entry =>
            {
                entry.Entity.ModifiedDate = DateTime.Now;

                if (entry.Entity.CreationDate == null)
                {
                    entry.Entity.CreationDate = DateTime.Now;
                }
            });

        return base.SaveChangesAsync(cancellationToken);
    }
}
