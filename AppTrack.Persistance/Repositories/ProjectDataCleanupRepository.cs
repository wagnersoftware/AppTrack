using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class ProjectDataCleanupRepository(AppTrackDatabaseContext context) : IProjectDataCleanupRepository
{
    public async Task CleanupOlderThanAsync(DateTime cutoff, CancellationToken ct)
    {
        // Delete dependent rows first to satisfy the FK constraint UserProjectMatch → ScrapedProject.
        await context.UserProjectMatches
            .Where(m => m.ScrapedProject.CreationDate != null && m.ScrapedProject.CreationDate < cutoff)
            .ExecuteDeleteAsync(ct);

        await context.ProcessedProjectItems
            .Where(p => p.ProcessedAt < cutoff)
            .ExecuteDeleteAsync(ct);

        await context.ScrapedProjects
            .Where(p => p.CreationDate != null && p.CreationDate < cutoff)
            .ExecuteDeleteAsync(ct);
    }
}
