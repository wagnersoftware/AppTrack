using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

public class StubRssFeedReader : IRssFeedReader
{
    public static readonly List<RawFeedItem> FakeItems =
    [
        new("Senior .NET Developer - Berlin", "https://example.com/job/1", ".NET Core, Azure", DateTime.UtcNow),
        new("Marketing Manager", "https://example.com/job/2", "Brand strategy", DateTime.UtcNow)
    ];

    public Task<List<RawFeedItem>> ReadAsync(string feedUrl, CancellationToken ct)
        => Task.FromResult(FakeItems);
}
