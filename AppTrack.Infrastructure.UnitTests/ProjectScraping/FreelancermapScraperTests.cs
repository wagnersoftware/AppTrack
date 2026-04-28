using AppTrack.Infrastructure.ProjectScraping;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using System.Net;

namespace AppTrack.Infrastructure.UnitTests.ProjectScraping;

public class FreelancermapScraperTests
{
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, string> _responses;
        public List<string> CapturedUserAgents { get; } = [];

        public FakeHttpMessageHandler(Dictionary<string, string> responses)
        {
            _responses = responses;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CapturedUserAgents.Add(request.Headers.UserAgent.ToString());

            var url = request.RequestUri!.ToString();

            if (_responses.TryGetValue(url, out var body))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body)
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }

    private static FreelancermapScraper BuildScraper(
        Dictionary<string, string> responses,
        Func<int, CancellationToken, Task>? delayProvider = null)
    {
        var handler = new FakeHttpMessageHandler(responses);
        var httpClient = new HttpClient(handler);
        Func<int, CancellationToken, Task> delay = delayProvider ?? ((_, _) => Task.CompletedTask);
        return new FreelancermapScraper(httpClient, NullLogger<FreelancermapScraper>.Instance, delay);
    }

    private static string ListingHtml(params string[] cardHtmlFragments)
    {
        var cards = string.Join("\n", cardHtmlFragments);
        return $"<html><body>\n{cards}\n</body></html>";
    }

    private static string CardHtml(string href, string title, string company) =>
        $"""
        <div class="project-card">
          <a data-testid="title" href="{href}">{title}</a>
          <div class="project-info">
            <span class="mg-b-display-m">{company}</span>
          </div>
        </div>
        """;

    private static string CardHtmlNoTitleLink(string company) =>
        $"""
        <div class="project-card">
          <div class="project-info">
            <span class="mg-b-display-m">{company}</span>
          </div>
        </div>
        """;

    private static string DetailHtml(string? qlEditorContent) =>
        qlEditorContent is null
            ? "<html><body><p>No editor here</p></body></html>"
            : $"""<html><body><div class="ql-editor">{qlEditorContent}</div></body></html>""";

    [Fact]
    public async Task ScrapeAsync_HappyPath_ReturnsCorrectScrapedProjectData()
    {
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string detailUrl1 = "https://www.freelancermap.de/projekte/dev-1";
        const string detailUrl2 = "https://www.freelancermap.de/projekte/dev-2";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(
                CardHtml(detailUrl1, "Senior .NET Developer", "Acme GmbH"),
                CardHtml(detailUrl2, "Cloud Architect", "TechCorp AG")),
            [detailUrl1] = DetailHtml("Work on a greenfield .NET application."),
            [detailUrl2] = DetailHtml("Design and implement cloud-native services.")
        };

        var results = await BuildScraper(responses).ScrapeAsync(portalUrl, CancellationToken.None);

        results.Count.ShouldBe(2);
        results[0].Position.ShouldBe("Senior .NET Developer");
        results[0].Url.ShouldBe(detailUrl1);
        results[0].CompanyName.ShouldBe("Acme GmbH");
        results[0].JobDescription.ShouldBe("Work on a greenfield .NET application.");
        results[0].PortalName.ShouldBe("Freelancermap");
        results[1].Position.ShouldBe("Cloud Architect");
        results[1].JobDescription.ShouldBe("Design and implement cloud-native services.");
    }

    [Fact]
    public async Task ScrapeAsync_CardWithoutTitleLink_IsSkipped()
    {
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string detailUrl = "https://www.freelancermap.de/projekte/valid";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(
                CardHtmlNoTitleLink("Ghost Company"),
                CardHtml(detailUrl, "Valid Project", "Real GmbH")),
            [detailUrl] = DetailHtml("Real description.")
        };

        var results = await BuildScraper(responses).ScrapeAsync(portalUrl, CancellationToken.None);

        results.ShouldHaveSingleItem();
        results[0].Position.ShouldBe("Valid Project");
    }

    [Fact]
    public async Task ScrapeAsync_DetailPageFetchFails_ReturnsProjectWithEmptyDescription()
    {
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string detailUrl = "https://www.freelancermap.de/projekte/failing";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(detailUrl, "Failing Project", "Error Corp"))
            // detailUrl deliberately absent → 404 → empty description
        };

        var results = await BuildScraper(responses).ScrapeAsync(portalUrl, CancellationToken.None);

        results.ShouldHaveSingleItem();
        results[0].JobDescription.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task ScrapeAsync_DetailPageHasNoQlEditor_ReturnsEmptyDescription()
    {
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string detailUrl = "https://www.freelancermap.de/projekte/no-editor";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(detailUrl, "No Editor Project", "Plain Corp")),
            [detailUrl] = DetailHtml(null)
        };

        var results = await BuildScraper(responses).ScrapeAsync(portalUrl, CancellationToken.None);

        results.ShouldHaveSingleItem();
        results[0].JobDescription.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task ScrapeAsync_RelativeHrefOnCard_IsResolvedToAbsoluteUrl()
    {
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string relativeHref = "/projekte/relative-project";
        const string expectedAbsoluteUrl = "https://www.freelancermap.de/projekte/relative-project";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(relativeHref, "Relative Project", "Rel Corp")),
            [expectedAbsoluteUrl] = DetailHtml("Relative description.")
        };

        var results = await BuildScraper(responses).ScrapeAsync(portalUrl, CancellationToken.None);

        results.ShouldHaveSingleItem();
        results[0].Url.ShouldBe(expectedAbsoluteUrl);
        results[0].JobDescription.ShouldBe("Relative description.");
    }

    [Fact]
    public async Task ScrapeAsync_SendsBrowserUserAgentOnAllRequests()
    {
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string detailUrl = "https://www.freelancermap.de/projekte/ua-test";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(detailUrl, "UA Test Project", "Test Corp")),
            [detailUrl] = DetailHtml("Some description.")
        };

        var handler = new FakeHttpMessageHandler(responses);
        var scraper = new FreelancermapScraper(
            new HttpClient(handler),
            NullLogger<FreelancermapScraper>.Instance,
            (_, _) => Task.CompletedTask);

        await scraper.ScrapeAsync(portalUrl, CancellationToken.None);

        handler.CapturedUserAgents.ShouldNotBeEmpty();
        handler.CapturedUserAgents.ShouldAllBe(ua => ua.Contains("Mozilla"));
    }

    [Fact]
    public async Task ScrapeAsync_WithMultipleDetailPages_DelaysBetweenFetches()
    {
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string detailUrl1 = "https://www.freelancermap.de/projekte/seq-1";
        const string detailUrl2 = "https://www.freelancermap.de/projekte/seq-2";
        const string detailUrl3 = "https://www.freelancermap.de/projekte/seq-3";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(
                CardHtml(detailUrl1, "Project 1", "Corp"),
                CardHtml(detailUrl2, "Project 2", "Corp"),
                CardHtml(detailUrl3, "Project 3", "Corp")),
            [detailUrl1] = DetailHtml("Desc 1."),
            [detailUrl2] = DetailHtml("Desc 2."),
            [detailUrl3] = DetailHtml("Desc 3.")
        };

        var delayCallCount = 0;
        var scraper = BuildScraper(responses, (_, _) => { delayCallCount++; return Task.CompletedTask; });

        await scraper.ScrapeAsync(portalUrl, CancellationToken.None);

        delayCallCount.ShouldBe(2); // 3 detail pages → 2 delays between them
    }
}
