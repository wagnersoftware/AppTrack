using AppTrack.Application.Features.ProjectMonitoring.Models;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IProjectScraper
{
    Task<ScrapingResult> ScrapeAsync(string portalUrl, IReadOnlySet<string> knownUrls, CancellationToken ct);
}
