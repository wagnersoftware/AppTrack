using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IJobApplicationDefaultsRepository : IGenericRepository<JobApplicationDefaults>
{
    Task<JobApplicationDefaults?> CreateForUserAsync(int userId);
    Task<JobApplicationDefaults> GetByUserIdAsync(int userId);
}
