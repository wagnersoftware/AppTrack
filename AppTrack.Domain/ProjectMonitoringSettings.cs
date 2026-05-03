using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class ProjectMonitoringSettings : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = [];
    public int NotificationIntervalMinutes { get; set; } = 60;
    public bool NotifyByEmail { get; set; }
    public string NotificationEmail { get; set; } = string.Empty;
    public DateTime? LastNotifiedAt { get; set; }
}
