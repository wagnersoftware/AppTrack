using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.RssFeeds.Commands.UpdateRssMonitoringSettings;

public class UpdateRssMonitoringSettingsCommand : IRequest<Unit>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;

    [JsonIgnore]
    public string NotificationEmail { get; set; } = string.Empty;

    public List<string> Keywords { get; set; } = [];

    public int PollIntervalMinutes { get; set; }
}
