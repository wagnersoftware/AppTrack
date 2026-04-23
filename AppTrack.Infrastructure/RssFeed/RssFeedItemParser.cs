using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;
using AppTrack.Domain.Enums;
using AppTrack.Infrastructure.RssFeed.Parsers;

namespace AppTrack.Infrastructure.RssFeed;

public class RssFeedItemParser : IRssFeedItemParser
{
    public RssJobApplicationData Parse(RawFeedItem item, RssParserType parserType, string portalName)
    {
        var parser = RssFeedParserFactory.GetParser(parserType);
        return parser.Parse(item, portalName);
    }
}
