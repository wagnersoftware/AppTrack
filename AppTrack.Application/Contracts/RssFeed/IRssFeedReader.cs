using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IRssFeedReader
{
    Task<List<RawFeedItem>> ReadAsync(string feedUrl, CancellationToken ct);
}
