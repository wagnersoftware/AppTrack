using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class ScrapedProject : BaseEntity
{
    public int ProjectPortalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string DurationInMonths { get; set; } = string.Empty;
    public string StartDateText { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public ProjectPortal ProjectPortal { get; set; } = null!;
}
