using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class ScrapedProject : BaseEntity
{
    public int ProjectPortalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public DateTime ScrapedAt { get; set; }
    public ProjectPortal ProjectPortal { get; set; } = null!;
}
