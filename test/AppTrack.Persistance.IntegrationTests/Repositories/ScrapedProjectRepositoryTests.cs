using AppTrack.Domain;
using AppTrack.Domain.Enums;
using AppTrack.Persistance.DatabaseContext;
using AppTrack.Persistance.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AppTrack.Persistance.IntegrationTests.Repositories;

/// <summary>
/// Persistence integration tests for <see cref="ScrapedProjectRepository.AddNewForPortalAsync"/>.
/// Each test receives a fresh InMemory database so tests are fully isolated.
/// </summary>
public class ScrapedProjectRepositoryTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static AppTrackDatabaseContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppTrackDatabaseContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new AppTrackDatabaseContext(options);
        context.Database.EnsureCreated();   // applies HasData seeds (ProjectPortal Id=1)
        return context;
    }

    /// <summary>
    /// The InMemory database used by each test is named after the test method,
    /// guaranteeing isolation without needing teardown logic.
    /// </summary>
    private static ScrapedProjectRepository CreateRepository(AppTrackDatabaseContext context)
        => new(context);

    private static ScrapedProject MakeProject(int portalId, string url, string description = "desc")
        => new()
        {
            ProjectPortalId = portalId,
            Title = "Test Project",
            Url = url,
            CompanyName = "Test Corp",
            Description = description
        };

    // -----------------------------------------------------------------------
    // AddNewForPortalAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AddNewForPortalAsync_ShouldInsertNewProjects_WhenNoneExistForPortal()
    {
        // Arrange — fresh database with no scraped projects
        await using var context = CreateContext(nameof(AddNewForPortalAsync_ShouldInsertNewProjects_WhenNoneExistForPortal));
        var repo = CreateRepository(context);

        var incoming = new[]
        {
            MakeProject(1, "https://freelancermap.de/projekte/alpha"),
            MakeProject(1, "https://freelancermap.de/projekte/beta")
        };

        // Act
        await repo.AddNewForPortalAsync(1, incoming, CancellationToken.None);

        // Assert
        var saved = await context.ScrapedProjects.AsNoTracking().ToListAsync();
        saved.Count.ShouldBe(2);
        saved.ShouldContain(p => p.Url == "https://freelancermap.de/projekte/alpha");
        saved.ShouldContain(p => p.Url == "https://freelancermap.de/projekte/beta");
    }

    [Fact]
    public async Task AddNewForPortalAsync_ShouldSkipProjects_WhoseUrlAlreadyExistsForPortal()
    {
        // Arrange — seed one existing project
        await using var context = CreateContext(nameof(AddNewForPortalAsync_ShouldSkipProjects_WhoseUrlAlreadyExistsForPortal));
        var repo = CreateRepository(context);

        context.ScrapedProjects.Add(MakeProject(1, "https://freelancermap.de/projekte/existing"));
        await context.SaveChangesAsync();

        var incoming = new[]
        {
            MakeProject(1, "https://freelancermap.de/projekte/existing"),   // duplicate
            MakeProject(1, "https://freelancermap.de/projekte/new-project") // novel
        };

        // Act
        await repo.AddNewForPortalAsync(1, incoming, CancellationToken.None);

        // Assert — only the novel project was added; total is 2 (seed + new)
        var saved = await context.ScrapedProjects.AsNoTracking().ToListAsync();
        saved.Count.ShouldBe(2);
        saved.Count(p => p.Url == "https://freelancermap.de/projekte/new-project").ShouldBe(1);
        saved.Count(p => p.Url == "https://freelancermap.de/projekte/existing").ShouldBe(1);
    }

    [Fact]
    public async Task AddNewForPortalAsync_ShouldDeduplicateUrls_CaseInsensitively()
    {
        // Arrange — seed with a lower-case URL
        await using var context = CreateContext(nameof(AddNewForPortalAsync_ShouldDeduplicateUrls_CaseInsensitively));
        var repo = CreateRepository(context);

        context.ScrapedProjects.Add(MakeProject(1, "https://freelancermap.de/projekte/item-one"));
        await context.SaveChangesAsync();

        // Incoming uses different casing for the same URL
        var incoming = new[]
        {
            MakeProject(1, "https://FREELANCERMAP.DE/Projekte/Item-One")
        };

        // Act
        await repo.AddNewForPortalAsync(1, incoming, CancellationToken.None);

        // Assert — the mixed-case incoming URL was treated as a duplicate; count stays at 1
        var saved = await context.ScrapedProjects.AsNoTracking().ToListAsync();
        saved.Count.ShouldBe(1);
    }

    [Fact]
    public async Task AddNewForPortalAsync_ShouldNotInsertAnything_WhenAllProjectsAreDuplicates()
    {
        // Arrange — seed two projects for portal 1
        await using var context = CreateContext(nameof(AddNewForPortalAsync_ShouldNotInsertAnything_WhenAllProjectsAreDuplicates));
        var repo = CreateRepository(context);

        context.ScrapedProjects.AddRange(
            MakeProject(1, "https://freelancermap.de/projekte/a"),
            MakeProject(1, "https://freelancermap.de/projekte/b")
        );
        await context.SaveChangesAsync();

        var incoming = new[]
        {
            MakeProject(1, "https://freelancermap.de/projekte/a"),
            MakeProject(1, "https://freelancermap.de/projekte/b")
        };

        // Act
        await repo.AddNewForPortalAsync(1, incoming, CancellationToken.None);

        // Assert — count is unchanged
        var saved = await context.ScrapedProjects.AsNoTracking().ToListAsync();
        saved.Count.ShouldBe(2);
    }

    [Fact]
    public async Task AddNewForPortalAsync_ShouldInsertProject_WhenSameUrlExistsForDifferentPortal()
    {
        // Arrange — seed a project for portal 1 with a URL
        await using var context = CreateContext(nameof(AddNewForPortalAsync_ShouldInsertProject_WhenSameUrlExistsForDifferentPortal));
        var repo = CreateRepository(context);

        // Add a second portal so FK constraint is satisfied
        context.ProjectPortals.Add(new ProjectPortal
        {
            Id = 2,
            Name = "OtherPortal",
            Url = "https://other-portal.de",
            ScraperType = ScraperType.FreelancerMap,
            IsActive = true
        });
        context.ScrapedProjects.Add(MakeProject(1, "https://freelancermap.de/projekte/shared-url"));
        await context.SaveChangesAsync();

        // Incoming targets portal 2 with the same URL — this is a DIFFERENT portal, so it is NOT a duplicate
        var incoming = new[]
        {
            MakeProject(2, "https://freelancermap.de/projekte/shared-url")
        };

        // Act
        await repo.AddNewForPortalAsync(2, incoming, CancellationToken.None);

        // Assert — both portal 1 and portal 2 rows exist; total = 2
        var saved = await context.ScrapedProjects.AsNoTracking().ToListAsync();
        saved.Count.ShouldBe(2);
        saved.Count(p => p.ProjectPortalId == 2).ShouldBe(1);
    }

    [Fact]
    public async Task AddNewForPortalAsync_ShouldPersistDescription_WhenProjectIsInserted()
    {
        // Arrange
        await using var context = CreateContext(nameof(AddNewForPortalAsync_ShouldPersistDescription_WhenProjectIsInserted));
        var repo = CreateRepository(context);

        const string expectedDescription = "Looking for a senior .NET developer to join our team.";
        var incoming = new[]
        {
            MakeProject(1, "https://freelancermap.de/projekte/desc-test", expectedDescription)
        };

        // Act
        await repo.AddNewForPortalAsync(1, incoming, CancellationToken.None);

        // Assert — Description is persisted correctly (new nvarchar(max) column)
        var saved = await context.ScrapedProjects.AsNoTracking().SingleAsync();
        saved.Description.ShouldBe(expectedDescription);
    }
}
