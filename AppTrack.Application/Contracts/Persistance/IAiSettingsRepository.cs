using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IAiSettingsRepository : IGenericRepository<AiSettings>
{
    Task<AiSettings?> GetByUserIdIncludePromptParameterAsync(string userId);

    Task<AiSettings?> GetByIdWithPromptParameterAsync(int id);
}
