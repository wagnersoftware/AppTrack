using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class FreelancerProfileRepository : GenericRepository<FreelancerProfile>, IFreelancerProfileRepository
{
    public FreelancerProfileRepository(AppTrackDatabaseContext dbContext) : base(dbContext)
    {
    }

    public async Task<FreelancerProfile?> GetByUserIdAsync(string userId)
    {
        return await _context.FreelancerProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task UpsertAsync(FreelancerProfile profile)
    {
        if (profile.Id == 0)
        {
            await CreateAsync(profile);
        }
        else
        {
            await UpdateAsync(profile);
        }
    }
}
