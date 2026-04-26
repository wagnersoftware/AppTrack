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

        public FakeHttpMessageHandler(Dictionary<string, string> responses)
        {
            _responses = responses;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
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

    private static FreelancermapScraper BuildScraper(Dictionary<string, string> responses)
    {
        var handler = new FakeHttpMessageHandler(responses);
        var httpClient = new HttpClient(handler);
        return new FreelancermapScraper(httpClient, NullLogger<FreelancermapScraper>.Instance);
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
}
