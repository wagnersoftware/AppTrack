using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Models;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using Microsoft.Extensions.Logging;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.ScrapePortals;

public class ScrapePortalsCommandHandler : IRequestHandler<ScrapePortalsCommand, Unit>
{
    private readonly IProjectPortalRepository _portalRepository;
    private readonly IProjectScraperFactory _scraperFactory;
    private readonly IScrapedProjectRepository _scrapedProjectRepository;
    private readonly ILogger<ScrapePortalsCommandHandler> _logger;

    public ScrapePortalsCommandHandler(
        IProjectPortalRepository portalRepository,
        IProjectScraperFactory scraperFactory,
        IScrapedProjectRepository scrapedProjectRepository,
        ILogger<ScrapePortalsCommandHandler> logger)
    {
        _portalRepository = portalRepository;
        _scraperFactory = scraperFactory;
        _scrapedProjectRepository = scrapedProjectRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(ScrapePortalsCommand request, CancellationToken cancellationToken)
    {
        var portals = await _portalRepository.GetAllActiveAsync();

        foreach (var portal in portals)
        {
            var scraper = _scraperFactory.GetScraper(portal.ScraperType);
            var scraped = await scraper.ScrapeAsync(portal.Url, cancellationToken);

            var projects = new List<ScrapedProject>();
            foreach (var item in scraped)
            {
                var reason = ScrapedProjectDataValidator.Validate(item);
                if (reason is not null)
                {
                    _logger.LogWarning("Skipping scraped project from {Portal}: {Reason}. Url={Url}", portal.Name, reason, item.Url);
                    continue;
                }

                projects.Add(new ScrapedProject
                {
                    ProjectPortalId = portal.Id,
                    Title = item.Position,
                    Url = item.Url,
                    CompanyName = item.CompanyName,
                    Description = item.JobDescription,
                    ScrapedAt = DateTime.UtcNow
                });
            }

            await _scrapedProjectRepository.AddNewForPortalAsync(portal.Id, projects, cancellationToken);
        }

        return Unit.Value;
    }


}
