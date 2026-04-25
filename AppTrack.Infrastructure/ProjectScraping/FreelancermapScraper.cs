using AngleSharp;
using AngleSharp.Dom;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Models;
using Microsoft.Extensions.Logging;

namespace AppTrack.Infrastructure.ProjectScraping;

public class FreelancermapScraper : IProjectScraper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FreelancermapScraper> _logger;

    public FreelancermapScraper(HttpClient httpClient, ILogger<FreelancermapScraper> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ScrapedProjectData>> ScrapeAsync(string portalUrl, CancellationToken ct)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(portalUrl, ct);
            var config = Configuration.Default;
            using var context = BrowsingContext.New(config);
            using var document = await context.OpenAsync(req => req.Content(html), ct);

            var cards = document.QuerySelectorAll(".project-card");
            var results = new List<ScrapedProjectData>();

            foreach (var card in cards)
            {
                var titleLink = card.QuerySelector("a[data-testid=\"title\"]");
                if (titleLink is null) continue;

                var title = titleLink.TextContent.Trim();
                var href = titleLink.GetAttribute("href") ?? string.Empty;
                var url = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? href
                    : new Uri(new Uri(portalUrl), href).ToString();

                var companyElement = card.QuerySelector(".project-info > .mg-b-display-m");
                var company = companyElement?.TextContent.Trim() ?? string.Empty;

                results.Add(new ScrapedProjectData(
                    Position: title,
                    Url: url,
                    JobDescription: string.Empty,
                    CompanyName: company,
                    PortalName: "Freelancermap"));
            }

            return results;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error scraping Freelancermap at {Url}", portalUrl);
            return [];
        }
    }
}
