using FluentValidation;

namespace AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;

public class GetAiSettingsByUserIdQueryValidator : AbstractValidator<GetAiSettingsByUserIdQuery>
{
    public GetAiSettingsByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
        .NotEmpty().WithMessage("{PropertyName} is required")
        .NotNull().WithMessage("{PropertyName} is required");
    }
}
