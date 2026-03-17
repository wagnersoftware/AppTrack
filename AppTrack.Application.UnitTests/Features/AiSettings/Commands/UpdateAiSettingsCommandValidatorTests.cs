using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using FluentValidation.TestHelper;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Commands;

public class UpdateAiSettingsCommandValidatorTests
{
    private const string OwnerId = "user-1";
    private const string OtherId = "other-user";
    private const int ExistingId = 1;

    private readonly Mock<IAiSettingsRepository> _mockRepo;
    private readonly UpdateAiSettingsCommandValidator _validator;

    public UpdateAiSettingsCommandValidatorTests()
    {
        _mockRepo = new Mock<IAiSettingsRepository>();

        var existingSettings = new DomainAiSettings
        {
            Id = ExistingId,
            UserId = OwnerId
        };

        _mockRepo
            .Setup(r => r.GetByIdAsync(ExistingId))
            .ReturnsAsync(existingSettings);

        _mockRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingId)))
            .ReturnsAsync((DomainAiSettings?)null);

        _validator = new UpdateAiSettingsCommandValidator(_mockRepo.Object);
    }

    private static UpdateAiSettingsCommand BuildValidCommand(int id = ExistingId, string userId = OwnerId) => new()
    {
        Id = id,
        UserId = userId,
        SelectedChatModelId = 1,
        PromptParameter = new List<PromptParameterDto>
        {
            new() { Key = "Temperature", Value = "0.7" }
        }
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
    public async Task Validate_ShouldHaveError_WhenAiSettingsNotFound()
    {
        var command = BuildValidCommand(id: 9999);
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Id" && e.ErrorMessage == "Ai settings not found.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenAiSettingsBelongsToAnotherUser()
    {
        var command = BuildValidCommand(id: ExistingId, userId: OtherId);
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "UserId" && e.ErrorMessage == "Ai settings not assigned to this user.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenDuplicateKeysExistInPromptParameters()
    {
        var command = BuildValidCommand();
        command.PromptParameter = new List<PromptParameterDto>
        {
            new() { Key = "Temperature", Value = "0.8" },
            new() { Key = "Temperature", Value = "0.9" }
        };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.PromptParameter)
            .WithErrorMessage("Each prompt parameter key must be unique.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenPromptParameterValueIsEmpty()
    {
        var command = BuildValidCommand();
        command.PromptParameter = new List<PromptParameterDto>
        {
            new() { Key = "Temperature", Value = "" }
        };
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Value is required.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenPromptParameterKeyIsEmpty()
    {
        var command = BuildValidCommand();
        command.PromptParameter = new List<PromptParameterDto>
        {
            new() { Key = "", Value = "TestValue" }
        };
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Key is required.");
    }
}
