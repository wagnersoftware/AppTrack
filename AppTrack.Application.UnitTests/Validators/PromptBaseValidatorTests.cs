using AppTrack.Shared.Validation.Interfaces;
using AppTrack.Shared.Validation.Validators;
using FluentValidation;
using Shouldly;

namespace AppTrack.Application.UnitTests.Validators;

// Concrete test implementation
internal sealed class TestPromptValidator : PromptBaseValidator<TestPrompt> { }

internal sealed class TestPrompt : IPromptValidatable
{
    public string Name { get; init; } = string.Empty;
    public string PromptTemplate { get; init; } = string.Empty;
}

public class PromptBaseValidatorTests
{
    private readonly TestPromptValidator _validator = new();

    [Fact]
    public async Task ValidPrompt_ShouldPass()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Name = "My_Prompt", PromptTemplate = "Hello {name}" });
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task EmptyName_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Name = "", PromptTemplate = "Hello" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task NameTooLong_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Name = new string('a', 101), PromptTemplate = "Hello" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task EmptyPromptTemplate_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Name = "My_Prompt", PromptTemplate = "" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PromptTemplate");
    }

    [Fact]
    public async Task NameWithSpace_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Name = "My Prompt", PromptTemplate = "Hello" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name" && e.ErrorMessage == "A prompt name must not contain spaces.");
    }

    [Fact]
    public async Task NameStartingWithDefaultPrefix_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Name = "Default_Something", PromptTemplate = "Hello" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name" && e.ErrorMessage == "A prompt name must not start with 'Default_'.");
    }
}
