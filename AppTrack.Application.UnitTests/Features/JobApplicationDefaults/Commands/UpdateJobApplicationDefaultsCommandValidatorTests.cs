using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;
using FluentValidation.TestHelper;
using Moq;
using Shouldly;
using DomainJobApplicationDefaults = AppTrack.Domain.JobApplicationDefaults;

namespace AppTrack.Application.UnitTests.Features.JobApplicationDefaults.Commands;

public class UpdateJobApplicationDefaultsCommandValidatorTests
{
    private const string OwnerId = "user-123";
    private const string OtherId = "other-user";
    private const int ExistingId = 1;

    private readonly Mock<IJobApplicationDefaultsRepository> _mockRepo;
    private readonly UpdateJobApplicationDefaultsCommandValidator _validator;

    public UpdateJobApplicationDefaultsCommandValidatorTests()
    {
        _mockRepo = new Mock<IJobApplicationDefaultsRepository>();

        var existingDefaults = new DomainJobApplicationDefaults
        {
            Id = ExistingId,
            UserId = OwnerId,
            Name = "Existing",
            Position = "Existing",
            Location = "Existing"
        };

        _mockRepo
            .Setup(r => r.GetByIdAsync(ExistingId))
            .ReturnsAsync(existingDefaults);

        _mockRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingId)))
            .ReturnsAsync((DomainJobApplicationDefaults?)null);

        _validator = new UpdateJobApplicationDefaultsCommandValidator(_mockRepo.Object);
    }

    private static UpdateJobApplicationDefaultsCommand BuildValidCommand(int id = ExistingId, string userId = OwnerId) => new()
    {
        Id = id,
        UserId = userId,
        Name = "Valid Name",
        Position = "Valid Position",
        Location = "Valid Location"
    };

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var result = await _validator.TestValidateAsync(BuildValidCommand());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenIdIsZero()
    {
        var command = BuildValidCommand(id: 0);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenEntityDoesNotExist()
    {
        var command = BuildValidCommand(id: 9999);
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Id" && e.ErrorMessage == "Job Application Defaults not found.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenEntityBelongsToAnotherUser()
    {
        var command = BuildValidCommand(id: ExistingId, userId: OtherId);
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "UserId" && e.ErrorMessage == "Job Application Defaults not assigned to this user.");
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
    public async Task Validate_ShouldHaveError_WhenLocationExceeds200Characters()
    {
        var command = BuildValidCommand();
        command.Location = new string('x', 201);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location must not exceed 200 characters.");
    }
}
