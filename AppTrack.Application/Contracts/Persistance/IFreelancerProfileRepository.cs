using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IFreelancerProfileRepository : IGenericRepository<FreelancerProfile>
{
    Task<FreelancerProfile?> GetByUserIdAsync(string userId);
    Task UpsertAsync(FreelancerProfile profile);
}
