using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services;

public class ApplicationTextService : BaseHttpService, IApplicationTextService
{
    public ApplicationTextService(IClient client) : base(client)
    {
    }

    public Task<Response<ApplicationTextModel>> GenerateAiText(string prompt, int jobApplicationId, string promptKey, CancellationToken token) =>
        TryExecuteAsync(async () =>
        {
            var command = new GenerateAiTextCommand() { Prompt = prompt, JobApplicationId = jobApplicationId, PromptKey = promptKey };
            var generatedTextDto = await _client.GenerateAsync(command, token);
            return new ApplicationTextModel()
            {
                Text = generatedTextDto.GeneratedText,
                WindowTitle = "Generated application text",
            };
        });

    public Task<Response<RenderedPromptModel>> RenderPrompt(int jobApplicationId, string promptKey) =>
        TryExecuteAsync(async () =>
        {
            var query = new RenderPromptQuery() { JobApplicationId = jobApplicationId, PromptKey = promptKey };
            var renderedPromptDto = await _client.RenderPromptAsync(query);
            return new RenderedPromptModel()
            {
                Text = renderedPromptDto.Prompt,
                WindowTitle = "Rendered prompt",
                UnusedKeys = renderedPromptDto.UnusedKeys.ToList()
            };
        });

    public Task<Response<List<string>>> GetPromptNames() =>
        TryExecuteAsync(async () =>
        {
            var dto = await _client.PromptNamesAsync();
            return dto.Names.ToList();
        });

    public Task<Response<bool>> DeleteAiTextAsync(int id) =>
        TryExecuteAsync(async () =>
        {
            await _client.AiTextAsync(id);
            return true;
        });
}
