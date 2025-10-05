using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplicationsForUser;

public class GetJobApplicationsForUserQueryValidator : AbstractValidator<GetJobApplicationsForUserQuery>
{
    public GetJobApplicationsForUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");
    }
}
