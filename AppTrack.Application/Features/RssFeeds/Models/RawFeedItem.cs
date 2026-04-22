namespace AppTrack.Application.Features.RssFeeds.Models;

public record RawFeedItem(string Title, string Url, string Description, DateTime? PublishedAt);
