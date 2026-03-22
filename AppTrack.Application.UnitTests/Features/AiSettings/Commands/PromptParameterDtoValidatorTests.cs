using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Commands;

public class PromptParameterDtoValidatorTests
{
    private readonly PromptParameterDtoValidator _validator = new();

    private static PromptParameterDto BuildValidDto() => new()
    {
        Key = "Temperature",
        Value = "0.7"
    };

    [Fact]
    public void Validate_ShouldPass_WhenDtoIsValid()
    {
        var result = _validator.TestValidate(BuildValidDto());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenKeyIsEmpty()
    {
        var dto = BuildValidDto();
        dto.Key = string.Empty;
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Key)
            .WithErrorMessage("Key is required.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenKeyExceeds50Characters()
    {
        var dto = BuildValidDto();
        dto.Key = new string('k', 51);
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Key)
            .WithErrorMessage("Key must not exceed 50 characters.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenKeyIsExactly50Characters()
    {
        var dto = BuildValidDto();
        dto.Key = new string('k', 50);
        _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Key);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenValueIsEmpty()
    {
        var dto = BuildValidDto();
        dto.Value = string.Empty;
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Value)
            .WithErrorMessage("Value is required.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenValueExceeds500Characters()
    {
        var dto = BuildValidDto();
        dto.Value = new string('v', 501);
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Value)
            .WithErrorMessage("Value must not exceed 500 characters.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenValueIsExactly500Characters()
    {
        var dto = BuildValidDto();
        dto.Value = new string('v', 500);
        _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Value);
    }
}
