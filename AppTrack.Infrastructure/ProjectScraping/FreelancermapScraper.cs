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
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua",
            "\"Chromium\";v=\"124\", \"Google Chrome\";v=\"124\", \"Not-A.Brand\";v=\"99\"");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("DNT", "1");
    }

    public async Task<ScrapingResult> ScrapeAsync(string portalUrl, IReadOnlySet<string> knownUrls, CancellationToken ct)
    {
        try
        {
            var portalUri = new Uri(portalUrl);
            var warmUpUrl = $"{portalUri.Scheme}://{portalUri.Host}/";

            // Warm-up: visit homepage first to establish session/cookies like a real browser.
            // Failure is intentionally swallowed — warm-up is best-effort.
            try { await SendGetAsync(warmUpUrl, referer: null, secFetchSite: "none", ct); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogDebug(ex, "Warm-up request to {Url} failed (ignored)", warmUpUrl);
            }

            await _delayProvider(Random.Shared.Next(1000, 3000), ct);

            var html = await SendGetAsync(portalUrl, referer: warmUpUrl, secFetchSite: "same-origin", ct);
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

            var listingItemCount = items.Count;
            var results = new List<ScrapedProjectData>();
            var isFirstNewFetch = true;

            foreach (var item in items)
            {
                if (knownUrls.Contains(item.Url))
                    continue;

                if (!isFirstNewFetch)
                    await _delayProvider(Random.Shared.Next(5000, 12000), ct);
                isFirstNewFetch = false;

                var description = await FetchDescriptionAsync(item.Url, portalUrl, ct);
                results.Add(new ScrapedProjectData(
                    Position: item.Title,
                    Url: item.Url,
                    JobDescription: description,
                    CompanyName: item.Company,
                    PortalName: "Freelancermap"));
            }

            return ScrapingResult.Success(results, listingItemCount);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error scraping Freelancermap at {Url}", portalUrl);
            return ScrapingResult.Failure(ex.Message);
        }
    }

    private async Task<string> FetchDescriptionAsync(string projectUrl, string listingUrl, CancellationToken ct)
    {
        try
        {
            var html = await SendGetAsync(projectUrl, referer: listingUrl, secFetchSite: "same-origin", ct);
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

    private async Task<string> SendGetAsync(string url, string? referer, string secFetchSite, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
        request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
        request.Headers.TryAddWithoutValidation("sec-fetch-site", secFetchSite);
        request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
        if (referer is not null)
            request.Headers.TryAddWithoutValidation("Referer", referer);

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }
}
