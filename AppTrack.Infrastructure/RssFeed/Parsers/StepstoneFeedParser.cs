using System.Text.RegularExpressions;
using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Infrastructure.RssFeed.Parsers;

/// <summary>
/// Stepstone-specific parser. Extends DefaultFeedParser behaviour
/// with any portal-specific field extraction. Placeholder for now.
/// </summary>
internal class StepstoneFeedParser : IRssFeedParser
{
    public RssJobApplicationData Parse(RawFeedItem item, string portalName) =>
        new(
            Position: item.Title,
            Url: item.Url,
            JobDescription: Regex.Replace(item.Description, "<[^>]*>", string.Empty).Trim(),
            CompanyName: string.Empty,
            PortalName: portalName);
}
