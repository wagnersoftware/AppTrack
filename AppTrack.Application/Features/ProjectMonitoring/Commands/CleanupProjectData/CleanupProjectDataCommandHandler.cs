using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.CleanupProjectData;

public class CleanupProjectDataCommandHandler(IProjectDataCleanupRepository cleanupRepository)
    : IRequestHandler<CleanupProjectDataCommand, Unit>
{
    private const int RetentionDays = 60;

    public async Task<Unit> Handle(CleanupProjectDataCommand request, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-RetentionDays);
        await cleanupRepository.CleanupOlderThanAsync(cutoff, cancellationToken);
        return Unit.Value;
    }
}
