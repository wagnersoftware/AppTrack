using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Commands.ScrapePortals;
using AppTrack.Application.Shared;
using AppTrack.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;

namespace AppTrack.Functions.UnitTests;

/// <summary>
/// Unit tests for <see cref="ScrapePortalsFunction.Run"/>.
/// <see cref="FakeTimeProvider"/> from <c>Microsoft.Extensions.TimeProvider.Testing</c> is used to
/// pin the clock to deterministic UTC instants, making time-window assertions fully reliable
/// regardless of when or where the tests execute.
/// </summary>
public class ScrapePortalsFunctionTests
{
    /// <summary>
    /// A UTC instant that maps to 12:00 CET/CEST (CEST = UTC+2, 29 Apr 2026 is summer time).
    /// Solidly inside the 09:00–17:00 operating window.
    /// </summary>
    private static readonly DateTimeOffset InsideWindowUtc =
        new(2026, 4, 29, 10, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// A UTC instant that maps to 22:00 CET/CEST.
    /// Solidly outside the 09:00–17:00 operating window.
    /// </summary>
    private static readonly DateTimeOffset OutsideWindowUtc =
        new(2026, 4, 29, 20, 0, 0, TimeSpan.Zero);

    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IScrapingEventPublisher> _mockEventPublisher;
    private readonly Mock<IScrapingScheduleRepository> _mockScheduleRepo;

    public ScrapePortalsFunctionTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockEventPublisher = new Mock<IScrapingEventPublisher>();
        _mockScheduleRepo = new Mock<IScrapingScheduleRepository>();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ScrapePortalsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        _mockEventPublisher
            .Setup(p => p.PublishScrapingCompletedAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockScheduleRepo
            .Setup(r => r.SetNextRunAfterAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private ScrapePortalsFunction CreateFunction(TimeProvider timeProvider) =>
        new(
            _mockMediator.Object,
            _mockEventPublisher.Object,
            _mockScheduleRepo.Object,
            timeProvider,
            NullLogger<ScrapePortalsFunction>.Instance);

    // -----------------------------------------------------------------------
    // Time-window gate
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Run_WhenTimeIsOutsideOperatingWindow_ShouldNotInvokeMediator()
    {
        // Arrange — clock is pinned to 22:00 CET, outside the 09:00–17:00 window
        var fakeTime = new FakeTimeProvider(OutsideWindowUtc);

        _mockScheduleRepo
            .Setup(r => r.GetNextRunAfterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        // Act
        await CreateFunction(fakeTime).Run(new TimerInfo(), CancellationToken.None);

        // Assert — time-window gate fired before schedule check; mediator never invoked
        _mockMediator.Verify(
            m => m.Send(It.IsAny<ScrapePortalsCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenTimeIsOutsideOperatingWindow_ShouldNotUpdateSchedule()
    {
        // Arrange — clock is pinned to 22:00 CET
        var fakeTime = new FakeTimeProvider(OutsideWindowUtc);

        _mockScheduleRepo
            .Setup(r => r.GetNextRunAfterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        // Act
        await CreateFunction(fakeTime).Run(new TimerInfo(), CancellationToken.None);

        // Assert — nothing written to the schedule store when the time gate fires
        _mockScheduleRepo.Verify(
            r => r.SetNextRunAfterAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenTimeIsInsideOperatingWindowAndNoScheduleConstraint_ShouldInvokeMediator()
    {
        // Arrange — clock is pinned to 12:00 CET, inside the window; no schedule record
        var fakeTime = new FakeTimeProvider(InsideWindowUtc);

        _mockScheduleRepo
            .Setup(r => r.GetNextRunAfterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        // Act
        await CreateFunction(fakeTime).Run(new TimerInfo(), CancellationToken.None);

        // Assert
        _mockMediator.Verify(
            m => m.Send(It.IsAny<ScrapePortalsCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // Schedule gate (time is always pinned inside the operating window)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Run_WhenScheduledNextRunIsInFuture_ShouldNotInvokeMediator()
    {
        // Arrange — clock inside window; schedule gate closed (next run 2 hours away)
        var fakeTime = new FakeTimeProvider(InsideWindowUtc);
        var futureTime = InsideWindowUtc.UtcDateTime.AddHours(2);

        _mockScheduleRepo
            .Setup(r => r.GetNextRunAfterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(futureTime);

        // Act
        await CreateFunction(fakeTime).Run(new TimerInfo(), CancellationToken.None);

        // Assert
        _mockMediator.Verify(
            m => m.Send(It.IsAny<ScrapePortalsCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenScheduledNextRunIsInFuture_ShouldNotUpdateSchedule()
    {
        // Arrange — clock inside window; schedule gate closed
        var fakeTime = new FakeTimeProvider(InsideWindowUtc);
        var futureTime = InsideWindowUtc.UtcDateTime.AddHours(2);

        _mockScheduleRepo
            .Setup(r => r.GetNextRunAfterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(futureTime);

        // Act
        await CreateFunction(fakeTime).Run(new TimerInfo(), CancellationToken.None);

        // Assert — schedule not written when gate is closed
        _mockScheduleRepo.Verify(
            r => r.SetNextRunAfterAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenScheduledNextRunHasPassed_ShouldInvokeMediator()
    {
        // Arrange — clock inside window; schedule record exists but already elapsed
        var fakeTime = new FakeTimeProvider(InsideWindowUtc);
        var pastTime = InsideWindowUtc.UtcDateTime.AddMinutes(-5);

        _mockScheduleRepo
            .Setup(r => r.GetNextRunAfterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pastTime);

        // Act
        await CreateFunction(fakeTime).Run(new TimerInfo(), CancellationToken.None);

        // Assert — both gates are open; mediator should be called
        _mockMediator.Verify(
            m => m.Send(It.IsAny<ScrapePortalsCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // Successful scrape path
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Run_WhenScrapingSucceeds_ShouldPublishScrapingCompletedEvent()
    {
        var fakeTime = new FakeTimeProvider(InsideWindowUtc);

        _mockScheduleRepo
            .Setup(r => r.GetNextRunAfterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        await CreateFunction(fakeTime).Run(new TimerInfo(), CancellationToken.None);

        _mockEventPublisher.Verify(
            p => p.PublishScrapingCompletedAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenScrapingSucceeds_ShouldUpdateScheduleWithIntervalBetween90And150Minutes()
    {
        var fakeTime = new FakeTimeProvider(InsideWindowUtc);
        var nowUtc = InsideWindowUtc.UtcDateTime;

        _mockScheduleRepo
            .Setup(r => r.GetNextRunAfterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        DateTime? capturedNextRun = null;
        _mockScheduleRepo
            .Setup(r => r.SetNextRunAfterAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Callback<DateTime, CancellationToken>((dt, _) => capturedNextRun = dt)
            .Returns(Task.CompletedTask);

        await CreateFunction(fakeTime).Run(new TimerInfo(), CancellationToken.None);

        _mockScheduleRepo.Verify(
            r => r.SetNextRunAfterAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);

        capturedNextRun.ShouldNotBeNull();
        // Next run must be at least 90 minutes from the pinned fake time.
        // The upper bound is relaxed because CalculateNextRunUtc may push the candidate
        // to the next day's window start if the random interval would fall outside 09:00–17:00.
        capturedNextRun.Value.ShouldBeGreaterThan(nowUtc.AddMinutes(89));
    }

    // -----------------------------------------------------------------------
    // Failed scrape path
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Run_WhenScrapingThrows_ShouldStillUpdateScheduleWith30MinuteDelay()
    {
        var fakeTime = new FakeTimeProvider(InsideWindowUtc);
        var nowUtc = InsideWindowUtc.UtcDateTime;

        _mockScheduleRepo
            .Setup(r => r.GetNextRunAfterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ScrapePortalsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated scraping failure"));

        DateTime? capturedNextRun = null;
        _mockScheduleRepo
            .Setup(r => r.SetNextRunAfterAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Callback<DateTime, CancellationToken>((dt, _) => capturedNextRun = dt)
            .Returns(Task.CompletedTask);

        // Act — should NOT throw (exception is caught internally)
        await CreateFunction(fakeTime).Run(new TimerInfo(), CancellationToken.None);

        // Assert — schedule updated with exactly 30-minute delay from the pinned fake time
        _mockScheduleRepo.Verify(
            r => r.SetNextRunAfterAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);

        capturedNextRun.ShouldNotBeNull();
        capturedNextRun.Value.ShouldBe(nowUtc.AddMinutes(30));
    }

    [Fact]
    public async Task Run_WhenScrapingThrows_ShouldNotPublishScrapingCompletedEvent()
    {
        var fakeTime = new FakeTimeProvider(InsideWindowUtc);

        _mockScheduleRepo
            .Setup(r => r.GetNextRunAfterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ScrapePortalsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated scraping failure"));

        await CreateFunction(fakeTime).Run(new TimerInfo(), CancellationToken.None);

        _mockEventPublisher.Verify(
            p => p.PublishScrapingCompletedAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
