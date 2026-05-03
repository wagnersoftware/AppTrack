using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Commands.CleanupProjectData;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

public class CleanupProjectDataCommandHandlerTests
{
    private readonly Mock<IProjectDataCleanupRepository> _cleanupRepo = new();

    [Fact]
    public async Task Handle_ShouldCallCleanupWithCutoffOf60Days()
    {
        DateTime? capturedCutoff = null;
        _cleanupRepo
            .Setup(r => r.CleanupOlderThanAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Callback<DateTime, CancellationToken>((cutoff, _) => capturedCutoff = cutoff)
            .Returns(Task.CompletedTask);

        var handler = new CleanupProjectDataCommandHandler(_cleanupRepo.Object);
        await handler.Handle(new CleanupProjectDataCommand(), CancellationToken.None);

        capturedCutoff.ShouldNotBeNull();
        var expectedCutoff = DateTime.UtcNow.AddDays(-60);
        capturedCutoff.Value.ShouldBeInRange(expectedCutoff.AddSeconds(-5), expectedCutoff.AddSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldReturnUnit()
    {
        _cleanupRepo.Setup(r => r.CleanupOlderThanAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CleanupProjectDataCommandHandler(_cleanupRepo.Object);
        var result = await handler.Handle(new CleanupProjectDataCommand(), CancellationToken.None);

        result.ShouldBe(AppTrack.Application.Shared.Unit.Value);
    }
}
