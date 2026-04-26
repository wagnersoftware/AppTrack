using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Dto;

namespace AppTrack.Application.Features.ProjectMonitoring.Queries.GetProjectMonitoringSettings;

public class GetProjectMonitoringSettingsQueryHandler
    : IRequestHandler<GetProjectMonitoringSettingsQuery, ProjectMonitoringSettingsDto>
{
    private readonly IProjectMonitoringSettingsRepository _repository;

    public GetProjectMonitoringSettingsQueryHandler(IProjectMonitoringSettingsRepository repository)
        => _repository = repository;

    public async Task<ProjectMonitoringSettingsDto> Handle(
        GetProjectMonitoringSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByUserIdAsync(request.UserId);
        return settings is null
            ? new ProjectMonitoringSettingsDto([], 60, false, 60)
            : new ProjectMonitoringSettingsDto(settings.Keywords, settings.NotificationIntervalMinutes, settings.NotifyByEmail, settings.PollIntervalMinutes);
    }
}
