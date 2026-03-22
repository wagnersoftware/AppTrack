using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IDefaultPromptRepository : IGenericRepository<DefaultPrompt>
{
    /// <summary>
    /// Reserved for future use: filters default prompts by language once user language preference is implemented.
    /// </summary>
    Task<IReadOnlyList<DefaultPrompt>> GetByLanguageAsync(string language);
}
