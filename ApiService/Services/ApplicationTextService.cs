using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services;

public class ApplicationTextService : BaseHttpService, IApplicationTextService
{
    public ApplicationTextService(IClient client) : base(client)
    {
    }

    public Task<Response<ApplicationTextModel>> GenerateApplicationText(string prompt, int jobApplicationId, string promptKey, CancellationToken token) =>
        TryExecuteAsync(async () =>
        {
            var command = new GenerateApplicationTextCommand() { Prompt = prompt, JobApplicationId = jobApplicationId, PromptKey = promptKey };
            var generatedTextDto = await _client.GenerateApplicationTextAsync(command, token);
            return new ApplicationTextModel()
            {
                Text = generatedTextDto.GeneratedText,
                WindowTitle = "Generated application text",
            };
        });

    public Task<Response<GeneratedPromptModel>> GeneratePrompt(int jobApplicationId, string promptName) =>
        TryExecuteAsync(async () =>
        {
            var query = new GeneratePromptQuery() { JobApplicationId = jobApplicationId, PromptName = promptName };
            var generatedPromptDto = await _client.GeneratePromptAsync(query);
            return new GeneratedPromptModel()
            {
                Text = generatedPromptDto.Prompt,
                WindowTitle = "Generated prompt",
                UnusedKeys = generatedPromptDto.UnusedKeys.ToList()
            };
        });

    public Task<Response<List<string>>> GetPromptNames() =>
        TryExecuteAsync(async () =>
        {
            var dto = await _client.PromptNamesAsync();
            return dto.Names.ToList();
        });
}
