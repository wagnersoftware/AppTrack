using AppTrack.Domain.Enums;

namespace AppTrack.Infrastructure.RssFeed.Parsers;

internal static class RssFeedParserFactory
{
    public static IRssFeedParser GetParser(RssParserType parserType) =>
        parserType switch
        {
            RssParserType.FreelancerMap => new FreelancerMapFeedParser(),
            _ => new DefaultFeedParser()
        };
}
