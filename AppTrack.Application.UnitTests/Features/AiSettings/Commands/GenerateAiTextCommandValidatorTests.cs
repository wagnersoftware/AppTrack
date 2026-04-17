using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.AiSettings.Commands.GenerateAiText;
using FluentValidation.TestHelper;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;
using DomainChatModel = AppTrack.Domain.ChatModel;
using DomainJobApplication = AppTrack.Domain.JobApplication;
using DomainPrompt = AppTrack.Domain.Prompt;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Commands;

public class GenerateAiTextCommandValidatorTests
{
    private const string UserId = "user-1";
    private const int ExistingJobApplicationId = 42;
    private const int ExistingAiSettingsId = 1;
    private const int ExistingChatModelId = 10;

    private readonly Mock<IJobApplicationRepository> _jobAppRepo;
    private readonly Mock<IAiSettingsRepository> _aiSettingsRepo;
    private readonly Mock<IChatModelRepository> _chatModelRepo;
    private readonly GenerateAiTextCommandValidator _validator;

    public GenerateAiTextCommandValidatorTests()
    {
        _jobAppRepo = new Mock<IJobApplicationRepository>();
        _aiSettingsRepo = new Mock<IAiSettingsRepository>();
        _chatModelRepo = new Mock<IChatModelRepository>();

        var jobApplication = new DomainJobApplication { Id = ExistingJobApplicationId, UserId = UserId };
        _jobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(jobApplication);
        _jobAppRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingJobApplicationId)))
            .ReturnsAsync((DomainJobApplication?)null);

        var chatModel = new DomainChatModel { Id = ExistingChatModelId, ApiModelName = "gpt-4o" };
        var aiSettings = new DomainAiSettings
        {
            Id = ExistingAiSettingsId,
            UserId = UserId,
            SelectedChatModelId = ExistingChatModelId
        };
        aiSettings.Prompts.Add(DomainPrompt.Create("Default", "Write a cover letter for {position}"));

        _aiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync(aiSettings);
        _aiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(It.Is<string>(id => id != UserId)))
            .ReturnsAsync((DomainAiSettings?)null);

        _chatModelRepo
            .Setup(r => r.GetByIdAsync(ExistingChatModelId))
            .ReturnsAsync(chatModel);
        _chatModelRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingChatModelId)))
            .ReturnsAsync((DomainChatModel?)null);

        _validator = new GenerateAiTextCommandValidator(
            _jobAppRepo.Object,
            _aiSettingsRepo.Object,
            _chatModelRepo.Object);
    }

    private static GenerateAiTextCommand BuildValidCommand(
        string userId = UserId,
        int jobApplicationId = ExistingJobApplicationId,
        string prompt = "My custom prompt",
        string promptKey = "my-prompt-key") => new()
    {
        UserId = userId,
        JobApplicationId = jobApplicationId,
        Prompt = prompt,
        PromptKey = promptKey
    };

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var result = await _validator.TestValidateAsync(BuildValidCommand());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenPromptIsEmpty()
    {
        var command = BuildValidCommand(prompt: string.Empty);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Prompt);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationIdIsZero()
    {
        var command = BuildValidCommand(jobApplicationId: 0);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.JobApplicationId);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationDoesNotExist()
    {
        var command = BuildValidCommand(jobApplicationId: 9999);
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Job application doesn't exist");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenAiSettingsNotFound()
    {
        var command = BuildValidCommand(userId: "unknown-user");
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "AI settings not found for this user.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenChatModelNotFound()
    {
        var aiSettingsWithMissingModel = new DomainAiSettings
        {
            Id = 2,
            UserId = "user-no-model",
            SelectedChatModelId = 9999
        };
        _aiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync("user-no-model"))
            .ReturnsAsync(aiSettingsWithMissingModel);

        var command = BuildValidCommand(userId: "user-no-model");
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "No ChatModel configured on server.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenChatModelApiModelNameIsNull()
    {
        const string userWithNullApiModel = "user-null-apimodel";
        var chatModelWithNullName = new DomainChatModel { Id = 20, ApiModelName = null! };
        var aiSettingsForUser = new DomainAiSettings
        {
            Id = 3,
            UserId = userWithNullApiModel,
            SelectedChatModelId = 20
        };

        _aiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(userWithNullApiModel))
            .ReturnsAsync(aiSettingsForUser);
        _chatModelRepo
            .Setup(r => r.GetByIdAsync(20))
            .ReturnsAsync(chatModelWithNullName);

        var command = BuildValidCommand(userId: userWithNullApiModel);
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "No model name for chat api set.");
    }
}
