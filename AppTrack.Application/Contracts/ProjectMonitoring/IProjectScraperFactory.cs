using AppTrack.Domain.Enums;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IProjectScraperFactory
{
    IProjectScraper GetScraper(ScraperType scraperType);
}
