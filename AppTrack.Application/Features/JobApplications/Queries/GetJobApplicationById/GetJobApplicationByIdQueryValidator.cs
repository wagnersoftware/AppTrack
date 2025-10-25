using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Queries.GetJobApplicationById;

public class GetJobApplicationByIdQueryValidator : AbstractValidator<GetJobApplicationByIdQuery>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public GetJobApplicationByIdQueryValidator(IJobApplicationRepository jobApplicationRepository)
    {
        this._jobApplicationRepository = jobApplicationRepository;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x)
            .CustomAsync(async (command, context, cancellationToken) =>
            {
                var jobApplication = await _jobApplicationRepository.GetByIdAsync(command.Id);
                if (jobApplication == null)
                {
                    context.AddFailure("Id", "Job application not found.");
                    return;
                }

                if (jobApplication.UserId != command.UserId)
                {
                    context.AddFailure("UserId", "Job application doesn't belong to this user.");
                }
            });
    }
}
