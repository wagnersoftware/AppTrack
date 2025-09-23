using FluentValidation;

namespace AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId;

public class GetJobApplicationDefaultsByUserIdQueryValidator : AbstractValidator<GetJobApplicationDefaultsByUserIdQuery>
{
    public GetJobApplicationDefaultsByUserIdQueryValidator()
    {
        RuleFor(x => x.Id)
        .NotEmpty().WithMessage("{PropertyName} is required")
        .NotNull().WithMessage("{PropertyName} is required");
    }
}
