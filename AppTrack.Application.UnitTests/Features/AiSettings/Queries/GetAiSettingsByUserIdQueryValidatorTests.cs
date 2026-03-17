using AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Queries;

public class GetAiSettingsByUserIdQueryValidatorTests
{
    private readonly GetAiSettingsByUserIdQueryValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenQueryHasUserId()
    {
        var query = new GetAiSettingsByUserIdQuery { UserId = "user-1" };
        var result = _validator.TestValidate(query);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenUserIdIsEmpty()
    {
        // The validator has no rules; all inputs are valid.
        var query = new GetAiSettingsByUserIdQuery { UserId = string.Empty };
        var result = _validator.TestValidate(query);
        result.IsValid.ShouldBeTrue();
    }
}
