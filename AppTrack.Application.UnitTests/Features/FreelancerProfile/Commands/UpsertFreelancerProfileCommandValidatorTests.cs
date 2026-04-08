using AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Commands;

public class UpsertFreelancerProfileCommandValidatorTests
{
    private readonly UpsertFreelancerProfileCommandValidator _validator = new();

    private static UpsertFreelancerProfileCommand ValidCommand() => new()
    {
        UserId = "user-1",
        HourlyRate = null,
        DailyRate = null,
        Skills = null,
    };

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var result = await _validator.TestValidateAsync(ValidCommand());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenFirstNameIsNull()
    {
        var command = ValidCommand();
        command.FirstName = null;
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenLastNameIsNull()
    {
        var command = ValidCommand();
        command.LastName = null;
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenFirstNameExceedsMaxLength()
    {
        var command = ValidCommand();
        command.FirstName = new string('x', 101);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenLastNameExceedsMaxLength()
    {
        var command = ValidCommand();
        command.LastName = new string('x', 101);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenHourlyRateIsZero()
    {
        var command = ValidCommand();
        command.HourlyRate = 0;
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.HourlyRate);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenHourlyRateIsNegative()
    {
        var command = ValidCommand();
        command.HourlyRate = -1;
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.HourlyRate);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenHourlyRateIsNull()
    {
        var command = ValidCommand();
        command.HourlyRate = null;
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.HourlyRate);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenSkillsExceedMaxLength()
    {
        var command = ValidCommand();
        command.Skills = new string('x', 1001);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Skills);
    }
}
