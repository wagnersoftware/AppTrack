using AppTrack.Application.Contracts.AiTextGenerator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.AiSettings.Commands.GenerateAiText;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;
using DomainChatModel = AppTrack.Domain.ChatModel;
using DomainJobApplicationAiText = AppTrack.Domain.JobApplicationAiText;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Commands.GenerateAiText;

public class GenerateAiTextCommandHandlerTests
{
    private const string UserId = "user-1";
    private const int JobApplicationId = 42;
    private const string PromptKey = "cover-letter";
    private const string PromptText = "Write a cover letter for {position}";
    private const string GeneratedText = "Here is your cover letter.";
    private const string ApiModelName = "gpt-4o";

    private readonly Mock<IAiTextGenerator> _mockAiTextGenerator;
    private readonly Mock<IAiSettingsRepository> _mockAiSettingsRepo;
    private readonly Mock<IChatModelRepository> _mockChatModelRepo;
    private readonly Mock<IJobApplicationAiTextRepository> _mockAiTextRepo;
    private readonly Mock<IValidator<GenerateAiTextCommand>> _mockValidator;

    private readonly DomainAiSettings _aiSettings;
    private readonly DomainChatModel _chatModel;

    public GenerateAiTextCommandHandlerTests()
    {
        _mockAiTextGenerator = new Mock<IAiTextGenerator>();
        _mockAiSettingsRepo = new Mock<IAiSettingsRepository>();
        _mockChatModelRepo = new Mock<IChatModelRepository>();
        _mockAiTextRepo = new Mock<IJobApplicationAiTextRepository>();
        _mockValidator = new Mock<IValidator<GenerateAiTextCommand>>();

        _chatModel = new DomainChatModel { Id = 10, ApiModelName = ApiModelName };
        _aiSettings = new DomainAiSettings
        {
            Id = 1,
            UserId = UserId,
            SelectedChatModelId = 10,
            Language = AiResponseLanguage.English,
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GenerateAiTextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync(_aiSettings);

        _mockChatModelRepo
            .Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(_chatModel);

        _mockAiTextGenerator
            .Setup(g => g.GenerateAiTextAsync(PromptText, ApiModelName, AiResponseLanguage.English, It.IsAny<CancellationToken>()))
            .ReturnsAsync(GeneratedText);

        _mockAiTextRepo
            .Setup(r => r.CountByJobApplicationAndPromptAsync(JobApplicationId, PromptKey))
            .ReturnsAsync(0);

        _mockAiTextRepo
            .Setup(r => r.AddAsync(It.IsAny<DomainJobApplicationAiText>()))
            .Returns(Task.CompletedTask);

        _mockAiTextRepo
            .Setup(r => r.DeleteAsync(It.IsAny<DomainJobApplicationAiText>()))
            .Returns(Task.CompletedTask);
    }

    private GenerateAiTextCommandHandler CreateHandler() =>
        new(_mockAiTextGenerator.Object,
            _mockAiSettingsRepo.Object,
            _mockChatModelRepo.Object,
            _mockAiTextRepo.Object,
            _mockValidator.Object);

    private static GenerateAiTextCommand BuildValidCommand() => new()
    {
        UserId = UserId,
        JobApplicationId = JobApplicationId,
        Prompt = PromptText,
        PromptKey = PromptKey,
    };

    [Fact]
    public async Task Handle_ShouldReturnGeneratedAiTextDto_WhenCommandIsValid()
    {
        var command = BuildValidCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<GeneratedAiTextDto>();
        result.GeneratedText.ShouldBe(GeneratedText);
    }

    [Fact]
    public async Task Handle_ShouldCallAiTextGenerator_WithCorrectArguments()
    {
        var command = BuildValidCommand();
        var handler = CreateHandler();

        await handler.Handle(command, CancellationToken.None);

        _mockAiTextGenerator.Verify(
            g => g.GenerateAiTextAsync(PromptText, ApiModelName, AiResponseLanguage.English, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsync_WithCorrectJobApplicationIdAndPromptKey()
    {
        var command = BuildValidCommand();
        var handler = CreateHandler();

        await handler.Handle(command, CancellationToken.None);

        _mockAiTextRepo.Verify(
            r => r.AddAsync(It.Is<DomainJobApplicationAiText>(t =>
                t.JobApplicationId == JobApplicationId &&
                t.PromptKey == PromptKey &&
                t.GeneratedText == GeneratedText)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenValidationFails()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GenerateAiTextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Prompt", "Prompt is required")]));

        var command = BuildValidCommand();
        var handler = CreateHandler();

        await Should.ThrowAsync<BadRequestException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotCallAiTextGenerator_WhenValidationFails()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GenerateAiTextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Prompt", "Prompt is required")]));

        var command = BuildValidCommand();
        var handler = CreateHandler();

        await Should.ThrowAsync<BadRequestException>(() =>
            handler.Handle(command, CancellationToken.None));

        _mockAiTextGenerator.Verify(
            g => g.GenerateAiTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AiResponseLanguage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteOldestEntry_WhenHistoryCountIsBelow5()
    {
        _mockAiTextRepo
            .Setup(r => r.CountByJobApplicationAndPromptAsync(JobApplicationId, PromptKey))
            .ReturnsAsync(4);

        var command = BuildValidCommand();
        var handler = CreateHandler();

        await handler.Handle(command, CancellationToken.None);

        _mockAiTextRepo.Verify(
            r => r.GetOldestByJobApplicationAndPromptAsync(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
        _mockAiTextRepo.Verify(
            r => r.DeleteAsync(It.IsAny<DomainJobApplicationAiText>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteOldestEntry_WhenHistoryCountReaches5()
    {
        var oldestEntry = new DomainJobApplicationAiText
        {
            Id = 1,
            JobApplicationId = JobApplicationId,
            PromptKey = PromptKey,
            GeneratedText = "old text",
        };

        _mockAiTextRepo
            .Setup(r => r.CountByJobApplicationAndPromptAsync(JobApplicationId, PromptKey))
            .ReturnsAsync(5);
        _mockAiTextRepo
            .Setup(r => r.GetOldestByJobApplicationAndPromptAsync(JobApplicationId, PromptKey))
            .ReturnsAsync(oldestEntry);

        var command = BuildValidCommand();
        var handler = CreateHandler();

        await handler.Handle(command, CancellationToken.None);

        _mockAiTextRepo.Verify(r => r.DeleteAsync(oldestEntry), Times.Once);
        _mockAiTextRepo.Verify(r => r.AddAsync(It.IsAny<DomainJobApplicationAiText>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteAnything_WhenHistoryCountIs5ButOldestIsNull()
    {
        _mockAiTextRepo
            .Setup(r => r.CountByJobApplicationAndPromptAsync(JobApplicationId, PromptKey))
            .ReturnsAsync(5);
        _mockAiTextRepo
            .Setup(r => r.GetOldestByJobApplicationAndPromptAsync(JobApplicationId, PromptKey))
            .ReturnsAsync((DomainJobApplicationAiText?)null);

        var command = BuildValidCommand();
        var handler = CreateHandler();

        await handler.Handle(command, CancellationToken.None);

        _mockAiTextRepo.Verify(r => r.DeleteAsync(It.IsAny<DomainJobApplicationAiText>()), Times.Never);
        _mockAiTextRepo.Verify(r => r.AddAsync(It.IsAny<DomainJobApplicationAiText>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDeleteOldestEntry_WhenHistoryCountExceeds5()
    {
        var oldestEntry = new DomainJobApplicationAiText
        {
            Id = 99,
            JobApplicationId = JobApplicationId,
            PromptKey = PromptKey,
            GeneratedText = "very old text",
        };

        _mockAiTextRepo
            .Setup(r => r.CountByJobApplicationAndPromptAsync(JobApplicationId, PromptKey))
            .ReturnsAsync(7);
        _mockAiTextRepo
            .Setup(r => r.GetOldestByJobApplicationAndPromptAsync(JobApplicationId, PromptKey))
            .ReturnsAsync(oldestEntry);

        var command = BuildValidCommand();
        var handler = CreateHandler();

        await handler.Handle(command, CancellationToken.None);

        _mockAiTextRepo.Verify(r => r.DeleteAsync(oldestEntry), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetGeneratedAtToUtcNow_WhenAddingAiText()
    {
        var before = DateTime.UtcNow;
        var command = BuildValidCommand();
        var handler = CreateHandler();

        await handler.Handle(command, CancellationToken.None);

        var after = DateTime.UtcNow;

        _mockAiTextRepo.Verify(
            r => r.AddAsync(It.Is<DomainJobApplicationAiText>(t =>
                t.GeneratedAt >= before && t.GeneratedAt <= after)),
            Times.Once);
    }
}
