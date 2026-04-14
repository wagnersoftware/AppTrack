using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IAiSettingsRepository : IGenericRepository<AiSettings>
{
    Task<AiSettings?> GetByUserIdIncludePromptParameterAsync(string userId);

    Task<AiSettings?> GetByIdIncludePromptParameterAsync(int id);

    Task<AiSettings?> GetByUserIdTrackedAsync(string userId);
}
