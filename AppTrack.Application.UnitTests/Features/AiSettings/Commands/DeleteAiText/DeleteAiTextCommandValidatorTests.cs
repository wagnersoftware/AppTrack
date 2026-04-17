using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.AiSettings.Commands.DeleteAiText;
using FluentValidation.TestHelper;
using Moq;
using Shouldly;
using DomainJobApplication = AppTrack.Domain.JobApplication;
using DomainJobApplicationAiText = AppTrack.Domain.JobApplicationAiText;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Commands.DeleteAiText;

public class DeleteAiTextCommandValidatorTests
{
    private const string OwnerId = "owner-user";
    private const string OtherId = "other-user";
    private const int ExistingAiTextId = 10;
    private const int ExistingJobApplicationId = 1;

    private readonly Mock<IJobApplicationAiTextRepository> _mockAiTextRepo;
    private readonly Mock<IJobApplicationRepository> _mockJobAppRepo;
    private readonly DeleteAiTextCommandValidator _validator;

    public DeleteAiTextCommandValidatorTests()
    {
        _mockAiTextRepo = new Mock<IJobApplicationAiTextRepository>();
        _mockJobAppRepo = new Mock<IJobApplicationRepository>();

        var existingAiText = new DomainJobApplicationAiText
        {
            Id = ExistingAiTextId,
            JobApplicationId = ExistingJobApplicationId,
            PromptKey = "cover-letter",
            GeneratedText = "Some generated text"
        };

        var ownerJobApplication = new DomainJobApplication
        {
            Id = ExistingJobApplicationId,
            UserId = OwnerId
        };

        _mockAiTextRepo
            .Setup(r => r.GetByIdAsync(ExistingAiTextId))
            .ReturnsAsync(existingAiText);

        _mockAiTextRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingAiTextId)))
            .ReturnsAsync((DomainJobApplicationAiText?)null);

        _mockJobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(ownerJobApplication);

        _validator = new DeleteAiTextCommandValidator(_mockAiTextRepo.Object, _mockJobAppRepo.Object);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new DeleteAiTextCommand { Id = ExistingAiTextId, UserId = OwnerId };
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenIdIsZero()
    {
        var command = new DeleteAiTextCommand { Id = 0, UserId = OwnerId };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenAiTextEntryDoesNotExist()
    {
        var command = new DeleteAiTextCommand { Id = 999, UserId = OwnerId };
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "AI text entry not found");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenAiTextBelongsToAnotherUser()
    {
        var command = new DeleteAiTextCommand { Id = ExistingAiTextId, UserId = OtherId };
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "AI text entry does not belong to this user");
    }
}
