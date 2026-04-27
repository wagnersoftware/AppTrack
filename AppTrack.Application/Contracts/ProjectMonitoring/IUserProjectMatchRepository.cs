using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IUserProjectMatchRepository : IGenericRepository<UserProjectMatch>
{
    Task<UserProjectMatch?> GetByUserAndProjectAsync(string userId, int scrapedProjectId, CancellationToken ct);
    Task<List<UserProjectMatch>> GetUnnotifiedMatchesAsync(string userId, CancellationToken ct);
    Task MarkAsNotifiedAsync(string userId, int scrapedProjectId, CancellationToken ct);
}
