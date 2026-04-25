using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Mappings;

internal static class ProjectMonitoringMappings
{
    internal static ProjectPortalModel ToModel(this ProjectPortalDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name ?? string.Empty,
        Url = dto.Url ?? string.Empty,
        IsSubscribed = dto.IsSubscribed,
    };

    internal static ProjectMonitoringSettingsModel ToModel(this ProjectMonitoringSettingsDto dto) => new()
    {
        Keywords = dto.Keywords?.ToList() ?? [],
        NotificationIntervalMinutes = dto.NotificationIntervalMinutes,
        NotifyByEmail = dto.NotifyByEmail,
    };

    internal static SetPortalSubscriptionsCommand ToSetSubscriptionsCommand(this IEnumerable<ProjectPortalModel> portals) => new()
    {
        Subscriptions = portals.Select(p => new PortalSubscriptionItemDto { PortalId = p.Id, IsActive = p.IsSubscribed }).ToList(),
    };

    internal static UpdateProjectMonitoringSettingsCommand ToUpdateSettingsCommand(this ProjectMonitoringSettingsModel model) => new()
    {
        Keywords = model.Keywords,
        NotificationIntervalMinutes = model.NotificationIntervalMinutes,
        NotifyByEmail = model.NotifyByEmail,
    };
}
