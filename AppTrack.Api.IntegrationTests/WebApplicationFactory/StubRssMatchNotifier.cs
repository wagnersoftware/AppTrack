using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

public class StubRssMatchNotifier : IRssMatchNotifier
{
    public Task NotifyAsync(string userId, string userEmail, List<RssJobApplicationData> matches, CancellationToken ct)
        => Task.CompletedTask;
}
