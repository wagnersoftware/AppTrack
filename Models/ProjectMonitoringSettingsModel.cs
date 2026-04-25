using AppTrack.Frontend.Models.Base;

namespace AppTrack.Frontend.Models;

public class ProjectMonitoringSettingsModel : ModelBase
{
    public List<string> Keywords { get; set; } = [];
    public int NotificationIntervalMinutes { get; set; } = 60;
    public bool NotifyByEmail { get; set; }
}
