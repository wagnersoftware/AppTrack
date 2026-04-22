namespace AppTrack.Application.Features.RssFeeds.Models;

public record RssJobApplicationData(
    string Position,
    string Url,
    string JobDescription,
    string CompanyName,
    string PortalName);
