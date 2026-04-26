using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Commands.ScrapePortals;
using AppTrack.Application.Features.ProjectMonitoring.Models;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

public class ScrapePortalsCommandHandlerTests
{
    private readonly Mock<IProjectPortalRepository> _mockPortalRepo;
    private readonly Mock<IProjectScraperFactory> _mockScraperFactory;
    private readonly Mock<IScrapedProjectRepository> _mockScrapedProjectRepo;
    private readonly Mock<IProjectScraper> _mockScraper;

    public ScrapePortalsCommandHandlerTests()
    {
        _mockPortalRepo = new Mock<IProjectPortalRepository>();
        _mockScraperFactory = new Mock<IProjectScraperFactory>();
        _mockScrapedProjectRepo = new Mock<IScrapedProjectRepository>();
        _mockScraper = new Mock<IProjectScraper>();

        _mockScraperFactory
            .Setup(f => f.GetScraper(It.IsAny<ScraperType>()))
            .Returns(_mockScraper.Object);

        _mockScrapedProjectRepo
            .Setup(r => r.AddNewForPortalAsync(It.IsAny<int>(), It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private ScrapePortalsCommandHandler CreateHandler() =>
        new(_mockPortalRepo.Object, _mockScraperFactory.Object, _mockScrapedProjectRepo.Object);

    [Fact]
    public async Task Handle_ShouldMapDescriptionFromJobDescription()
    {
        var portal = new ProjectPortal { Id = 1, Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal]);
        _mockScraper.Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ScrapedProjectData("Dev", "https://freelancermap.de/projekte/dev", "Job description text", "Acme Corp", "Freelancermap")
            ]);

        IEnumerable<ScrapedProject>? capturedProjects = null;
        _mockScrapedProjectRepo
            .Setup(r => r.AddNewForPortalAsync(portal.Id, It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()))
            .Callback<int, IEnumerable<ScrapedProject>, CancellationToken>((_, projects, _) => capturedProjects = projects)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        capturedProjects.ShouldNotBeNull();
        var project = capturedProjects.Single();
        project.Description.ShouldBe("Job description text");
    }

    [Fact]
    public async Task Handle_ShouldMapAllFieldsCorrectly()
    {
        var portal = new ProjectPortal { Id = 5, Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal]);
        _mockScraper.Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ScrapedProjectData("Senior Dev", "https://freelancermap.de/projekte/senior-dev", "Great project", "Tech GmbH", "Freelancermap")
            ]);

        IEnumerable<ScrapedProject>? capturedProjects = null;
        _mockScrapedProjectRepo
            .Setup(r => r.AddNewForPortalAsync(portal.Id, It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()))
            .Callback<int, IEnumerable<ScrapedProject>, CancellationToken>((_, projects, _) => capturedProjects = projects)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        var project = capturedProjects!.Single();
        project.ProjectPortalId.ShouldBe(5);
        project.Title.ShouldBe("Senior Dev");
        project.Url.ShouldBe("https://freelancermap.de/projekte/senior-dev");
        project.CompanyName.ShouldBe("Tech GmbH");
        project.Description.ShouldBe("Great project");
    }

    [Fact]
    public async Task Handle_ShouldCallAddNewForPortalAsync_ForEachPortal()
    {
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([
            new ProjectPortal { Id = 1, Url = "https://portal1.de", ScraperType = ScraperType.FreelancerMap, IsActive = true },
            new ProjectPortal { Id = 2, Url = "https://portal2.de", ScraperType = ScraperType.FreelancerMap, IsActive = true }
        ]);
        _mockScraper.Setup(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        _mockScrapedProjectRepo.Verify(
            r => r.AddNewForPortalAsync(It.IsAny<int>(), It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldNotCallScraper_WhenNoPortalsActive()
    {
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([]);

        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        _mockScraper.Verify(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFilterOut_ItemsWithEmptyJobDescription()
    {
        // Arrange — one item has a real description, one has an empty string.
        // The handler uses string.IsNullOrEmpty, so only the truly-empty string is excluded.
        var portal = new ProjectPortal { Id = 1, Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal]);
        _mockScraper.Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ScrapedProjectData("With Description", "https://freelancermap.de/projekte/a", "Real description", "Acme", "Freelancermap"),
                new ScrapedProjectData("No Description",   "https://freelancermap.de/projekte/b", "",                "Beta", "Freelancermap")
            ]);

        IEnumerable<ScrapedProject>? capturedProjects = null;
        _mockScrapedProjectRepo
            .Setup(r => r.AddNewForPortalAsync(portal.Id, It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()))
            .Callback<int, IEnumerable<ScrapedProject>, CancellationToken>((_, projects, _) => capturedProjects = projects.ToList())
            .Returns(Task.CompletedTask);

        // Act
        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        // Assert — only the item with a non-empty description must reach the repository
        capturedProjects.ShouldNotBeNull();
        capturedProjects.ShouldHaveSingleItem();
        capturedProjects.Single().Title.ShouldBe("With Description");
    }

    [Fact]
    public async Task Handle_ShouldPassEmptyCollection_WhenAllItemsHaveEmptyJobDescription()
    {
        // Arrange
        var portal = new ProjectPortal { Id = 1, Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal]);
        _mockScraper.Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ScrapedProjectData("No Desc A", "https://freelancermap.de/projekte/a", "",  "Acme", "Freelancermap"),
                new ScrapedProjectData("No Desc B", "https://freelancermap.de/projekte/b", "",   "Beta", "Freelancermap")
            ]);

        IEnumerable<ScrapedProject>? capturedProjects = null;
        _mockScrapedProjectRepo
            .Setup(r => r.AddNewForPortalAsync(portal.Id, It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()))
            .Callback<int, IEnumerable<ScrapedProject>, CancellationToken>((_, projects, _) => capturedProjects = projects.ToList())
            .Returns(Task.CompletedTask);

        // Act
        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        // Assert — repository is still called (handler does not short-circuit), but with an empty sequence
        capturedProjects.ShouldNotBeNull();
        capturedProjects.ShouldBeEmpty();
    }
}
