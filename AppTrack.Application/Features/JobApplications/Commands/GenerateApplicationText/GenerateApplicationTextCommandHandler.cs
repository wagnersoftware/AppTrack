using AppTrack.Application.Contracts.ApplicationTextGenerator;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain.Contracts;
using AppTrack.Domain.Extensions;

namespace AppTrack.Application.Features.JobApplications.Commands.GenerateApplicationText;

public class GenerateApplicationTextCommandHandler : IRequestHandler<GenerateApplicationTextCommand, GeneratedApplicationTextDto>
{
    private readonly IApplicationTextGenerator _applicationTextGenerator;
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IPromptBuilder _promptBuilder;

    public GenerateApplicationTextCommandHandler(IApplicationTextGenerator applicationTextGenerator,
                                                 IAiSettingsRepository aiSettingsRepository,
                                                 IJobApplicationRepository jobApplicationRepository,
                                                 IPromptBuilder promptBuilder)
    {
        this._applicationTextGenerator = applicationTextGenerator;
        this._aiSettingsRepository = aiSettingsRepository;
        this._jobApplicationRepository = jobApplicationRepository;
        this._promptBuilder = promptBuilder;
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

        //get job application
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId);

        //build prompt
        var applicantParameter = aiSettings.PromptParameter.ToList();
        var jobApplicationParameter = jobApplication!.ToPromptParameters().ToList();
        var promptParameter = jobApplicationParameter.Union(applicantParameter).ToList();
        var (prompt, unusedKeys) = _promptBuilder.BuildPrompt(promptParameter, aiSettings.PromptTemplate);

        //generate application text
        var generatedApplicationText = await _applicationTextGenerator.GenerateApplicationTextAsync(prompt, cancellationToken);

        //update the job application with generated text
        jobApplication!.ApplicationText = generatedApplicationText;
        await _jobApplicationRepository.UpdateAsync(jobApplication);

        return new GeneratedApplicationTextDto() { ApplicationText = generatedApplicationText, UnusedKeys= unusedKeys};
    }
}
