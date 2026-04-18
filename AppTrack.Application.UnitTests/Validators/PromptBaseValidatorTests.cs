using AppTrack.Shared.Validation.Interfaces;
using AppTrack.Shared.Validation.Validators;
using FluentValidation;
using Shouldly;

namespace AppTrack.Application.UnitTests.Validators;

// Concrete test implementation
internal sealed class TestPromptValidator : PromptBaseValidator<TestPrompt> { }

internal sealed class TestPrompt : IPromptValidatable
{
    public string Key { get; init; } = string.Empty;
    public string PromptTemplate { get; init; } = string.Empty;
}

public class PromptBaseValidatorTests
{
    private readonly TestPromptValidator _validator = new();

    [Fact]
    public async Task ValidPrompt_ShouldPass()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Key ="My_Prompt", PromptTemplate = "Hello {name}" });
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task EmptyKey_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Key ="", PromptTemplate = "Hello" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Key");
    }

    [Fact]
    public async Task KeyTooLong_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Key =new string('a', 101), PromptTemplate = "Hello" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Key");
    }

    [Fact]
    public async Task EmptyPromptTemplate_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Key ="My_Prompt", PromptTemplate = "" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PromptTemplate");
    }

    [Fact]
    public async Task KeyWithSpace_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Key ="My Prompt", PromptTemplate = "Hello" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Key" && e.ErrorMessage == "A prompt key must not contain spaces.");
    }

    [Fact]
    public async Task KeyStartingWithBuiltInPrefix_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Key ="builtIn_Something", PromptTemplate = "Hello" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Key" && e.ErrorMessage == "A prompt key must not start with 'builtIn_'.");
    }
}
