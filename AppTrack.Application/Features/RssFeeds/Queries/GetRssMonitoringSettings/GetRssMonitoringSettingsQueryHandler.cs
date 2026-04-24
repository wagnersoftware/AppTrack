using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Dto;

namespace AppTrack.Application.Features.RssFeeds.Queries.GetRssMonitoringSettings;

public class GetRssMonitoringSettingsQueryHandler
    : IRequestHandler<GetRssMonitoringSettingsQuery, RssMonitoringSettingsDto>
{
    private readonly IRssMonitoringSettingsRepository _repository;

    public GetRssMonitoringSettingsQueryHandler(IRssMonitoringSettingsRepository repository)
        => _repository = repository;

    public async Task<RssMonitoringSettingsDto> Handle(
        GetRssMonitoringSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByUserIdAsync(request.UserId);
        return settings is null
            ? new RssMonitoringSettingsDto([], 60, false)
            : new RssMonitoringSettingsDto(settings.Keywords, settings.PollIntervalMinutes, settings.NotifyByEmail);
    }
}
