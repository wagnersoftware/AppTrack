using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ProjectMonitoring.Commands.UpdateProjectMonitoringSettings;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

public class UpdateProjectMonitoringSettingsCommandHandlerTests
{
    private readonly Mock<IProjectMonitoringSettingsRepository> _mockRepo;
    private readonly Mock<IValidator<UpdateProjectMonitoringSettingsCommand>> _mockValidator;

    public UpdateProjectMonitoringSettingsCommandHandlerTests()
    {
        _mockRepo = new Mock<IProjectMonitoringSettingsRepository>();
        _mockValidator = new Mock<IValidator<UpdateProjectMonitoringSettingsCommand>>();
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateProjectMonitoringSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private UpdateProjectMonitoringSettingsCommandHandler CreateHandler() =>
        new(_mockRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldCallUpsert_WhenCommandIsValid()
    {
        var command = new UpdateProjectMonitoringSettingsCommand
        {
            UserId = "user-1",
            NotificationEmail = "user@example.com",
            Keywords = ["dotnet", "azure"],
            NotificationIntervalMinutes = 60,
            PollIntervalMinutes = 30
        };

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockRepo.Verify(r => r.UpsertAsync(It.Is<ProjectMonitoringSettings>(
            s => s.UserId == "user-1" &&
                 s.NotificationEmail == "user@example.com" &&
                 s.Keywords.SequenceEqual(new List<string> { "dotnet", "azure" }) &&
                 s.NotificationIntervalMinutes == 60 &&
                 s.PollIntervalMinutes == 30)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnit_WhenCommandIsValid()
    {
        var command = new UpdateProjectMonitoringSettingsCommand
        {
            UserId = "user-1",
            Keywords = ["dotnet"],
            NotificationIntervalMinutes = 30
        };

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.ShouldBe(Unit.Value);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenValidationFails()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateProjectMonitoringSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("NotificationIntervalMinutes", "Out of range")]));

        await Should.ThrowAsync<BadRequestException>(() =>
            CreateHandler().Handle(new UpdateProjectMonitoringSettingsCommand(), CancellationToken.None));

        _mockRepo.Verify(r => r.UpsertAsync(It.IsAny<ProjectMonitoringSettings>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCallUpsert_WhenValidationFails()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateProjectMonitoringSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Keywords", "Required")]));

        await Should.ThrowAsync<BadRequestException>(() =>
            CreateHandler().Handle(new UpdateProjectMonitoringSettingsCommand(), CancellationToken.None));

        _mockRepo.Verify(r => r.UpsertAsync(It.IsAny<ProjectMonitoringSettings>()), Times.Never);
    }
}
