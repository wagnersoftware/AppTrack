using AppTrack.Application.Contracts.Persistance;
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
            .MustAsync(JobApplicationExists)
            .WithMessage("Job application doesn't exist");

    }

    private async Task<bool> JobApplicationExists(DeleteJobApplicationCommand command, CancellationToken token)
    {
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(command.Id);
        return jobApplication != null;
    }
}
