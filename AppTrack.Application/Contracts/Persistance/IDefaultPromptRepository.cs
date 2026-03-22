using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IDefaultPromptRepository : IGenericRepository<DefaultPrompt>
{
    Task<IReadOnlyList<DefaultPrompt>> GetByLanguageAsync(string language);
}
