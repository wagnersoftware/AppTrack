using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.JobApplications.Commands.DeleteJobApplication;
using AppTrack.Domain;
using FluentValidation.TestHelper;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Commands;

public class DeleteJobApplicationCommandValidatorTests
{
    private const string OwnerId = "owner-user";
    private const string OtherId = "other-user";
    private const int ExistingId = 42;

    private readonly Mock<IJobApplicationRepository> _mockRepo;
    private readonly DeleteJobApplicationCommandValidator _validator;

    public DeleteJobApplicationCommandValidatorTests()
    {
        _mockRepo = new Mock<IJobApplicationRepository>();

        var existingEntity = new JobApplication
        {
            Id = ExistingId,
            UserId = OwnerId
        };

        _mockRepo
            .Setup(r => r.GetByIdAsync(ExistingId))
            .ReturnsAsync(existingEntity);

        _mockRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingId)))
            .ReturnsAsync((JobApplication?)null);

        _validator = new DeleteJobApplicationCommandValidator(_mockRepo.Object);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new DeleteJobApplicationCommand { Id = ExistingId, UserId = OwnerId };
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenIdIsZero()
    {
        var command = new DeleteJobApplicationCommand { Id = 0, UserId = OwnerId };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationDoesNotExist()
    {
        var command = new DeleteJobApplicationCommand { Id = 999, UserId = OwnerId };
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Job application doesn't exist for user");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationBelongsToAnotherUser()
    {
        var command = new DeleteJobApplicationCommand { Id = ExistingId, UserId = OtherId };
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Job application doesn't exist for user");
    }
}
