using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IApplicationTextService
{
    Task<Response<ApplicationTextModel>> GenerateApplicationText(string prompt, string userId, int jobApplicationId, CancellationToken token);

    Task<Response<GeneratedPromptModel>> GeneratePrompt(int jobApplicationId, string userId);
}
