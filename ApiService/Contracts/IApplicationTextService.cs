using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IApplicationTextService
{
    Task<Response<ApplicationTextModel>> GenerateAiText(string prompt, int jobApplicationId, string promptKey, CancellationToken token);

    Task<Response<RenderedPromptModel>> RenderPrompt(int jobApplicationId, string promptKey);

    Task<Response<List<string>>> GetPromptKeys();

    Task<Response<bool>> DeleteAiTextAsync(int id);
}
