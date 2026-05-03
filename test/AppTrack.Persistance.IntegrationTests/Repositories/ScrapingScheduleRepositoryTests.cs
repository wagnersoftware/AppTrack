using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using AppTrack.Persistance.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AppTrack.Persistance.IntegrationTests.Repositories;

/// <summary>
/// Persistence integration tests for <see cref="ScrapingScheduleRepository"/>.
/// Each test uses a unique InMemory database so tests are fully isolated.
/// </summary>
public class ScrapingScheduleRepositoryTests
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
        context.Database.EnsureCreated();
        return context;
    }

    private static ScrapingScheduleRepository CreateRepository(AppTrackDatabaseContext context)
        => new(context);

    // -----------------------------------------------------------------------
    // GetNextRunAfterAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetNextRunAfterAsync_WhenNoStateRowExists_ReturnsNull()
    {
        // Arrange — fresh database with no ScrapingScheduleState row
        await using var context = CreateContext(nameof(GetNextRunAfterAsync_WhenNoStateRowExists_ReturnsNull));
        var repo = CreateRepository(context);

        // Act
        var result = await repo.GetNextRunAfterAsync(CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetNextRunAfterAsync_WhenStateRowExists_ReturnsStoredValue()
    {
        // Arrange — seed the singleton row
        await using var context = CreateContext(nameof(GetNextRunAfterAsync_WhenStateRowExists_ReturnsStoredValue));
        var expectedTime = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc);
        context.ScrapingScheduleStates.Add(new ScrapingScheduleState
        {
            Id = 1,
            NextRunAfterUtc = expectedTime
        });
        await context.SaveChangesAsync();

        var repo = CreateRepository(context);

        // Act
        var result = await repo.GetNextRunAfterAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(expectedTime);
    }

    // -----------------------------------------------------------------------
    // SetNextRunAfterAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SetNextRunAfterAsync_WhenNoStateRowExists_InsertsRow()
    {
        // Arrange — fresh database, no state row
        await using var context = CreateContext(nameof(SetNextRunAfterAsync_WhenNoStateRowExists_InsertsRow));
        var repo = CreateRepository(context);
        var nextRun = new DateTime(2026, 5, 2, 9, 30, 0, DateTimeKind.Utc);

        // Act
        await repo.SetNextRunAfterAsync(nextRun, CancellationToken.None);

        // Assert — exactly one row was inserted with the correct value
        var saved = await context.ScrapingScheduleStates.AsNoTracking().SingleAsync();
        saved.NextRunAfterUtc.ShouldBe(nextRun);
    }

    [Fact]
    public async Task SetNextRunAfterAsync_WhenStateRowAlreadyExists_UpdatesExistingRow()
    {
        // Arrange — seed the singleton row with an old value
        await using var context = CreateContext(nameof(SetNextRunAfterAsync_WhenStateRowAlreadyExists_UpdatesExistingRow));
        var original = new DateTime(2026, 4, 30, 10, 0, 0, DateTimeKind.Utc);
        context.ScrapingScheduleStates.Add(new ScrapingScheduleState
        {
            Id = 1,
            NextRunAfterUtc = original
        });
        await context.SaveChangesAsync();

        var repo = CreateRepository(context);
        var updated = new DateTime(2026, 5, 1, 11, 0, 0, DateTimeKind.Utc);

        // Act
        await repo.SetNextRunAfterAsync(updated, CancellationToken.None);

        // Assert — still exactly one row, with the updated value
        var rows = await context.ScrapingScheduleStates.AsNoTracking().ToListAsync();
        rows.Count.ShouldBe(1);
        rows[0].NextRunAfterUtc.ShouldBe(updated);
    }

    [Fact]
    public async Task SetNextRunAfterAsync_ThenGetNextRunAfterAsync_RoundTripsValue()
    {
        // Arrange
        await using var context = CreateContext(nameof(SetNextRunAfterAsync_ThenGetNextRunAfterAsync_RoundTripsValue));
        var repo = CreateRepository(context);
        var expectedTime = new DateTime(2026, 5, 3, 14, 15, 0, DateTimeKind.Utc);

        // Act — write then immediately read back
        await repo.SetNextRunAfterAsync(expectedTime, CancellationToken.None);
        var result = await repo.GetNextRunAfterAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(expectedTime);
    }
}
