using AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
using AppTrack.Domain.Enums;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Commands;

public class CreateJobApplicationCommandValidatorTests
{
    private readonly CreateJobApplicationCommandValidator _validator = new();

    private static CreateJobApplicationCommand BuildValidCommand() => new()
    {
        Name = "Acme Corp",
        Position = "Software Engineer",
        URL = "https://acme.com/jobs/1",
        JobDescription = "Great opportunity",
        Location = "Remote",
        ContactPerson = "Jane Doe",
        StartDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        Status = JobApplicationStatus.New,
        DurationInMonths = "6"
    };

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var result = _validator.TestValidate(BuildValidCommand());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenDurationInMonthsIsEmpty()
    {
        var command = BuildValidCommand();
        command.DurationInMonths = string.Empty;
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.DurationInMonths);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNameIsEmpty()
    {
        var command = BuildValidCommand();
        command.Name = string.Empty;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNameExceeds200Characters()
    {
        var command = BuildValidCommand();
        command.Name = new string('x', 201);
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenNameIsExactly200Characters()
    {
        var command = BuildValidCommand();
        command.Name = new string('x', 200);
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPositionIsEmpty()
    {
        var command = BuildValidCommand();
        command.Position = string.Empty;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Position);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPositionExceeds200Characters()
    {
        var command = BuildValidCommand();
        command.Position = new string('x', 201);
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorMessage("Position must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenUrlIsEmpty()
    {
        var command = BuildValidCommand();
        command.URL = string.Empty;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.URL);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenUrlIsNotAbsolute()
    {
        var command = BuildValidCommand();
        command.URL = "invalid-url";
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.URL)
            .WithErrorMessage("URL must be a valid URL.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenUrlExceeds1000Characters()
    {
        var command = BuildValidCommand();
        command.URL = "https://acme.com/" + new string('x', 984);
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.URL)
            .WithErrorMessage("URL must not exceed 1000 characters.");
    }

    [Theory]
    [InlineData("https://company.com/job")]
    [InlineData("http://company.com/job")]
    public void Validate_ShouldNotHaveError_WhenUrlIsValidAbsolute(string url)
    {
        var command = BuildValidCommand();
        command.URL = url;
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.URL);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLocationIsEmpty()
    {
        var command = BuildValidCommand();
        command.Location = string.Empty;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Location);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLocationExceeds200Characters()
    {
        var command = BuildValidCommand();
        command.Location = new string('x', 201);
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenContactPersonIsEmpty()
    {
        var command = BuildValidCommand();
        command.ContactPerson = string.Empty;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ContactPerson);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenContactPersonExceeds200Characters()
    {
        var command = BuildValidCommand();
        command.ContactPerson = new string('x', 201);
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.ContactPerson)
            .WithErrorMessage("Contact Person must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenJobDescriptionIsEmpty()
    {
        var command = BuildValidCommand();
        command.JobDescription = string.Empty;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.JobDescription);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenStartDateIsDefault()
    {
        var command = BuildValidCommand();
        command.StartDate = default;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDurationInMonthsIsNotNumeric()
    {
        var command = BuildValidCommand();
        command.DurationInMonths = "abc";
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.DurationInMonths)
            .WithErrorMessage("Duration In Months must be a valid number.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDurationInMonthsIsZero()
    {
        var command = BuildValidCommand();
        command.DurationInMonths = "0";
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.DurationInMonths)
            .WithErrorMessage("Duration In Months must be between 1 and 120.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDurationInMonthsExceeds120()
    {
        var command = BuildValidCommand();
        command.DurationInMonths = "121";
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.DurationInMonths)
            .WithErrorMessage("Duration In Months must be between 1 and 120.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenDurationInMonthsIsValidPositiveNumber()
    {
        var command = BuildValidCommand();
        command.DurationInMonths = "12";
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.DurationInMonths);
    }
}
