using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IScrapedProjectRepository : IGenericRepository<ScrapedProject>
{
    Task<List<ScrapedProject>> GetByPortalIdsAsync(IEnumerable<int> portalIds);
    Task AddNewForPortalAsync(int portalId, IEnumerable<ScrapedProject> projects, CancellationToken ct);
    // Returns ScrapedProjects for the given portal IDs that have no ProcessedProjectItem
    // entry for the given userId (LEFT JOIN on UserId + Url).
    Task<List<ScrapedProject>> GetUnprocessedForUserAsync(
        string userId,
        IEnumerable<int> portalIds,
        CancellationToken ct);
}
