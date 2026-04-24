using AppTrack.Frontend.Models.Base;

namespace AppTrack.Frontend.Models;

public class RssMonitoringSettingsModel : ModelBase
{
    public List<string> Keywords { get; set; } = [];
    public int PollIntervalMinutes { get; set; } = 60;
    public bool NotifyByEmail { get; set; }
}
