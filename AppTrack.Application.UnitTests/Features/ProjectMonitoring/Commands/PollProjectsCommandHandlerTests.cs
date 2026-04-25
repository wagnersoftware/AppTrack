using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Commands.PollProjects;
using AppTrack.Application.Features.ProjectMonitoring.Models;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

public class PollProjectsCommandHandlerTests
{
    private readonly Mock<IUserPortalSubscriptionRepository> _mockSubRepo;
    private readonly Mock<IProjectMonitoringSettingsRepository> _mockSettingsRepo;
    private readonly Mock<IJobApplicationRepository> _mockJobAppRepo;
    private readonly Mock<IProcessedProjectItemRepository> _mockProcessedRepo;
    private readonly Mock<IScrapedProjectRepository> _mockScrapedProjectRepo;
    private readonly Mock<IProjectMatchNotifier> _mockNotifier;
    private readonly Mock<IUnitOfWork> _mockUow;

    private const string UserId = "user-1";

    private readonly ProjectPortal _portal;
    private readonly ProjectMonitoringSettings _settings;
    private readonly ScrapedProject _matchingProject;
    private readonly ScrapedProject _nonMatchingProject;

    private UserPortalSubscription NewSubscription() => new()
    {
        Id = 1, UserId = UserId, ProjectPortalId = 1, IsActive = true, ProjectPortal = _portal
    };

    public PollProjectsCommandHandlerTests()
    {
        _portal = new ProjectPortal
        {
            Id = 1, Name = "Freelancermap", Url = "https://freelancermap.de",
            ScraperType = ScraperType.FreelancerMap, IsActive = true
        };

        _settings = new ProjectMonitoringSettings
        {
            UserId = UserId, Keywords = ["dotnet"], NotificationIntervalMinutes = 60,
            PollIntervalMinutes = 60, LastPolledAt = null,
            NotificationEmail = "user@example.com", NotifyByEmail = true, LastNotifiedAt = null
        };

        _matchingProject = new ScrapedProject
        {
            Id = 1, ProjectPortalId = 1, Title = "Senior dotnet Developer",
            Url = "https://freelancermap.de/job/1", CompanyName = string.Empty,
            ScrapedAt = DateTime.UtcNow, ProjectPortal = _portal
        };

        _nonMatchingProject = new ScrapedProject
        {
            Id = 2, ProjectPortalId = 1, Title = "Marketing Manager",
            Url = "https://freelancermap.de/job/2", CompanyName = string.Empty,
            ScrapedAt = DateTime.UtcNow, ProjectPortal = _portal
        };

        _mockSubRepo = new Mock<IUserPortalSubscriptionRepository>();
        _mockSettingsRepo = new Mock<IProjectMonitoringSettingsRepository>();
        _mockJobAppRepo = new Mock<IJobApplicationRepository>();
        _mockProcessedRepo = new Mock<IProcessedProjectItemRepository>();
        _mockScrapedProjectRepo = new Mock<IScrapedProjectRepository>();
        _mockNotifier = new Mock<IProjectMatchNotifier>();
        _mockUow = new Mock<IUnitOfWork>();

        _mockSubRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync())
            .ReturnsAsync(() => [NewSubscription()]);
        _mockSettingsRepo.Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync(_settings);
        _mockScrapedProjectRepo.Setup(r => r.GetByPortalIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([_matchingProject, _nonMatchingProject]);
        _mockProcessedRepo.Setup(r => r.GetProcessedUrlsAsync(UserId, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([]);
        _mockUow.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((action, ct) => action(ct));
        _mockJobAppRepo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>()))
            .Returns(Task.CompletedTask);
        _mockProcessedRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedProjectItem>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockSettingsRepo.Setup(r => r.UpdateAsync(It.IsAny<ProjectMonitoringSettings>()))
            .Returns(Task.CompletedTask);
    }

    private PollProjectsCommandHandler CreateHandler() => new(
        _mockSubRepo.Object,
        _mockSettingsRepo.Object,
        _mockJobAppRepo.Object,
        _mockProcessedRepo.Object,
        _mockScrapedProjectRepo.Object,
        _mockNotifier.Object,
        _mockUow.Object);

    [Fact]
    public async Task Handle_ShouldCreateJobApplication_ForMatchingProject()
    {
        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(
            j => j.Position == "Senior dotnet Developer" &&
                 j.Status == JobApplicationStatus.Discovered &&
                 j.UserId == UserId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCreateJobApplication_ForNonMatchingProject()
    {
        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(
            j => j.Position == "Marketing Manager")), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipAlreadyProcessedItems()
    {
        _mockProcessedRepo.Setup(r => r.GetProcessedUrlsAsync(UserId, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(["https://freelancermap.de/job/1", "https://freelancermap.de/job/2"]);

        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.IsAny<JobApplication>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSendNotification_WhenMatchesFound_AndIntervalElapsed()
    {
        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockNotifier.Verify(n => n.NotifyAsync(
            UserId,
            "user@example.com",
            It.Is<List<ScrapedProjectData>>(m => m.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotSendNotification_WhenIntervalNotElapsed()
    {
        _settings.LastNotifiedAt = DateTime.UtcNow.AddMinutes(-30); // only 30 min ago, interval is 60

        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockNotifier.Verify(n => n.NotifyAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<List<ScrapedProjectData>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSendNotification_WhenIntervalElapsed()
    {
        _settings.LastNotifiedAt = DateTime.UtcNow.AddMinutes(-61); // 61 min ago, interval is 60

        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockNotifier.Verify(n => n.NotifyAsync(
            UserId, "user@example.com",
            It.Is<List<ScrapedProjectData>>(m => m.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkipUser_WhenNoSettingsExist()
    {
        _mockSettingsRepo.Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync((ProjectMonitoringSettings?)null);

        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockScrapedProjectRepo.Verify(r => r.GetByPortalIdsAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipUser_WhenKeywordsAreEmpty()
    {
        _mockSettingsRepo.Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync(new ProjectMonitoringSettings { UserId = UserId, Keywords = [], NotificationIntervalMinutes = 60 });

        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockScrapedProjectRepo.Verify(r => r.GetByPortalIdsAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMarkItemsAsProcessed_AfterMatching()
    {
        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockProcessedRepo.Verify(r => r.AddRangeAsync(
            It.Is<IEnumerable<ProcessedProjectItem>>(items =>
                items.Any(i => i.ProjectItemUrl == "https://freelancermap.de/job/1" && i.UserId == UserId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseTransaction_ForPersistenceOperations()
    {
        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockUow.Verify(u => u.ExecuteInTransactionAsync(
            It.IsAny<Func<CancellationToken, Task>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetJobApplicationName_ToTitle_WhenCompanyNameEmpty()
    {
        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(
            j => j.Name == "Senior dotnet Developer")), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetJobApplicationName_ToCompanyName_WhenAvailable()
    {
        _matchingProject.CompanyName = "Acme GmbH";

        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(
            j => j.Name == "Acme GmbH")), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateLastNotifiedAt_AfterSendingNotification()
    {
        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockSettingsRepo.Verify(r => r.UpdateAsync(It.Is<ProjectMonitoringSettings>(
            s => s.LastNotifiedAt != null)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkipUser_WhenPollIntervalNotElapsed()
    {
        _settings.LastPolledAt = DateTime.UtcNow.AddMinutes(-30); // only 30 min ago, interval is 60

        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockScrapedProjectRepo.Verify(r => r.GetByPortalIdsAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldProcessUser_WhenPollIntervalElapsed()
    {
        _settings.LastPolledAt = DateTime.UtcNow.AddMinutes(-61); // 61 min ago, interval is 60

        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.IsAny<JobApplication>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateLastPolledAt_AfterProcessing()
    {
        await CreateHandler().Handle(new PollProjectsCommand(), CancellationToken.None);

        _mockSettingsRepo.Verify(r => r.UpdateAsync(It.Is<ProjectMonitoringSettings>(
            s => s.LastPolledAt != null)), Times.AtLeastOnce);
    }
}
