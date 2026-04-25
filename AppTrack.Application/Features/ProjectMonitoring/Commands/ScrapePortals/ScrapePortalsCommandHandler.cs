using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Shared;
using AppTrack.Domain;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.ScrapePortals;

public class ScrapePortalsCommandHandler : IRequestHandler<ScrapePortalsCommand, Unit>
{
    private readonly IProjectPortalRepository _portalRepository;
    private readonly IProjectScraperFactory _scraperFactory;
    private readonly IScrapedProjectRepository _scrapedProjectRepository;

    public ScrapePortalsCommandHandler(
        IProjectPortalRepository portalRepository,
        IProjectScraperFactory scraperFactory,
        IScrapedProjectRepository scrapedProjectRepository)
    {
        _portalRepository = portalRepository;
        _scraperFactory = scraperFactory;
        _scrapedProjectRepository = scrapedProjectRepository;
    }

    public async Task<Unit> Handle(ScrapePortalsCommand request, CancellationToken cancellationToken)
    {
        var portals = await _portalRepository.GetAllActiveAsync();

        foreach (var portal in portals)
        {
            var scraper = _scraperFactory.GetScraper(portal.ScraperType);
            var scraped = await scraper.ScrapeAsync(portal.Url, cancellationToken);

            var projects = scraped.Select(item => new ScrapedProject
            {
                ProjectPortalId = portal.Id,
                Title = item.Position,
                Url = item.Url,
                CompanyName = item.CompanyName,
                ScrapedAt = DateTime.UtcNow
            });

            await _scrapedProjectRepository.ReplaceForPortalAsync(portal.Id, projects, cancellationToken);
        }

        return Unit.Value;
    }
}
