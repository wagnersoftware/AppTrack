using AngleSharp;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Models;
using Microsoft.Extensions.Logging;

namespace AppTrack.Infrastructure.ProjectScraping;

public class FreelancermapScraper : IProjectScraper
{
    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";

    private readonly HttpClient _httpClient;
    private readonly ILogger<FreelancermapScraper> _logger;
    private readonly Func<int, CancellationToken, Task> _delayProvider;

    public FreelancermapScraper(
        HttpClient httpClient,
        ILogger<FreelancermapScraper> logger,
        Func<int, CancellationToken, Task>? delayProvider = null)
    {
        _delayProvider = delayProvider ?? Task.Delay;
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "de-DE,de;q=0.9,en;q=0.8");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
    }

    public async Task<List<ScrapedProjectData>> ScrapeAsync(string portalUrl, CancellationToken ct)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(portalUrl, ct);
            using var context = BrowsingContext.New(Configuration.Default);
            using var document = await context.OpenAsync(req => req.Content(html), ct);

            var items = new List<(string Title, string Url, string Company)>();

            foreach (var card in document.QuerySelectorAll(".project-card"))
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

                items.Add((title, url, company));
            }

            var descriptions = new List<string>(items.Count);
            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                    await _delayProvider(Random.Shared.Next(2000, 5000), ct);
                descriptions.Add(await FetchDescriptionAsync(items[i].Url, ct));
            }

            return items
                .Select((item, i) => new ScrapedProjectData(
                    Position: item.Title,
                    Url: item.Url,
                    JobDescription: descriptions[i],
                    CompanyName: item.Company,
                    PortalName: "Freelancermap"))
                .ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error scraping Freelancermap at {Url}", portalUrl);
            return [];
        }
    }

    private async Task<string> FetchDescriptionAsync(string projectUrl, CancellationToken ct)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(projectUrl, ct);
            using var ctx = BrowsingContext.New(Configuration.Default);
            using var doc = await ctx.OpenAsync(req => req.Content(html), ct);
            return doc.QuerySelector(".ql-editor")?.TextContent.Trim() ?? string.Empty;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to fetch description from {Url}", projectUrl);
            return string.Empty;
        }
    }
}
