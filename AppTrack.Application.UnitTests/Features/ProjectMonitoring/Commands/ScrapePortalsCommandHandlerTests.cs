using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Commands.ScrapePortals;
using AppTrack.Application.Features.ProjectMonitoring.Models;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
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
            .Setup(r => r.GetExistingUrlsForPortalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlySet<string>)new HashSet<string>());

        _mockScrapedProjectRepo
            .Setup(r => r.AddNewForPortalAsync(It.IsAny<int>(), It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private ScrapePortalsCommandHandler CreateHandler() =>
        new(_mockPortalRepo.Object, _mockScraperFactory.Object, _mockScrapedProjectRepo.Object, NullLogger<ScrapePortalsCommandHandler>.Instance);

    [Fact]
    public async Task Handle_ShouldMapDescriptionFromJobDescription()
    {
        var portal = new ProjectPortal { Id = 1, Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal]);
        _mockScraper.Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScrapingResult.Success([
                new ScrapedProjectData("Dev", "https://freelancermap.de/projekte/dev", "Job description text", "Acme Corp", "Freelancermap")
            ], 1));

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
        _mockScraper.Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScrapingResult.Success([
                new ScrapedProjectData("Senior Dev", "https://freelancermap.de/projekte/senior-dev", "Great project", "Tech GmbH", "Freelancermap")
            ], 1));

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
        _mockScraper.Setup(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScrapingResult.Success([], 0));

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

        _mockScraper.Verify(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenScrapingFails_ShouldSkipPortalAndNotCallAddNew()
    {
        var portal = new ProjectPortal { Id = 1, Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal]);
        _mockScraper.Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScrapingResult.Failure("Connection refused"));

        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        _mockScrapedProjectRepo.Verify(
            r => r.AddNewForPortalAsync(It.IsAny<int>(), It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFilterOut_ItemsWithEmptyJobDescription()
    {
        var portal = new ProjectPortal { Id = 1, Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal]);
        _mockScraper.Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScrapingResult.Success([
                new ScrapedProjectData("With Description", "https://freelancermap.de/projekte/a", "Real description", "Acme", "Freelancermap"),
                new ScrapedProjectData("No Description",   "https://freelancermap.de/projekte/b", "",                "Beta", "Freelancermap")
            ], 2));

        IEnumerable<ScrapedProject>? capturedProjects = null;
        _mockScrapedProjectRepo
            .Setup(r => r.AddNewForPortalAsync(portal.Id, It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()))
            .Callback<int, IEnumerable<ScrapedProject>, CancellationToken>((_, projects, _) => capturedProjects = projects.ToList())
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        capturedProjects.ShouldNotBeNull();
        capturedProjects.ShouldHaveSingleItem();
        capturedProjects.Single().Title.ShouldBe("With Description");
    }

    [Fact]
    public async Task Handle_ShouldPassEmptyCollection_WhenAllItemsHaveEmptyJobDescription()
    {
        var portal = new ProjectPortal { Id = 1, Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal]);
        _mockScraper.Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScrapingResult.Success([
                new ScrapedProjectData("No Desc A", "https://freelancermap.de/projekte/a", "", "Acme", "Freelancermap"),
                new ScrapedProjectData("No Desc B", "https://freelancermap.de/projekte/b", "", "Beta", "Freelancermap")
            ], 2));

        IEnumerable<ScrapedProject>? capturedProjects = null;
        _mockScrapedProjectRepo
            .Setup(r => r.AddNewForPortalAsync(portal.Id, It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()))
            .Callback<int, IEnumerable<ScrapedProject>, CancellationToken>((_, projects, _) => capturedProjects = projects.ToList())
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        // Repository is still called (handler does not short-circuit), but with an empty sequence
        capturedProjects.ShouldNotBeNull();
        capturedProjects.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("", "https://x.de/p", "desc", "Acme")]           // empty title
    [InlineData("Dev", "", "desc", "Acme")]                       // empty URL
    [InlineData("Dev", "https://x.de/p", "desc", "")]            // empty company
    public async Task Handle_ShouldFilterOut_ItemsWithMissingRequiredFields(
        string title, string url, string description, string company)
    {
        var portal = new ProjectPortal { Id = 1, Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal]);
        _mockScraper.Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScrapingResult.Success([new ScrapedProjectData(title, url, description, company, "Freelancermap")], 1));

        IEnumerable<ScrapedProject>? capturedProjects = null;
        _mockScrapedProjectRepo
            .Setup(r => r.AddNewForPortalAsync(portal.Id, It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()))
            .Callback<int, IEnumerable<ScrapedProject>, CancellationToken>((_, projects, _) => capturedProjects = projects.ToList())
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        capturedProjects.ShouldNotBeNull();
        capturedProjects.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(501, 100, 100)]   // title too long
    [InlineData(100, 2001, 100)]  // URL too long
    [InlineData(100, 100, 301)]   // company too long
    public async Task Handle_ShouldFilterOut_ItemsExceedingColumnLengths(
        int titleLength, int urlLength, int companyLength)
    {
        var portal = new ProjectPortal { Id = 1, Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal]);
        _mockScraper.Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScrapingResult.Success([new ScrapedProjectData(
                new string('A', titleLength),
                "https://x.de/" + new string('p', urlLength),
                "valid description",
                new string('C', companyLength),
                "Freelancermap")], 1));

        IEnumerable<ScrapedProject>? capturedProjects = null;
        _mockScrapedProjectRepo
            .Setup(r => r.AddNewForPortalAsync(portal.Id, It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()))
            .Callback<int, IEnumerable<ScrapedProject>, CancellationToken>((_, projects, _) => capturedProjects = projects.ToList())
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        capturedProjects.ShouldNotBeNull();
        capturedProjects.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFirstPortalFails_ShouldStillProcessSecondPortal()
    {
        var portal1 = new ProjectPortal { Id = 1, Url = "https://portal1.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        var portal2 = new ProjectPortal { Id = 2, Url = "https://portal2.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal1, portal2]);

        _mockScraper
            .Setup(s => s.ScrapeAsync(portal1.Url, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScrapingResult.Failure("Connection refused"));

        _mockScraper
            .Setup(s => s.ScrapeAsync(portal2.Url, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScrapingResult.Success([], 0));

        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        // portal2 should still have AddNewForPortalAsync called even though portal1 failed
        _mockScrapedProjectRepo.Verify(
            r => r.AddNewForPortalAsync(portal2.Id, It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // portal1 must NOT have AddNewForPortalAsync called
        _mockScrapedProjectRepo.Verify(
            r => r.AddNewForPortalAsync(portal1.Id, It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnit()
    {
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([]);

        var result = await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        result.ShouldBe(AppTrack.Application.Shared.Unit.Value);
    }

    [Fact]
    public async Task Handle_ShouldMapDetailFieldsFromScrapedProjectData()
    {
        var portal = new ProjectPortal { Id = 1, Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal]);
        _mockScraper.Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScrapingResult.Success([
                new ScrapedProjectData(
                    "Senior Dev", "https://freelancermap.de/projekte/1", "Great project", "Tech GmbH", "Freelancermap",
                    Location: "Berlin",
                    DurationInMonths: "6",
                    StartDateText: "ab sofort",
                    ContactPerson: "Max Mustermann")
            ], 1));

        IEnumerable<ScrapedProject>? capturedProjects = null;
        _mockScrapedProjectRepo
            .Setup(r => r.AddNewForPortalAsync(portal.Id, It.IsAny<IEnumerable<ScrapedProject>>(), It.IsAny<CancellationToken>()))
            .Callback<int, IEnumerable<ScrapedProject>, CancellationToken>((_, projects, _) => capturedProjects = projects.ToList())
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        var project = capturedProjects!.Single();
        project.Location.ShouldBe("Berlin");
        project.DurationInMonths.ShouldBe("6");
        project.StartDateText.ShouldBe("ab sofort");
        project.ContactPerson.ShouldBe("Max Mustermann");
    }

    [Fact]
    public async Task Handle_ShouldPassKnownUrlsFromRepository_ToScraper()
    {
        var portal = new ProjectPortal { Id = 7, Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true };
        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([portal]);

        var knownUrls = (IReadOnlySet<string>)new HashSet<string> { "https://freelancermap.de/projekte/existing" };
        _mockScrapedProjectRepo
            .Setup(r => r.GetExistingUrlsForPortalAsync(portal.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(knownUrls);

        IReadOnlySet<string>? capturedKnownUrls = null;
        _mockScraper
            .Setup(s => s.ScrapeAsync(portal.Url, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .Callback<string, IReadOnlySet<string>, CancellationToken>((_, urls, _) => capturedKnownUrls = urls)
            .ReturnsAsync(ScrapingResult.Success([], 0));

        await CreateHandler().Handle(new ScrapePortalsCommand(), CancellationToken.None);

        capturedKnownUrls.ShouldNotBeNull();
        capturedKnownUrls.ShouldContain("https://freelancermap.de/projekte/existing");
    }
}
