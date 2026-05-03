using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class UserProjectMatch : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int ScrapedProjectId { get; set; }
    public ScrapedProject ScrapedProject { get; set; } = null!;
    public bool IsNotified { get; set; }
}
