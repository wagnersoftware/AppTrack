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
        Name = "Cover_Letter",
        PromptTemplate = "Write a cover letter for {position} at {company}."
    };

    [Fact]
    public void Validate_ShouldPass_WhenDtoIsValid()
    {
        var result = _validator.TestValidate(BuildValidDto());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNameIsEmpty()
    {
        var dto = BuildValidDto();
        dto.Name = string.Empty;
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNameExceeds100Characters()
    {
        var dto = BuildValidDto();
        dto.Name = new string('x', 101);
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenNameIsExactly100Characters()
    {
        var dto = BuildValidDto();
        dto.Name = new string('x', 100);
        _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Name);
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
    public void Validate_ShouldHaveError_WhenNameContainsSpace()
    {
        var dto = BuildValidDto();
        dto.Name = "Cover Letter";
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("A prompt name must not contain spaces.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNameStartsWithBuiltInPrefix()
    {
        var dto = BuildValidDto();
        dto.Name = "builtIn_Cover_Letter";
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("A prompt name must not start with 'builtIn_'.");
    }
}
