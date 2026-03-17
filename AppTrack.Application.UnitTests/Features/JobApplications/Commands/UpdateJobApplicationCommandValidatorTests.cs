using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using FluentValidation.TestHelper;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Commands;

public class UpdateJobApplicationCommandValidatorTests
{
    private const string OwnerId = "owner-user";
    private const string OtherId = "other-user";
    private const int ExistingId = 10;

    private readonly Mock<IJobApplicationRepository> _mockRepo;
    private readonly UpdateJobApplicationCommandValidator _validator;

    public UpdateJobApplicationCommandValidatorTests()
    {
        _mockRepo = new Mock<IJobApplicationRepository>();

        var existingEntity = new JobApplication
        {
            Id = ExistingId,
            UserId = OwnerId,
            Name = "Existing Application"
        };

        _mockRepo
            .Setup(r => r.GetByIdAsync(ExistingId))
            .ReturnsAsync(existingEntity);

        _mockRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingId)))
            .ReturnsAsync((JobApplication?)null);

        _validator = new UpdateJobApplicationCommandValidator(_mockRepo.Object);
    }

    private static UpdateJobApplicationCommand BuildValidCommand(int id = ExistingId, string userId = OwnerId) => new()
    {
        Id = id,
        UserId = userId,
        Name = "Valid Name",
        Position = "Valid Position",
        URL = "https://company.com/job",
        JobDescription = "Valid description",
        Location = "Remote",
        ContactPerson = "Jane Doe",
        StartDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        Status = JobApplicationStatus.New,
        DurationInMonths = "6"
    };

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var result = await _validator.TestValidateAsync(BuildValidCommand());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationDoesNotExist()
    {
        var command = BuildValidCommand(id: 9999);
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Job application doesn't exist for user");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationBelongsToAnotherUser()
    {
        var command = BuildValidCommand(id: ExistingId, userId: OtherId);
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Job application doesn't exist for user");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenNameIsEmpty()
    {
        var command = BuildValidCommand();
        command.Name = string.Empty;
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenNameExceeds200Characters()
    {
        var command = BuildValidCommand();
        command.Name = new string('x', 201);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 200 characters.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenPositionExceeds200Characters()
    {
        var command = BuildValidCommand();
        command.Position = new string('x', 201);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Position)
            .WithErrorMessage("Position must not exceed 200 characters.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenUrlIsInvalid()
    {
        var command = BuildValidCommand();
        command.URL = "invalid-url";
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.URL)
            .WithErrorMessage("URL must be a valid URL.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenUrlExceeds1000Characters()
    {
        var command = BuildValidCommand();
        command.URL = "https://acme.com/" + new string('x', 984);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.URL)
            .WithErrorMessage("URL must not exceed 1000 characters.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenLocationExceeds200Characters()
    {
        var command = BuildValidCommand();
        command.Location = new string('x', 201);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location must not exceed 200 characters.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenContactPersonExceeds200Characters()
    {
        var command = BuildValidCommand();
        command.ContactPerson = new string('x', 201);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ContactPerson)
            .WithErrorMessage("Contact Person must not exceed 200 characters.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenDurationInMonthsIsNotNumeric()
    {
        var command = BuildValidCommand();
        command.DurationInMonths = "abc";
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.DurationInMonths)
            .WithErrorMessage("Duration In Months must be a valid number.");
    }

    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenDurationInMonthsIsEmpty()
    {
        var command = BuildValidCommand();
        command.DurationInMonths = string.Empty;
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.DurationInMonths);
    }
}
