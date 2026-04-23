using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Infrastructure.RssFeed.Parsers;

internal interface IRssFeedParser
{
    RssJobApplicationData Parse(RawFeedItem item, string portalName);
}
