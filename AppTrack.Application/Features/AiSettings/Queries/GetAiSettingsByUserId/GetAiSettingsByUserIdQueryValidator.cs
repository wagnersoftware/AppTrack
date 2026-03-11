using FluentValidation;

namespace AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;

public class GetAiSettingsByUserIdQueryValidator : AbstractValidator<GetAiSettingsByUserIdQuery>
{
    public GetAiSettingsByUserIdQueryValidator()
    {
    }
}
