using AppTrack.Application.Contracts.ApplicationTextGenerator;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain.Contracts;

namespace AppTrack.Application.Features.JobApplications.Commands.GenerateApplicationText;

public class GenerateApplicationTextCommandHandler : IRequestHandler<GenerateApplicationTextCommand, GeneratedApplicationTextDto>
{
    private readonly IApplicationTextGenerator _applicationTextGenerator;
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public GenerateApplicationTextCommandHandler(IApplicationTextGenerator applicationTextGenerator,
                                                 IAiSettingsRepository aiSettingsRepository,
                                                 IJobApplicationRepository jobApplicationRepository,
                                                 IPromptBuilder promptBuilder)
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

        //generate application text
        var generatedApplicationText = await _applicationTextGenerator.GenerateApplicationTextAsync(request.Prompt, cancellationToken);

        //update the job application with generated text
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId);
        jobApplication!.ApplicationText = generatedApplicationText;
        await _jobApplicationRepository.UpdateAsync(jobApplication);

        return new GeneratedApplicationTextDto() { ApplicationText = generatedApplicationText};
    }
}
