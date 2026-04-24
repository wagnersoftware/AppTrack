using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Commands.PollRssFeeds;
using AppTrack.Application.Features.RssFeeds.Models;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Commands;

public class PollRssFeedsCommandHandlerTests
{
    private readonly Mock<IUserRssSubscriptionRepository> _mockSubRepo;
    private readonly Mock<IRssMonitoringSettingsRepository> _mockSettingsRepo;
    private readonly Mock<IJobApplicationRepository> _mockJobAppRepo;
    private readonly Mock<IProcessedFeedItemRepository> _mockProcessedRepo;
    private readonly Mock<IRssFeedReader> _mockFeedReader;
    private readonly Mock<IRssFeedItemParser> _mockParser;
    private readonly Mock<IRssMatchNotifier> _mockNotifier;
    private readonly Mock<IUnitOfWork> _mockUow;

    private const string UserId = "user-1";

    // Instance fields — not static — to avoid cross-test mutation of LastPolledAt
    private readonly RssPortal _portal;
    private readonly RssMonitoringSettings _settings;
    private readonly RawFeedItem _matchingItem;
    private readonly RawFeedItem _nonMatchingItem;
    private readonly RssJobApplicationData _parsedData;

    private UserRssSubscription NewSubscription() => new()
    {
        Id = 1, UserId = UserId, RssPortalId = 1, IsActive = true,
        LastPolledAt = null, RssPortal = _portal
    };

    public PollRssFeedsCommandHandlerTests()
    {
        _portal = new RssPortal
        {
            Id = 1, Name = "Freelancermap", Url = "https://freelancermap.de",
            ParserType = RssParserType.FreelancerMap, IsActive = true
        };

        _settings = new RssMonitoringSettings
        {
            UserId = UserId, Keywords = ["dotnet"], PollIntervalMinutes = 60,
            NotificationEmail = "user@example.com"
        };

        _matchingItem = new RawFeedItem(
            "Senior .NET Developer", "https://freelancermap.de/job/1", "dotnet azure", DateTime.UtcNow);

        _nonMatchingItem = new RawFeedItem(
            "Marketing Manager", "https://freelancermap.de/job/2", "brand strategy", DateTime.UtcNow);

        _parsedData = new RssJobApplicationData(
            "Senior .NET Developer", "https://freelancermap.de/job/1", "dotnet azure", "", "Freelancermap");

        _mockSubRepo = new Mock<IUserRssSubscriptionRepository>();
        _mockSettingsRepo = new Mock<IRssMonitoringSettingsRepository>();
        _mockJobAppRepo = new Mock<IJobApplicationRepository>();
        _mockProcessedRepo = new Mock<IProcessedFeedItemRepository>();
        _mockFeedReader = new Mock<IRssFeedReader>();
        _mockParser = new Mock<IRssFeedItemParser>();
        _mockNotifier = new Mock<IRssMatchNotifier>();
        _mockUow = new Mock<IUnitOfWork>();

        _mockSubRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync())
            .ReturnsAsync(() => [NewSubscription()]);
        _mockSettingsRepo.Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync(_settings);
        _mockFeedReader.Setup(r => r.ReadAsync(_portal.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync([_matchingItem, _nonMatchingItem]);
        _mockProcessedRepo.Setup(r => r.GetProcessedUrlsAsync(UserId, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([]);
        _mockParser.Setup(p => p.Parse(_matchingItem, RssParserType.FreelancerMap, "Freelancermap"))
            .Returns(_parsedData);
        _mockParser.Setup(p => p.Parse(_nonMatchingItem, RssParserType.FreelancerMap, "Freelancermap"))
            .Returns(new RssJobApplicationData("Marketing Manager", "https://freelancermap.de/job/2", "brand strategy", "", "Freelancermap"));
        _mockUow.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(async (action, ct) => await action(ct));
        _mockJobAppRepo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>()))
            .Returns(Task.CompletedTask);
        _mockProcessedRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedFeedItem>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockSubRepo.Setup(r => r.UpdateAsync(It.IsAny<UserRssSubscription>()))
            .Returns(Task.CompletedTask);
    }

    private PollRssFeedsCommandHandler CreateHandler() => new(
        _mockSubRepo.Object,
        _mockSettingsRepo.Object,
        _mockJobAppRepo.Object,
        _mockProcessedRepo.Object,
        _mockFeedReader.Object,
        _mockParser.Object,
        _mockNotifier.Object,
        _mockUow.Object);

    [Fact]
    public async Task Handle_ShouldCreateJobApplication_ForMatchingItem()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(
            j => j.Position == "Senior .NET Developer" &&
                 j.Status == JobApplicationStatus.Discovered &&
                 j.UserId == UserId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCreateJobApplication_ForNonMatchingItem()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(
            j => j.Position == "Marketing Manager")), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipAlreadyProcessedItems()
    {
        _mockProcessedRepo.Setup(r => r.GetProcessedUrlsAsync(UserId, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(["https://freelancermap.de/job/1"]);

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.IsAny<JobApplication>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSendNotification_WhenMatchesFound()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockNotifier.Verify(n => n.NotifyAsync(
            UserId,
            "user@example.com",
            It.Is<List<RssJobApplicationData>>(m => m.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotSendNotification_WhenNoMatchesFound()
    {
        _mockFeedReader.Setup(r => r.ReadAsync(_portal.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync([_nonMatchingItem]);

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockNotifier.Verify(n => n.NotifyAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<RssJobApplicationData>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipUser_WhenNoSettingsExist()
    {
        _mockSettingsRepo.Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync((RssMonitoringSettings?)null);

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockFeedReader.Verify(r => r.ReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipUser_WhenKeywordsAreEmpty()
    {
        _mockSettingsRepo.Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync(new RssMonitoringSettings { UserId = UserId, Keywords = [], PollIntervalMinutes = 60 });

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockFeedReader.Verify(r => r.ReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipSubscription_WhenNotDue()
    {
        var notDueSub = new UserRssSubscription
        {
            Id = 1, UserId = UserId, RssPortalId = 1, IsActive = true,
            LastPolledAt = DateTime.UtcNow.AddMinutes(-30),
            RssPortal = _portal
        };
        _mockSubRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync()).ReturnsAsync([notDueSub]);

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockFeedReader.Verify(r => r.ReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMarkItemsAsProcessed_AfterMatching()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockProcessedRepo.Verify(r => r.AddRangeAsync(
            It.Is<IEnumerable<ProcessedFeedItem>>(items =>
                items.Any(i => i.FeedItemUrl == "https://freelancermap.de/job/1" && i.UserId == UserId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateLastPolledAt_AfterProcessing()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockSubRepo.Verify(r => r.UpdateAsync(It.Is<UserRssSubscription>(
            s => s.LastPolledAt != null)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseTransaction_ForPersistenceOperations()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockUow.Verify(u => u.ExecuteInTransactionAsync(
            It.IsAny<Func<CancellationToken, Task>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetJobApplicationName_ToPortalName_WhenCompanyNameEmpty()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(
            j => j.Name == "Freelancermap")), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetJobApplicationName_ToCompanyName_WhenAvailable()
    {
        _mockParser.Setup(p => p.Parse(_matchingItem, RssParserType.FreelancerMap, "Freelancermap"))
            .Returns(_parsedData with { CompanyName = "Acme GmbH" });

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(
            j => j.Name == "Acme GmbH")), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkipUser_WhenNotificationEmailIsEmpty()
    {
        _mockSettingsRepo.Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync(new RssMonitoringSettings
            {
                UserId = UserId, Keywords = ["dotnet"], PollIntervalMinutes = 60,
                NotificationEmail = string.Empty
            });

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockFeedReader.Verify(r => r.ReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
