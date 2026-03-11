using AppTrack.Application.Contracts.Persistance;
using AppTrack.Shared.Validation.Validators;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;

public class UpdateJobApplicationCommandValidator : JobApplicationBaseValidator<UpdateJobApplicationCommand>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public UpdateJobApplicationCommandValidator(IJobApplicationRepository jobApplicationRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;

        RuleFor(x => x.Status)
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.StartDate)
            .NotNull().WithMessage("{PropertyName} is required")
            .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(x => x)
            .MustAsync(JobApplicationExistsForUser)
            .WithMessage("Job application doesn't exist for user");
    }

    private async Task<bool> JobApplicationExistsForUser(UpdateJobApplicationCommand command, CancellationToken token)
    {
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(command.Id);
        return jobApplication != null && jobApplication.UserId == command.UserId;
    }
}
