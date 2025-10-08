using AppTrack.Application.Contracts.ApplicationTextGenerator;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain;

namespace AppTrack.Application.Features.JobApplications.Commands.GenerateApplicationText;

public class GenerateApplicationTextCommandHandler : IRequestHandler<GenerateApplicationTextCommand, GeneratedApplicationTextDto>
{
    private readonly IApplicationTextGenerator _applicationTextGenerator;
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public GenerateApplicationTextCommandHandler(IApplicationTextGenerator applicationTextGenerator, IAiSettingsRepository aiSettingsRepository, IJobApplicationRepository jobApplicationRepository)
    {
        this._applicationTextGenerator = applicationTextGenerator;
        this._aiSettingsRepository = aiSettingsRepository;
        this._jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<GeneratedApplicationTextDto> Handle(GenerateApplicationTextCommand request, CancellationToken cancellationToken)
    {
        var validator = new GenerateApplicationTextCommandValidator(_jobApplicationRepository, _aiSettingsRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid Ai setting.", validationResult);
        }

        //get Ai settings
        var aiSettings = await _aiSettingsRepository.GetByUserIdWithPromptParameterAsync(request.UserId);
        _applicationTextGenerator.SetApiKey(aiSettings!.ApiKey);

        //build prompt
        var (prompt, unusedKeys) = BuildPrompt(aiSettings.Prompt, request.Position, aiSettings.PromptParameter.ToList(), request.URL);

        //generate application text
        var generatedApplicationText = await _applicationTextGenerator.GenerateApplicationTextAsync(prompt, cancellationToken);

        //update the job application with generated text
        var jobApplicationToUpdate = await _jobApplicationRepository.GetByIdAsync(request.ApplicationId);
        jobApplicationToUpdate!.ApplicationText = generatedApplicationText;
        await _jobApplicationRepository.UpdateAsync(jobApplicationToUpdate);

        return new GeneratedApplicationTextDto() { ApplicationText = generatedApplicationText, UnusedKeys= unusedKeys};
    }

    private static (string prompt, List<string> unusedKeys) BuildPrompt(string prompt, string position, List<PromptParameter> promptParameter, string url)
    {
        var replacements = new Dictionary<string, string>();
        var unusedKeys = new List<string>();

        foreach (var parameter in promptParameter)
        {
            var key = $"{{{parameter.Key.Trim()}}}";
            replacements.Add(key, parameter.Value);
        }

        foreach (var kvp in replacements)
        {
            if (prompt.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)) // ignore upper/lower case
            {
                prompt = prompt.Replace(kvp.Key, kvp.Value, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                unusedKeys.Add(kvp.Key);
            }
        }

        return (prompt, unusedKeys);
    }
}
