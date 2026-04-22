using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class ProcessedFeedItem : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string FeedItemUrl { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
