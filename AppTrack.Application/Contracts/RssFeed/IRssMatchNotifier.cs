using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IRssMatchNotifier
{
    Task NotifyAsync(string userId, string userEmail, List<RssJobApplicationData> matches, CancellationToken ct);
}
