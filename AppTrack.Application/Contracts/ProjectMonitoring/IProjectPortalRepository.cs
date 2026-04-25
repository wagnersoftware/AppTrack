using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IProjectPortalRepository : IGenericRepository<ProjectPortal>
{
    Task<List<ProjectPortal>> GetAllActiveAsync();
}
