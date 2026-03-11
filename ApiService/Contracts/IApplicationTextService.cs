using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IApplicationTextService
{
    Task<Response<ApplicationTextModel>> GenerateApplicationText(string prompt, int jobApplicationId, CancellationToken token);

    Task<Response<GeneratedPromptModel>> GeneratePrompt(int jobApplicationId);
}
