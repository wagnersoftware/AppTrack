using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Commands;

public class PromptDtoValidatorTests
{
    private readonly PromptDtoValidator _validator = new();

    private static PromptDto BuildValidDto() => new()
    {
        Key = "Cover_Letter",
        PromptTemplate = "Write a cover letter for {position} at {company}."
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
        dto.Key =string.Empty;
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Key)
            .WithErrorMessage("Key is required.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenKeyExceeds100Characters()
    {
        var dto = BuildValidDto();
        dto.Key =new string('x', 101);
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Key)
            .WithErrorMessage("Key must not exceed 100 characters.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenKeyIsExactly100Characters()
    {
        var dto = BuildValidDto();
        dto.Key =new string('x', 100);
        _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Key);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPromptTemplateIsEmpty()
    {
        var dto = BuildValidDto();
        dto.PromptTemplate = string.Empty;
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.PromptTemplate)
            .WithErrorMessage("Prompt template is required.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPromptTemplateIsWhiteSpace()
    {
        var dto = BuildValidDto();
        dto.PromptTemplate = "   ";
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.PromptTemplate)
            .WithErrorMessage("Prompt template is required.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenKeyContainsSpace()
    {
        var dto = BuildValidDto();
        dto.Key ="Cover Letter";
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Key)
            .WithErrorMessage("A prompt key must not contain spaces.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenKeyStartsWithBuiltInPrefix()
    {
        var dto = BuildValidDto();
        dto.Key ="builtIn_Cover_Letter";
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Key)
            .WithErrorMessage("A prompt key must not start with 'builtIn_'.");
    }
}
