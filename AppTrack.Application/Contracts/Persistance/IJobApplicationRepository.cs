using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IJobApplicationRepository : IGenericRepository<JobApplication>
{
    Task<List<JobApplication>> GetAllForUserAsync(string userId);
}

