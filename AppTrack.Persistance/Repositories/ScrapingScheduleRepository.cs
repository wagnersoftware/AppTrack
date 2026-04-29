using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;

namespace AppTrack.Persistance.Repositories;

public class ScrapingScheduleRepository(AppTrackDatabaseContext context) : IScrapingScheduleRepository
{
    private const int SingletonId = 1;

    public async Task<DateTime?> GetNextRunAfterAsync(CancellationToken ct)
    {
        var state = await context.ScrapingScheduleStates.FindAsync([SingletonId], ct);
        return state?.NextRunAfterUtc;
    }

    public async Task SetNextRunAfterAsync(DateTime nextRunAfterUtc, CancellationToken ct)
    {
        var state = await context.ScrapingScheduleStates.FindAsync([SingletonId], ct);
        if (state is null)
        {
            context.ScrapingScheduleStates.Add(new ScrapingScheduleState
            {
                Id = SingletonId,
                NextRunAfterUtc = nextRunAfterUtc
            });
        }
        else
        {
            state.NextRunAfterUtc = nextRunAfterUtc;
        }

        await context.SaveChangesAsync(ct);
    }
}
