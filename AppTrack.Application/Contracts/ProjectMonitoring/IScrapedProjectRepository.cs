using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IScrapedProjectRepository : IGenericRepository<ScrapedProject>
{
    Task<List<ScrapedProject>> GetByPortalIdsAsync(IEnumerable<int> portalIds);
    Task AddNewForPortalAsync(int portalId, IEnumerable<ScrapedProject> projects, CancellationToken ct);
}
