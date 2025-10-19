using FluentValidation;

namespace AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId;

public class GetJobApplicationDefaultsByUserIdQueryValidator : AbstractValidator<GetJobApplicationDefaultsByUserIdQuery>
{
    public GetJobApplicationDefaultsByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
        .NotEmpty().WithMessage("UserId is required.")
        .Matches("^[a-zA-Z0-9\\-]+$").WithMessage("UserId contains invalid characters.");
    }
}
