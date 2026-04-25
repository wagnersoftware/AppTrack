using AppTrack.Application.Features.ProjectMonitoring.Models;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IProjectScraper
{
    Task<List<ScrapedProjectData>> ScrapeAsync(string portalUrl, CancellationToken ct);
}
