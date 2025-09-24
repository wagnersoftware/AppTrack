using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IJobApplicationDefaultsRepository : IGenericRepository<JobApplicationDefaults>
{
    Task<JobApplicationDefaults?> CreateForUserAsync(string userId);
    Task<JobApplicationDefaults> GetByUserIdAsync(string userId);
}
