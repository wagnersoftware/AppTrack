using AppTrack.Shared.Validation.Interfaces;
using AppTrack.Shared.Validation.Validators;
using Shouldly;

namespace AppTrack.Application.UnitTests.Validators;

internal sealed class TestPromptParameterValidator : PromptParameterBaseValidator<TestPromptParameter> { }

internal sealed class TestPromptParameter : IPromptParameterValidatable
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public class PromptParameterBaseValidatorTests
{
    private readonly TestPromptParameterValidator _validator = new();

    [Fact]
    public async Task ValidParameter_ShouldPass()
    {
        var result = await _validator.ValidateAsync(new TestPromptParameter { Key = "myKey", Value = "some value" });
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task EmptyKey_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPromptParameter { Key = "", Value = "value" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Key" && e.ErrorMessage == "Key is required.");
    }

    [Fact]
    public async Task KeyTooLong_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPromptParameter { Key = new string('k', 51), Value = "value" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Key" && e.ErrorMessage == "Key must not exceed 50 characters.");
    }

    [Fact]
    public async Task EmptyValue_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPromptParameter { Key = "myKey", Value = "" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Value" && e.ErrorMessage == "Value is required.");
    }

    [Fact]
    public async Task ValueTooLong_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPromptParameter { Key = "myKey", Value = new string('v', 501) });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Value" && e.ErrorMessage == "Value must not exceed 500 characters.");
    }

    [Fact]
    public async Task KeyStartingWithBuiltInPrefix_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPromptParameter { Key = "builtIn_something", Value = "value" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Key" && e.ErrorMessage == "A parameter key must not start with 'builtIn_'.");
    }

    [Fact]
    public async Task KeyStartingWithBuiltInPrefixCaseInsensitive_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPromptParameter { Key = "BUILTIN_something", Value = "value" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Key" && e.ErrorMessage == "A parameter key must not start with 'builtIn_'.");
    }

    [Fact]
    public async Task KeyNotStartingWithBuiltInPrefix_ShouldPass()
    {
        var result = await _validator.ValidateAsync(new TestPromptParameter { Key = "custom_key", Value = "value" });
        result.IsValid.ShouldBeTrue();
    }
}
