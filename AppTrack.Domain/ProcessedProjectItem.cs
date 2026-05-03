using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class ProcessedProjectItem : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string ProjectItemUrl { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
