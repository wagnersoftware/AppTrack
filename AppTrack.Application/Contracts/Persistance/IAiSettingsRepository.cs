using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IAiSettingsRepository : IGenericRepository<AiSettings>
{
    Task<AiSettings?> GetByUserIdWithPromptsReadOnlyAsync(string userId);

    Task<AiSettings?> GetByIdWithPromptsAsync(int id);

    Task<AiSettings?> GetByUserIdWithPromptParameterAsync(string userId);
}
