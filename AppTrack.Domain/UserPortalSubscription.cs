using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class UserPortalSubscription : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int ProjectPortalId { get; set; }
    public bool IsActive { get; set; }
    public ProjectPortal ProjectPortal { get; set; } = null!;
}
