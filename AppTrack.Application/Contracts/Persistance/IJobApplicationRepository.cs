using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IJobApplicationRepository : IGenericRepository<JobApplication>
{
    Task<List<JobApplication>> GetAllForUserAsync(string userId);
    Task<List<JobApplication>> GetAllForUserWithAiTextHistoryAsync(string userId);
    Task<JobApplication?> GetByIdWithAiTextHistoryAsync(int id);
}

