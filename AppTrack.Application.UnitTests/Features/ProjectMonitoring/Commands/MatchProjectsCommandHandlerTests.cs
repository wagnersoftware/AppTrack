using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Commands.MatchProjects;
using AppTrack.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

public class MatchProjectsCommandHandlerTests
{
    private readonly Mock<IProjectMonitoringSettingsRepository> _mockSettingsRepository;
    private readonly Mock<IProcessedProjectItemRepository> _mockProcessedRepository;
    private readonly Mock<IScrapedProjectRepository> _mockScrapedRepository;
    private readonly Mock<IUserProjectMatchRepository> _mockMatchRepository;

    public MatchProjectsCommandHandlerTests()
    {
        _mockSettingsRepository = new Mock<IProjectMonitoringSettingsRepository>();
        _mockProcessedRepository = new Mock<IProcessedProjectItemRepository>();
        _mockScrapedRepository = new Mock<IScrapedProjectRepository>();
        _mockMatchRepository = new Mock<IUserProjectMatchRepository>();
    }

    private MatchProjectsCommandHandler CreateHandler() =>
        new(
            _mockSettingsRepository.Object,
            _mockProcessedRepository.Object,
            _mockScrapedRepository.Object,
            _mockMatchRepository.Object,
            NullLogger<MatchProjectsCommandHandler>.Instance);

    [Fact]
    public async Task Handle_NoSettingsFound_ShouldReturnUnit()
    {
        var userId = "user-123";
        var command = new MatchProjectsCommand { UserId = userId };

        _mockSettingsRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((AppTrack.Domain.ProjectMonitoringSettings?)null);

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockSettingsRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
        _mockProcessedRepository.Verify(r => r.GetProcessedUrlsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoKeywords_ShouldReturnUnit()
    {
        var userId = "user-123";
        var command = new MatchProjectsCommand { UserId = userId };
        var settings = new AppTrack.Domain.ProjectMonitoringSettings
        {
            Id = 1,
            UserId = userId,
            Keywords = new List<string>(),
            NotifyByEmail = true,
            NotificationEmail = "test@example.com"
        };

        _mockSettingsRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(settings);

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockProcessedRepository.Verify(r => r.GetProcessedUrlsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        _mockScrapedRepository.Verify(r => r.GetUnprocessedForUserAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProjectMatches_ShouldCreateMatch()
    {
        var userId = "user-123";
        var command = new MatchProjectsCommand { UserId = userId };
        var settings = new AppTrack.Domain.ProjectMonitoringSettings
        {
            Id = 1,
            UserId = userId,
            Keywords = new List<string> { "csharp", "dotnet" },
            NotifyByEmail = true,
            NotificationEmail = "test@example.com"
        };

        var project = new ScrapedProject
        {
            Id = 1,
            ProjectPortalId = 1,
            Title = "C# Developer",
            Url = "https://example.com/project-1",
            CompanyName = "Tech Corp",
            Description = "We are looking for a .NET developer"
        };

        _mockSettingsRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(settings);

        _mockProcessedRepository
            .Setup(r => r.GetProcessedUrlsAsync(userId, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new HashSet<string>());

        _mockScrapedRepository
            .Setup(r => r.GetUnprocessedForUserAsync(userId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScrapedProject> { project });

        _mockMatchRepository
            .Setup(r => r.GetByUserAndProjectAsync(userId, project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProjectMatch?)null);

        _mockMatchRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserProjectMatch>()))
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockMatchRepository.Verify(
            r => r.CreateAsync(It.Is<UserProjectMatch>(m => m.UserId == userId && m.ScrapedProjectId == project.Id && !m.IsNotified)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ProjectDoesNotMatch_ShouldNotCreateMatch()
    {
        var userId = "user-123";
        var command = new MatchProjectsCommand { UserId = userId };
        var settings = new AppTrack.Domain.ProjectMonitoringSettings
        {
            Id = 1,
            UserId = userId,
            Keywords = new List<string> { "java", "python" },
            NotifyByEmail = true,
            NotificationEmail = "test@example.com"
        };

        var project = new ScrapedProject
        {
            Id = 1,
            ProjectPortalId = 1,
            Title = "C# Developer",
            Url = "https://example.com/project-1",
            CompanyName = "Tech Corp",
            Description = "We are looking for a .NET developer"
        };

        _mockSettingsRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(settings);

        _mockProcessedRepository
            .Setup(r => r.GetProcessedUrlsAsync(userId, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new HashSet<string>());

        _mockScrapedRepository
            .Setup(r => r.GetUnprocessedForUserAsync(userId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScrapedProject> { project });

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockMatchRepository.Verify(
            r => r.CreateAsync(It.IsAny<UserProjectMatch>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_AllProcessedProjects_ShouldNotCreateMatch()
    {
        var userId = "user-123";
        var command = new MatchProjectsCommand { UserId = userId };
        var settings = new AppTrack.Domain.ProjectMonitoringSettings
        {
            Id = 1,
            UserId = userId,
            Keywords = new List<string> { "csharp" },
            NotifyByEmail = true,
            NotificationEmail = "test@example.com"
        };

        _mockSettingsRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(settings);

        _mockProcessedRepository
            .Setup(r => r.GetProcessedUrlsAsync(userId, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new HashSet<string> { "https://example.com/project-1" });

        _mockScrapedRepository
            .Setup(r => r.GetUnprocessedForUserAsync(userId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScrapedProject>());

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockMatchRepository.Verify(
            r => r.CreateAsync(It.IsAny<UserProjectMatch>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_KeywordMatching_ShouldBeCaseInsensitive()
    {
        var userId = "user-123";
        var command = new MatchProjectsCommand { UserId = userId };
        var settings = new AppTrack.Domain.ProjectMonitoringSettings
        {
            Id = 1,
            UserId = userId,
            Keywords = new List<string> { "CSHARP" },
            NotifyByEmail = true,
            NotificationEmail = "test@example.com"
        };

        var project = new ScrapedProject
        {
            Id = 1,
            ProjectPortalId = 1,
            Title = "c# developer",
            Url = "https://example.com/project-1",
            CompanyName = "Tech Corp",
            Description = "We need a csharp expert"
        };

        _mockSettingsRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(settings);

        _mockProcessedRepository
            .Setup(r => r.GetProcessedUrlsAsync(userId, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new HashSet<string>());

        _mockScrapedRepository
            .Setup(r => r.GetUnprocessedForUserAsync(userId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScrapedProject> { project });

        _mockMatchRepository
            .Setup(r => r.GetByUserAndProjectAsync(userId, project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProjectMatch?)null);

        _mockMatchRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserProjectMatch>()))
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockMatchRepository.Verify(
            r => r.CreateAsync(It.Is<UserProjectMatch>(m => m.UserId == userId && m.ScrapedProjectId == project.Id)),
            Times.Once);
    }
}
