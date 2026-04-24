using AppTrack.Frontend.Models.Base;

namespace AppTrack.Frontend.Models;

public class RssPortalModel : ModelBase
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsSubscribed { get; set; }
}
