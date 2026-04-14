using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.UnitTests.Mocks;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Commands;

public class UpsertFreelancerProfileCommandHandlerTests
{
    private readonly Mock<IFreelancerProfileRepository> _mockRepo;
    private readonly Mock<IAiSettingsRepository> _mockAiSettingsRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UpsertFreelancerProfileCommandHandler _handler;

    public UpsertFreelancerProfileCommandHandlerTests()
    {
        _mockRepo = MockFreelancerProfileRepository.GetMock();
        _mockAiSettingsRepo = MockAiSettingsRepository.GetMock();

        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((action, ct) => action(ct));

        _handler = new UpsertFreelancerProfileCommandHandler(_mockRepo.Object, _mockAiSettingsRepo.Object, _mockUnitOfWork.Object);
    }

    private static UpsertFreelancerProfileCommand ValidCommand(string userId = MockFreelancerProfileRepository.ExistingUserId) => new()
    {
        UserId = userId,
        FirstName = "Anna",
        LastName = "Müller",
    };

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenCommandIsValid()
    {
        // Arrange
        var command = ValidCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<FreelancerProfileDto>();
        result.FirstName.ShouldBe("Anna");
        result.LastName.ShouldBe("Müller");
    }

    [Fact]
    public async Task Handle_ShouldCreate_WhenNoExistingProfile()
    {
        // Arrange
        var command = ValidCommand(userId: "new-user");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        _mockRepo.Verify(r => r.UpsertAsync(It.Is<AppTrack.Domain.FreelancerProfile>(p => p.Id == 0)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdate_WhenProfileExists()
    {
        // Arrange
        var command = ValidCommand(userId: MockFreelancerProfileRepository.ExistingUserId);
        command.FirstName = "Updated";

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FirstName.ShouldBe("Updated");
        _mockRepo.Verify(r => r.UpsertAsync(It.Is<AppTrack.Domain.FreelancerProfile>(p => p.Id == MockFreelancerProfileRepository.ExistingId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenHourlyRateIsNegative()
    {
        // Arrange
        var command = ValidCommand();
        command.HourlyRate = -10;

        // Act & Assert
        var ex = await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        ex.ValidationErrors.ShouldContainKey("HourlyRate");
    }

    [Fact]
    public async Task Handle_ShouldNotCallUpsertAsync_WhenValidationFails()
    {
        // Arrange
        var command = ValidCommand();
        command.HourlyRate = -10;

        // Act & Assert
        await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _mockRepo.Verify(r => r.UpsertAsync(It.IsAny<AppTrack.Domain.FreelancerProfile>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreate_WhenNamesAreNull()
    {
        // Arrange — use a userId with no existing profile to hit the create-path
        var command = new UpsertFreelancerProfileCommand
        {
            UserId = "nameless-user",
            FirstName = null,
            LastName = null,
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.FirstName.ShouldBeNull();
        result.LastName.ShouldBeNull();
        _mockRepo.Verify(r => r.UpsertAsync(It.Is<AppTrack.Domain.FreelancerProfile>(p => p.Id == 0)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSyncBuiltInParameters_WhenAiSettingsExist()
    {
        // Arrange — use the userId that MockAiSettingsRepository knows about
        var command = ValidCommand(userId: MockAiSettingsRepository.ExistingUserId);
        command.FirstName = "Jane";
        command.LastName = "Doe";
        command.HourlyRate = 120m;
        command.Skills = "C#, Azure";

        _mockRepo
            .Setup(r => r.GetByUserIdAsync(MockAiSettingsRepository.ExistingUserId))
            .ReturnsAsync((AppTrack.Domain.FreelancerProfile?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert: UpdateAsync called once on the AiSettings repo
        _mockAiSettingsRepo.Verify(r => r.UpdateAsync(It.IsAny<AppTrack.Domain.AiSettings>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateAiSettings_WhenNoAiSettingsForUser()
    {
        // Arrange — "new-user" has no AiSettings in the mock
        var command = ValidCommand(userId: "new-user");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert: CreateAsync called to bootstrap AiSettings, then UpdateAsync to sync parameters
        _mockAiSettingsRepo.Verify(r => r.CreateAsync(It.IsAny<AppTrack.Domain.AiSettings>()), Times.Once);
        _mockAiSettingsRepo.Verify(r => r.UpdateAsync(It.IsAny<AppTrack.Domain.AiSettings>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAddBuiltInParameter_WhenFieldHasValue()
    {
        // Arrange
        var aiSettings = new AppTrack.Domain.AiSettings
        {
            Id = 99,
            UserId = "sync-user",
            SelectedChatModelId = 1,
        };
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdTrackedAsync("sync-user"))
            .ReturnsAsync(aiSettings);
        _mockRepo
            .Setup(r => r.GetByUserIdAsync("sync-user"))
            .ReturnsAsync((AppTrack.Domain.FreelancerProfile?)null);

        var command = new UpsertFreelancerProfileCommand
        {
            UserId = "sync-user",
            FirstName = "Alice",
            HourlyRate = 90m,
            WorkMode = RemotePreference.Remote,
            AvailableFrom = new DateOnly(2025, 6, 1),
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert: parameters were added and UpdateAsync was called
        aiSettings.PromptParameter.ShouldContain(p => p.Key == "builtIn_FirstName" && p.Value == "Alice");
        aiSettings.PromptParameter.ShouldContain(p => p.Key == "builtIn_HourlyRate" && p.Value == "90");
        aiSettings.PromptParameter.ShouldContain(p => p.Key == "builtIn_WorkMode" && p.Value == "Remote");
        aiSettings.PromptParameter.ShouldContain(p => p.Key == "builtIn_AvailableFrom" && p.Value == "2025-06-01");
        _mockAiSettingsRepo.Verify(r => r.UpdateAsync(aiSettings), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingBuiltInParameter_WhenParameterAlreadyExists()
    {
        // Arrange
        var existingParam = AppTrack.Domain.PromptParameter.Create("builtIn_FirstName", "OldName");
        var aiSettings = new AppTrack.Domain.AiSettings
        {
            Id = 99,
            UserId = "update-user",
            SelectedChatModelId = 1,
            PromptParameter = new List<AppTrack.Domain.PromptParameter> { existingParam },
        };
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdTrackedAsync("update-user"))
            .ReturnsAsync(aiSettings);
        _mockRepo
            .Setup(r => r.GetByUserIdAsync("update-user"))
            .ReturnsAsync((AppTrack.Domain.FreelancerProfile?)null);

        var command = new UpsertFreelancerProfileCommand
        {
            UserId = "update-user",
            FirstName = "NewName",
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert: value updated in-place, collection still has one entry for that key
        existingParam.Value.ShouldBe("NewName");
        aiSettings.PromptParameter.Count(p => p.Key == "builtIn_FirstName").ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ShouldRemoveBuiltInParameter_WhenFieldBecomesNull()
    {
        // Arrange
        var existingParam = AppTrack.Domain.PromptParameter.Create("builtIn_FirstName", "OldName");
        var aiSettings = new AppTrack.Domain.AiSettings
        {
            Id = 99,
            UserId = "remove-user",
            SelectedChatModelId = 1,
            PromptParameter = new List<AppTrack.Domain.PromptParameter> { existingParam },
        };
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdTrackedAsync("remove-user"))
            .ReturnsAsync(aiSettings);
        _mockRepo
            .Setup(r => r.GetByUserIdAsync("remove-user"))
            .ReturnsAsync((AppTrack.Domain.FreelancerProfile?)null);

        var command = new UpsertFreelancerProfileCommand
        {
            UserId = "remove-user",
            FirstName = null,   // null → should remove existing builtIn_FirstName
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert: the parameter was removed
        aiSettings.PromptParameter.ShouldNotContain(p => p.Key == "builtIn_FirstName");
        _mockAiSettingsRepo.Verify(r => r.UpdateAsync(aiSettings), Times.Once);
    }
}
