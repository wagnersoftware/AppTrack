using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;
using CodeHollow.FeedReader;

namespace AppTrack.Infrastructure.RssFeed;

public class RssFeedReader : IRssFeedReader
{
    public async Task<List<RawFeedItem>> ReadAsync(string feedUrl, CancellationToken ct)
    {
        var feed = await FeedReader.ReadAsync(feedUrl);
        return feed.Items
            .Select(item => new RawFeedItem(
                Title: item.Title ?? string.Empty,
                Url: item.Link ?? string.Empty,
                Description: item.Description ?? string.Empty,
                PublishedAt: item.PublishingDate))
            .Where(item => !string.IsNullOrEmpty(item.Url))
            .ToList();
    }
}
