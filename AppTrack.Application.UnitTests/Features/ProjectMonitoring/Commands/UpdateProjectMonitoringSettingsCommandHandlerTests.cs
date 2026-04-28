using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ProjectMonitoring.Commands.UpdateProjectMonitoringSettings;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

public class UpdateProjectMonitoringSettingsCommandHandlerTests
{
    private readonly Mock<IProjectMonitoringSettingsRepository> _repository = new();
    private readonly UpdateProjectMonitoringSettingsCommandValidator _validator = new();

    private UpdateProjectMonitoringSettingsCommandHandler CreateHandler() => new(
        _repository.Object,
        _validator);

    private static UpdateProjectMonitoringSettingsCommand BuildValidCommand() => new()
    {
        UserId = "user-1",
        NotificationEmail = "user@example.com",
        Keywords = new List<string> { ".NET", "remote" },
        NotificationIntervalMinutes = 60,
        NotifyByEmail = true
    };

    [Fact]
    public async Task Handle_ShouldCallUpsertWithCorrectlyMappedEntity_WhenCommandIsValid()
    {
        // Arrange
        ProjectMonitoringSettings? captured = null;
        _repository
            .Setup(r => r.UpsertAsync(It.IsAny<ProjectMonitoringSettings>()))
            .Callback<ProjectMonitoringSettings>(s => captured = s)
            .Returns(Task.CompletedTask);

        var command = BuildValidCommand();

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert — all 5 fields mapped correctly
        captured.ShouldNotBeNull();
        captured!.UserId.ShouldBe("user-1");
        captured.NotificationEmail.ShouldBe("user@example.com");
        captured.Keywords.ShouldBe(new List<string> { ".NET", "remote" });
        captured.NotificationIntervalMinutes.ShouldBe(60);
        captured.NotifyByEmail.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ShouldMapNotificationEmail_WhenCommandIsValid()
    {
        // Arrange — NotificationEmail is the branch change; assert it explicitly
        ProjectMonitoringSettings? captured = null;
        _repository
            .Setup(r => r.UpsertAsync(It.IsAny<ProjectMonitoringSettings>()))
            .Callback<ProjectMonitoringSettings>(s => captured = s)
            .Returns(Task.CompletedTask);

        var command = BuildValidCommand();
        command.NotificationEmail = "alerts@company.org";

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        captured.ShouldNotBeNull();
        captured!.NotificationEmail.ShouldBe("alerts@company.org");
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenNotificationEmailIsEmpty()
    {
        // Arrange
        var command = BuildValidCommand();
        command.NotificationEmail = string.Empty;

        // Act & Assert
        await Should.ThrowAsync<BadRequestException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));

        _repository.Verify(r => r.UpsertAsync(It.IsAny<ProjectMonitoringSettings>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnitValue_WhenCommandIsValid()
    {
        // Arrange
        _repository.Setup(r => r.UpsertAsync(It.IsAny<ProjectMonitoringSettings>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await CreateHandler().Handle(BuildValidCommand(), CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
    }
}
