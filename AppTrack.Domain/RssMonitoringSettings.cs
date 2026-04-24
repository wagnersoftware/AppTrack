using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class RssMonitoringSettings : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = [];
    public int PollIntervalMinutes { get; set; } = 60;
    public bool NotifyByEmail { get; set; }
    public string NotificationEmail { get; set; } = string.Empty;
}
