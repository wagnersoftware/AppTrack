using System.Text.RegularExpressions;
using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Infrastructure.RssFeed.Parsers;

internal class FreelancerMapFeedParser : IRssFeedParser
{
    public RssJobApplicationData Parse(RawFeedItem item, string portalName) =>
        new(
            Position: item.Title,
            Url: item.Url,
            JobDescription: StripHtml(item.Description),
            CompanyName: string.Empty,
            PortalName: portalName);

    private static string StripHtml(string html) =>
        Regex.Replace(html, "<[^>]*>", string.Empty).Trim();
}
