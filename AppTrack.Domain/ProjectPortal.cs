using AppTrack.Domain.Common;
using AppTrack.Domain.Enums;

namespace AppTrack.Domain;

public class ProjectPortal : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public ScraperType ScraperType { get; set; } = ScraperType.FreelancerMap;
    public bool IsActive { get; set; } = true;
}
