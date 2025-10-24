using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Commands.DeleteJobApplication;

public class DeleteJobApplicationCommandValidator : AbstractValidator<DeleteJobApplicationCommand>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public DeleteJobApplicationCommandValidator(IJobApplicationRepository jobApplicationRepository)
    {
        this._jobApplicationRepository = jobApplicationRepository;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x)
            .MustAsync(JobApplicationExistsForUser)
            .WithMessage("Job application doesn't exist for user");
    }

    private async Task<bool> JobApplicationExistsForUser(DeleteJobApplicationCommand command, CancellationToken token)
    {
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(command.Id);
        return jobApplication != null && jobApplication.UserId == command.UserId;
    }
}
