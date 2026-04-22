using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Commands.UpdateRssMonitoringSettings;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Commands;

public class UpdateRssMonitoringSettingsCommandHandlerTests
{
    private readonly Mock<IRssMonitoringSettingsRepository> _mockRepo;
    private readonly Mock<IValidator<UpdateRssMonitoringSettingsCommand>> _mockValidator;

    public UpdateRssMonitoringSettingsCommandHandlerTests()
    {
        _mockRepo = new Mock<IRssMonitoringSettingsRepository>();
        _mockValidator = new Mock<IValidator<UpdateRssMonitoringSettingsCommand>>();
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateRssMonitoringSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private UpdateRssMonitoringSettingsCommandHandler CreateHandler() =>
        new(_mockRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldCallUpsert_WhenCommandIsValid()
    {
        var command = new UpdateRssMonitoringSettingsCommand
        {
            UserId = "user-1",
            NotificationEmail = "user@example.com",
            Keywords = ["dotnet", "azure"],
            PollIntervalMinutes = 60
        };

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockRepo.Verify(r => r.UpsertAsync(It.Is<RssMonitoringSettings>(
            s => s.UserId == "user-1" &&
                 s.NotificationEmail == "user@example.com" &&
                 s.Keywords.SequenceEqual(new List<string> { "dotnet", "azure" }) &&
                 s.PollIntervalMinutes == 60)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnit_WhenCommandIsValid()
    {
        var command = new UpdateRssMonitoringSettingsCommand
        {
            UserId = "user-1",
            Keywords = ["dotnet"],
            PollIntervalMinutes = 30
        };

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.ShouldBe(Unit.Value);
    }
}
