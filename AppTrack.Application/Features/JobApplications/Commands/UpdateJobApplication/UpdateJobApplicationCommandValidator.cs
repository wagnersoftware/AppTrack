using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.JobApplications.Commands.DeleteJobApplication;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;

public class UpdateJobApplicationCommandValidator : AbstractValidator<UpdateJobApplicationCommand>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public UpdateJobApplicationCommandValidator(IJobApplicationRepository jobApplicationRepository)
    {
        this._jobApplicationRepository = jobApplicationRepository;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.URL)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.Status)
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x)
            .MustAsync(JobApplicationExists)
            .WithMessage("Job application doesn't exist");
    }

    private async Task<bool> JobApplicationExists(UpdateJobApplicationCommand command, CancellationToken token)
    {
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(command.Id);
        return jobApplication != null;
    }
}
