using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IApplicationTextService
{
    Task<Response<ApplicationTextModel>> GenerateApplicationText(string prompt, int jobApplicationId, string promptKey, CancellationToken token);

    Task<Response<GeneratedPromptModel>> GeneratePrompt(int jobApplicationId, string promptKey);

    Task<Response<List<string>>> GetPromptNames();
}
