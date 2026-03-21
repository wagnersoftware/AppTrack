using AppTrack.Shared.Validation.Interfaces;
using AppTrack.Shared.Validation.Validators;
using Shouldly;

namespace AppTrack.Application.UnitTests.Validators;

file class TestAiSettingsValidator : AiSettingsBaseValidator<TestAiSettings> { }

file class TestAiSettings : IAiSettingsValidatable
{
    public IEnumerable<IPromptParameterValidatable> PromptParameter { get; init; } = [];
    public IEnumerable<IPromptValidatable> Prompts { get; init; } = [];
}

file class TestPromptItem : IPromptValidatable
{
    public string Name { get; init; } = string.Empty;
    public string PromptTemplate { get; init; } = string.Empty;
}

file class TestPromptParameterItem : IPromptParameterValidatable
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public class AiSettingsBaseValidatorTests
{
    private readonly TestAiSettingsValidator _validator = new();

    [Fact]
    public async Task EmptyPrompts_ShouldPass()
    {
        var result = await _validator.ValidateAsync(new TestAiSettings());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidPrompts_ShouldPass()
    {
        var settings = new TestAiSettings
        {
            Prompts =
            [
                new TestPromptItem { Name = "Prompt A", PromptTemplate = "Hello" },
                new TestPromptItem { Name = "Prompt B", PromptTemplate = "World" },
            ]
        };
        var result = await _validator.ValidateAsync(settings);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task DuplicatePromptNames_ShouldFail()
    {
        var settings = new TestAiSettings
        {
            Prompts =
            [
                new TestPromptItem { Name = "Same", PromptTemplate = "Hello" },
                new TestPromptItem { Name = "same", PromptTemplate = "World" }, // case-insensitive duplicate
            ]
        };
        var result = await _validator.ValidateAsync(settings);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Prompts");
    }

    [Fact]
    public async Task PromptWithEmptyName_ShouldFail()
    {
        var settings = new TestAiSettings
        {
            Prompts = [new TestPromptItem { Name = "", PromptTemplate = "Hello" }]
        };
        var result = await _validator.ValidateAsync(settings);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task PromptWithEmptyTemplate_ShouldFail()
    {
        // Verifies RuleForEach -> PromptItemValidator -> PromptTemplate rule is wired correctly
        var settings = new TestAiSettings
        {
            Prompts = [new TestPromptItem { Name = "Valid", PromptTemplate = "" }]
        };
        var result = await _validator.ValidateAsync(settings);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task DuplicateParameterKeys_ShouldFail()
    {
        // Regression: collection-level PromptParameter uniqueness rule still fires after additive change
        var settings = new TestAiSettings
        {
            PromptParameter =
            [
                new TestPromptParameterItem { Key = "key", Value = "a" },
                new TestPromptParameterItem { Key = "KEY", Value = "b" }, // case-insensitive duplicate
            ]
        };
        var result = await _validator.ValidateAsync(settings);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PromptParameter");
    }

    [Fact]
    public async Task ParameterWithEmptyKey_ShouldFail()
    {
        // Regression: item-level PromptParameter validation (RuleForEach -> PromptParameterItemValidator) still fires
        var settings = new TestAiSettings
        {
            PromptParameter = [new TestPromptParameterItem { Key = "", Value = "v" }]
        };
        var result = await _validator.ValidateAsync(settings);
        result.IsValid.ShouldBeFalse();
    }
}
