using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.UpdateProjectMonitoringSettings;

public class UpdateProjectMonitoringSettingsCommand : IRequest<Unit>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;

    [JsonIgnore]
    public string NotificationEmail { get; set; } = string.Empty;

    public List<string> Keywords { get; set; } = [];

    public int NotificationIntervalMinutes { get; set; }

    public int PollIntervalMinutes { get; set; }

    public bool NotifyByEmail { get; set; }
}
