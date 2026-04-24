using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Mappings;

internal static class RssMappings
{
    internal static RssPortalModel ToModel(this RssPortalDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name ?? string.Empty,
        Url = dto.Url ?? string.Empty,
        IsSubscribed = dto.IsSubscribed,
    };

    internal static RssMonitoringSettingsModel ToModel(this RssMonitoringSettingsDto dto) => new()
    {
        Keywords = dto.Keywords?.ToList() ?? [],
        PollIntervalMinutes = dto.PollIntervalMinutes,
        NotifyByEmail = dto.NotifyByEmail,
    };

    internal static SetRssSubscriptionsCommand ToSetSubscriptionsCommand(this IEnumerable<RssPortalModel> portals) => new()
    {
        Subscriptions = portals.Select(p => new RssSubscriptionItemDto { PortalId = p.Id, IsActive = p.IsSubscribed }).ToList(),
    };

    internal static UpdateRssMonitoringSettingsCommand ToUpdateSettingsCommand(this RssMonitoringSettingsModel model) => new()
    {
        Keywords = model.Keywords,
        PollIntervalMinutes = model.PollIntervalMinutes,
        NotifyByEmail = model.NotifyByEmail,
    };
}
