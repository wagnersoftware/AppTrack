using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class UserRssSubscription : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int RssPortalId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastPolledAt { get; set; }
    public RssPortal RssPortal { get; set; } = null!;
}
