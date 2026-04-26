using AppTrack.Application.Features.ProjectMonitoring.Models;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.ScrapePortals;

internal static class ScrapedProjectDataValidator
{
    public static string? Validate(ScrapedProjectData item)
    {
        if (string.IsNullOrEmpty(item.JobDescription))
            return "empty description";
        if (string.IsNullOrEmpty(item.Position) || item.Position.Length > 500)
            return "missing or oversized title";
        if (string.IsNullOrEmpty(item.Url) || item.Url.Length > 2000)
            return "missing or oversized URL";
        if (string.IsNullOrEmpty(item.CompanyName) || item.CompanyName.Length > 300)
            return "missing or oversized company name";
        return null;
    }
}
