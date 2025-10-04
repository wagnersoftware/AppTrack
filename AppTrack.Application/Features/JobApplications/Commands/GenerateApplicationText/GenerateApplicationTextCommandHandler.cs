using AppTrack.Application.Contracts.ApplicationTextGenerator;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;

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
        var aiSettings = await _aiSettingsRepository.GetByUserIdAsync(request.UserId);
        _applicationTextGenerator.SetApiKey(aiSettings.ApiKey);

        //build and generate prompt
        var prompt = BuildPrompt(aiSettings.Prompt, request.Position, aiSettings.MySkills, request.URL);
        var generatedApplicationText = await _applicationTextGenerator.GenerateApplicationTextAsync(prompt, cancellationToken);

        //update the job application with generated text
        var jobApplicationToUpdate = await _jobApplicationRepository.GetByIdAsync(request.ApplicationId);
        jobApplicationToUpdate.ApplicationText = generatedApplicationText;
        await _jobApplicationRepository.UpdateAsync(jobApplicationToUpdate);

        return new GeneratedApplicationTextDto() { ApplicationText = generatedApplicationText };
    }

    private static string BuildPrompt(string prompt, string position, string mySkills, string url)
    {
        var replacements = new Dictionary<string, string>
        {
            ["{position}"] = position,
            ["{skills}"] = mySkills,
            ["{url}"] = url
        };

        foreach (var kvp in replacements)
        {
            prompt = prompt.Replace(kvp.Key, kvp.Value);
        }

        if (prompt.Contains("{position}"))
        {
            throw new InvalidOperationException("Placeholder {position} was not replaced in the prompt.");
        }
        else if (prompt.Contains("{skills}"))
        {
            throw new InvalidOperationException("Placeholder {skills} was not replaced in the propmt.");
        }
        else if (prompt.Contains("{url}"))
        {
            throw new InvalidOperationException("Placeholder {url} was not replaced in the propmt.");
        }

        return prompt;
    }
}
