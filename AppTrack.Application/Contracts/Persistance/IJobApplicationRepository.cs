using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IJobApplicationRepository : IGenericRepository<JobApplication>
{
    Task<bool> IsClientUnique(string client);
}

