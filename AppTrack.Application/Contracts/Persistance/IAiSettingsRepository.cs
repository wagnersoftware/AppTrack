using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IAiSettingsRepository : IGenericRepository<AiSettings>
{
    Task<AiSettings> GetByUserIdAsync(string userId);
}
