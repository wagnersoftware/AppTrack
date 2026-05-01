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

    [Fact]
    public async Task Handle_ShouldFallBackToProjectTitle_WhenCompanyNameIsEmpty()
    {
        // Arrange — project with empty CompanyName; Name should default to Title
        var project = new ScrapedProject { Id = 20, Title = "Senior .NET Engineer", Url = "https://x.de/20", CompanyName = "", Description = ".NET required" };
        SetupSingleUserWithProject("u1", new List<string> { ".NET" }, project);

        List<JobApplication>? capturedApps = null;
        _jobAppRepo
            .Setup(r => r.CreateAsync(It.IsAny<JobApplication>()))
            .Callback<JobApplication>(app =>
            {
                capturedApps ??= new List<JobApplication>();
                capturedApps.Add(app);
            })
            .Returns(Task.CompletedTask);
        _matchRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _processedRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedProjectItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

        // Assert — Name must equal the Title when CompanyName is empty
        capturedApps.ShouldNotBeNull();
        capturedApps!.ShouldHaveSingleItem();
        capturedApps[0].Name.ShouldBe("Senior .NET Engineer");
    }

    [Fact]
    public async Task Handle_ShouldProcessEachUserIndependently_WhenMultipleUsersHaveMatches()
    {
        // Arrange — two users, each subscribed to different portals, each with a matching project
        var projectU1 = new ScrapedProject { Id = 30, Title = ".NET Developer", Url = "https://x.de/30", CompanyName = "Acme", Description = "desc" };
        var projectU2 = new ScrapedProject { Id = 31, Title = "C# Engineer .NET", Url = "https://x.de/31", CompanyName = "Beta Corp", Description = "desc" };

        _subscriptionRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync())
            .ReturnsAsync(new List<UserPortalSubscription>
            {
                new UserPortalSubscription { UserId = "u1", ProjectPortalId = 1 },
                new UserPortalSubscription { UserId = "u2", ProjectPortalId = 2 }
            });

        _settingsRepo.Setup(r => r.GetByUserIdAsync("u1"))
            .ReturnsAsync(new ProjectMonitoringSettings { UserId = "u1", Keywords = new List<string> { ".NET" } });
        _settingsRepo.Setup(r => r.GetByUserIdAsync("u2"))
            .ReturnsAsync(new ProjectMonitoringSettings { UserId = "u2", Keywords = new List<string> { ".NET" } });

        _scrapedProjectRepo.Setup(r => r.GetUnprocessedForUserAsync("u1", It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScrapedProject> { projectU1 });
        _scrapedProjectRepo.Setup(r => r.GetUnprocessedForUserAsync("u2", It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScrapedProject> { projectU2 });

        _matchRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _jobAppRepo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>())).Returns(Task.CompletedTask);
        _processedRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedProjectItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

        // Assert — each user gets their own job application created
        _jobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(j => j.UserId == "u1" && j.URL == "https://x.de/30")), Times.Once);
        _jobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(j => j.UserId == "u2" && j.URL == "https://x.de/31")), Times.Once);
        _jobAppRepo.Verify(r => r.CreateAsync(It.IsAny<JobApplication>()), Times.Exactly(2));
    }

    // -----------------------------------------------------------------------
    // Detail field mapping
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldMapLocationFromScrapedProject()
    {
        var project = new ScrapedProject { Id = 50, Title = ".NET Dev", Url = "https://x.de/50", CompanyName = "A", Description = ".NET", Location = "Berlin" };
        SetupSingleUserWithProject("u1", [".NET"], project);

        var app = await CaptureCreatedJobApplication();

        app.Location.ShouldBe("Berlin");
    }

    [Fact]
    public async Task Handle_ShouldFallbackToUnknown_WhenLocationIsEmpty()
    {
        var project = new ScrapedProject { Id = 51, Title = ".NET Dev", Url = "https://x.de/51", CompanyName = "A", Description = ".NET", Location = "" };
        SetupSingleUserWithProject("u1", [".NET"], project);

        var app = await CaptureCreatedJobApplication();

        app.Location.ShouldBe("Unknown");
    }

    [Fact]
    public async Task Handle_ShouldMapContactPersonFromScrapedProject()
    {
        var project = new ScrapedProject { Id = 52, Title = ".NET Dev", Url = "https://x.de/52", CompanyName = "A", Description = ".NET", ContactPerson = "Max Mustermann" };
        SetupSingleUserWithProject("u1", [".NET"], project);

        var app = await CaptureCreatedJobApplication();

        app.ContactPerson.ShouldBe("Max Mustermann");
    }

    [Fact]
    public async Task Handle_ShouldFallbackToUnknown_WhenContactPersonIsEmpty()
    {
        var project = new ScrapedProject { Id = 53, Title = ".NET Dev", Url = "https://x.de/53", CompanyName = "A", Description = ".NET", ContactPerson = "" };
        SetupSingleUserWithProject("u1", [".NET"], project);

        var app = await CaptureCreatedJobApplication();

        app.ContactPerson.ShouldBe("Unknown");
    }

    [Fact]
    public async Task Handle_ShouldMapDurationInMonthsFromScrapedProject()
    {
        var project = new ScrapedProject { Id = 54, Title = ".NET Dev", Url = "https://x.de/54", CompanyName = "A", Description = ".NET", DurationInMonths = "6" };
        SetupSingleUserWithProject("u1", [".NET"], project);

        var app = await CaptureCreatedJobApplication();

        app.DurationInMonths.ShouldBe("6");
    }

    [Fact]
    public async Task Handle_ShouldParseStartDate_WhenValidGermanDate()
    {
        var project = new ScrapedProject { Id = 55, Title = ".NET Dev", Url = "https://x.de/55", CompanyName = "A", Description = ".NET", StartDateText = "01.06.2026" };
        SetupSingleUserWithProject("u1", [".NET"], project);

        var app = await CaptureCreatedJobApplication();

        app.StartDate.ShouldBe(new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task Handle_ShouldFallbackToToday_WhenStartDateTextIsUnparseable()
    {
        var project = new ScrapedProject { Id = 56, Title = ".NET Dev", Url = "https://x.de/56", CompanyName = "A", Description = ".NET", StartDateText = "ab sofort" };
        SetupSingleUserWithProject("u1", [".NET"], project);

        var before = DateTime.UtcNow.Date;
        var app = await CaptureCreatedJobApplication();
        var after = DateTime.UtcNow.Date;

        app.StartDate.ShouldBeInRange(before, after);
    }

    [Fact]
    public async Task Handle_ShouldFallbackToToday_WhenStartDateTextIsEmpty()
    {
        var project = new ScrapedProject { Id = 57, Title = ".NET Dev", Url = "https://x.de/57", CompanyName = "A", Description = ".NET", StartDateText = "" };
        SetupSingleUserWithProject("u1", [".NET"], project);

        var before = DateTime.UtcNow.Date;
        var app = await CaptureCreatedJobApplication();
        var after = DateTime.UtcNow.Date;

        app.StartDate.ShouldBeInRange(before, after);
    }

    private async Task<JobApplication> CaptureCreatedJobApplication()
    {
        JobApplication? captured = null;
        _jobAppRepo
            .Setup(r => r.CreateAsync(It.IsAny<JobApplication>()))
            .Callback<JobApplication>(a => captured = a)
            .Returns(Task.CompletedTask);
        _matchRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _processedRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedProjectItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

        captured.ShouldNotBeNull();
        return captured!;
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
