using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Domain.Enums;

namespace AppTrack.Infrastructure.ProjectScraping;

public class ProjectScraperFactory : IProjectScraperFactory
{
    private readonly FreelancermapScraper _freelancermapScraper;

    public ProjectScraperFactory(FreelancermapScraper freelancermapScraper)
    {
        _freelancermapScraper = freelancermapScraper;
    }

    public IProjectScraper GetScraper(ScraperType scraperType) =>
        scraperType switch
        {
            ScraperType.FreelancerMap => _freelancermapScraper,
            _ => throw new ArgumentOutOfRangeException(nameof(scraperType), scraperType, null)
        };
}
