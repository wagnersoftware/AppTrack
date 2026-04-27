using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Commands.MatchProjects;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

public class MatchProjectsCommandHandlerTests
{
    private readonly Mock<IUserPortalSubscriptionRepository> _subscriptionRepo = new();
    private readonly Mock<IProjectMonitoringSettingsRepository> _settingsRepo = new();
    private readonly Mock<IScrapedProjectRepository> _scrapedProjectRepo = new();
    private readonly Mock<IUserProjectMatchRepository> _matchRepo = new();
    private readonly Mock<IJobApplicationRepository> _jobAppRepo = new();
    private readonly Mock<IProcessedProjectItemRepository> _processedRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public MatchProjectsCommandHandlerTests()
    {
        _unitOfWork
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(async (fn, ct) => await fn(ct));
    }

    private MatchProjectsCommandHandler CreateHandler() => new(
        _subscriptionRepo.Object,
        _settingsRepo.Object,
        _scrapedProjectRepo.Object,
        _matchRepo.Object,
        _jobAppRepo.Object,
        _processedRepo.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task Handle_ShouldSkipUser_WhenNoSettings()
    {
        _subscriptionRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync())
            .ReturnsAsync(new List<UserPortalSubscription> { new UserPortalSubscription { UserId = "u1", ProjectPortalId = 1 } });
        _settingsRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync((ProjectMonitoringSettings?)null);

        await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

        _scrapedProjectRepo.Verify(r => r.GetUnprocessedForUserAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipUser_WhenNoKeywords()
    {
        _subscriptionRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync())
            .ReturnsAsync(new List<UserPortalSubscription> { new UserPortalSubscription { UserId = "u1", ProjectPortalId = 1 } });
        _settingsRepo.Setup(r => r.GetByUserIdAsync("u1"))
            .ReturnsAsync(new ProjectMonitoringSettings { UserId = "u1", Keywords = new List<string>() });

        await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

        _scrapedProjectRepo.Verify(r => r.GetUnprocessedForUserAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateMatchAndJobApplication_WhenKeywordMatches()
    {
        var project = new ScrapedProject { Id = 10, Title = "Senior .NET Developer", Url = "https://x.de/1", CompanyName = "Acme", Description = "Great project" };
        SetupSingleUserWithProject("u1", new List<string> { ".NET" }, project);

        List<UserProjectMatch>? capturedMatches = null;
        _matchRepo
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<UserProjectMatch>, CancellationToken>((m, _) => capturedMatches = m.ToList())
            .Returns(Task.CompletedTask);
        _jobAppRepo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>())).Returns(Task.CompletedTask);
        _processedRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedProjectItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

        capturedMatches.ShouldNotBeNull();
        capturedMatches.ShouldHaveSingleItem();
        capturedMatches[0].UserId.ShouldBe("u1");
        capturedMatches[0].ScrapedProjectId.ShouldBe(10);
        capturedMatches[0].IsNotified.ShouldBeFalse();

        _jobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(j =>
            j.UserId == "u1" &&
            j.Status == JobApplicationStatus.Discovered &&
            j.URL == "https://x.de/1" &&
            j.JobDescription == "Great project")), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateMatch_WhenKeywordOnlyInDescription()
    {
        var project = new ScrapedProject { Id = 12, Title = "Freelance Developer", Url = "https://x.de/3", CompanyName = "Acme", Description = "Experience with .NET required" };
        SetupSingleUserWithProject("u1", new List<string> { ".NET" }, project);
        _matchRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _jobAppRepo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>())).Returns(Task.CompletedTask);
        _processedRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedProjectItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

        _matchRepo.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>()), Times.Once);
        _jobAppRepo.Verify(r => r.CreateAsync(It.IsAny<JobApplication>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCreateMatch_WhenNoKeywordMatches()
    {
        var project = new ScrapedProject { Id = 11, Title = "Java Developer", Url = "https://x.de/2", CompanyName = "Acme", Description = "Spring Boot experience required" };
        SetupSingleUserWithProject("u1", new List<string> { ".NET" }, project);
        _processedRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedProjectItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

        _matchRepo.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>()), Times.Never);
        _jobAppRepo.Verify(r => r.CreateAsync(It.IsAny<JobApplication>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMarkAllNewProjectsAsProcessed_IncludingNonMatches()
    {
        var matchingProject  = new ScrapedProject { Id = 1, Title = ".NET Dev", Url = "https://x.de/1", CompanyName = "A" };
        var unmatchedProject = new ScrapedProject { Id = 2, Title = "Java Dev",  Url = "https://x.de/2", CompanyName = "B" };

        _subscriptionRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync())
            .ReturnsAsync(new List<UserPortalSubscription> { new UserPortalSubscription { UserId = "u1", ProjectPortalId = 1 } });
        _settingsRepo.Setup(r => r.GetByUserIdAsync("u1"))
            .ReturnsAsync(new ProjectMonitoringSettings { UserId = "u1", Keywords = new List<string> { ".NET" } });
        _scrapedProjectRepo.Setup(r => r.GetUnprocessedForUserAsync("u1", It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScrapedProject> { matchingProject, unmatchedProject });
        _matchRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _jobAppRepo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>())).Returns(Task.CompletedTask);

        List<ProcessedProjectItem>? capturedProcessed = null;
        _processedRepo
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedProjectItem>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<ProcessedProjectItem>, CancellationToken>((items, _) => capturedProcessed = items.ToList())
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

        capturedProcessed.ShouldNotBeNull();
        capturedProcessed.Count.ShouldBe(2);
        capturedProcessed.Select(p => p.ProjectItemUrl).ShouldContain("https://x.de/1");
        capturedProcessed.Select(p => p.ProjectItemUrl).ShouldContain("https://x.de/2");
    }

    private void SetupSingleUserWithProject(string userId, List<string> keywords, ScrapedProject project)
    {
        _subscriptionRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync())
            .ReturnsAsync(new List<UserPortalSubscription> { new UserPortalSubscription { UserId = userId, ProjectPortalId = 1 } });
        _settingsRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(new ProjectMonitoringSettings { UserId = userId, Keywords = keywords });
        _scrapedProjectRepo.Setup(r => r.GetUnprocessedForUserAsync(userId, It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScrapedProject> { project });
    }
}
