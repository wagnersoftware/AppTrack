using AppTrack.Application.Features.RssFeeds.Models;
using AppTrack.Domain.Enums;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IRssFeedItemParser
{
    RssJobApplicationData Parse(RawFeedItem item, RssParserType parserType, string portalName);
}
