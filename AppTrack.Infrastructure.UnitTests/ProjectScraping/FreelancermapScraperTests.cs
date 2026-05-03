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

    private static readonly IReadOnlySet<string> NoKnownUrls = new HashSet<string>();

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

    private static string DetailHtmlFull(
        string description,
        string location = "",
        string duration = "",
        string startDateText = "",
        string contactPerson = "")
    {
        var locationHtml = string.IsNullOrEmpty(location) ? "" : $"""
            <div class="project-info-list">
              <div data-testid="city" class="align-items-center">
                <a class="city">{location}</a>
              </div>
            </div>
            """;

        var durationHtml = string.IsNullOrEmpty(duration) ? "" : $"""
            <div data-testid="duration" class="align-items-center">
              <span class="mg-r-display-s">{duration}</span>
            </div>
            """;

        var startDateHtml = string.IsNullOrEmpty(startDateText) ? "" : $"""
            <div data-testid="beginningText" class="align-items-center">
              <span class="mg-r-display-s">{startDateText}</span>
            </div>
            """;

        var contactHtml = string.IsNullOrEmpty(contactPerson) ? "" : $"""
            <div class="project-info-title">
              <div>
                <span class="project-body-info-title">Ansprechpartner: </span>
                <span class="project-info-name">{contactPerson}</span>
              </div>
            </div>
            """;

        return $"""
            <html><body>
              <div class="ql-editor">{description}</div>
              {locationHtml}
              {durationHtml}
              {startDateHtml}
              {contactHtml}
            </body></html>
            """;
    }

    [Fact]
    public async Task ScrapeAsync_HappyPath_ReturnsSuccessWithCorrectData()
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

        var result = await BuildScraper(responses).ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        result.ListingSucceeded.ShouldBeTrue();
        result.ListingItemCount.ShouldBe(2);
        result.Items.Count.ShouldBe(2);
        result.Items[0].Position.ShouldBe("Senior .NET Developer");
        result.Items[0].Url.ShouldBe(detailUrl1);
        result.Items[0].CompanyName.ShouldBe("Acme GmbH");
        result.Items[0].JobDescription.ShouldBe("Work on a greenfield .NET application.");
        result.Items[0].PortalName.ShouldBe("Freelancermap");
        result.Items[1].Position.ShouldBe("Cloud Architect");
        result.Items[1].JobDescription.ShouldBe("Design and implement cloud-native services.");
    }

    [Fact]
    public async Task ScrapeAsync_ListingPageFails_ReturnsFailureResult()
    {
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        // portalUrl not in responses → 404 → EnsureSuccessStatusCode throws
        var result = await BuildScraper([]).ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        result.ListingSucceeded.ShouldBeFalse();
        result.Items.ShouldBeEmpty();
        result.ErrorMessage.ShouldNotBeNullOrEmpty();
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

        var result = await BuildScraper(responses).ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        result.ListingSucceeded.ShouldBeTrue();
        result.ListingItemCount.ShouldBe(1); // only the card with a title link
        result.Items.ShouldHaveSingleItem();
        result.Items[0].Position.ShouldBe("Valid Project");
    }

    [Fact]
    public async Task ScrapeAsync_DetailPageFetchFails_ReturnsItemWithEmptyDescription()
    {
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string detailUrl = "https://www.freelancermap.de/projekte/failing";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(detailUrl, "Failing Project", "Error Corp"))
            // detailUrl deliberately absent → 404 → empty description
        };

        var result = await BuildScraper(responses).ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        result.ListingSucceeded.ShouldBeTrue();
        result.Items.ShouldHaveSingleItem();
        result.Items[0].JobDescription.ShouldBe(string.Empty);
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

        var result = await BuildScraper(responses).ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        result.ListingSucceeded.ShouldBeTrue();
        result.Items.ShouldHaveSingleItem();
        result.Items[0].JobDescription.ShouldBe(string.Empty);
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

        var result = await BuildScraper(responses).ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        result.ListingSucceeded.ShouldBeTrue();
        result.Items.ShouldHaveSingleItem();
        result.Items[0].Url.ShouldBe(expectedAbsoluteUrl);
        result.Items[0].JobDescription.ShouldBe("Relative description.");
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

        await scraper.ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

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

        await scraper.ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        // 1 warm-up delay + 2 delays between 3 detail pages = 3 total
        delayCallCount.ShouldBe(3);
    }

    [Fact]
    public async Task ScrapeAsync_KnownUrls_AreSkippedAndNotReturned()
    {
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string knownUrl = "https://www.freelancermap.de/projekte/already-known";
        const string newUrl = "https://www.freelancermap.de/projekte/brand-new";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(
                CardHtml(knownUrl, "Known Project", "Old Corp"),
                CardHtml(newUrl, "New Project", "New Corp")),
            [newUrl] = DetailHtml("Fresh description.")
            // knownUrl detail page intentionally absent — must not be requested
        };

        var requestedUrls = new List<string>();
        var handler = new FakeHttpMessageHandler(responses);
        var captureHandler = new CapturingHandler(handler, requestedUrls);
        var scraper = new FreelancermapScraper(
            new HttpClient(captureHandler),
            NullLogger<FreelancermapScraper>.Instance,
            (_, _) => Task.CompletedTask);

        var knownUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { knownUrl };
        var result = await scraper.ScrapeAsync(portalUrl, knownUrls, CancellationToken.None);

        result.ListingSucceeded.ShouldBeTrue();
        result.ListingItemCount.ShouldBe(2); // both cards on listing
        result.Items.ShouldHaveSingleItem();  // only the new one returned
        result.Items[0].Position.ShouldBe("New Project");
        result.Items[0].JobDescription.ShouldBe("Fresh description.");
        requestedUrls.ShouldNotContain(knownUrl);
    }

    [Fact]
    public async Task ScrapeAsync_WarmUpRequestFails_StillReturnsSuccessWithItems()
    {
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string detailUrl = "https://www.freelancermap.de/projekte/warmup-test";
        // The warm-up URL (https://www.freelancermap.de/) is absent → returns 404 → exception swallowed
        // The listing URL and detail URL are present → scraping continues normally

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(detailUrl, "WarmUp Project", "Test Corp")),
            [detailUrl] = DetailHtml("Description after failed warm-up.")
        };

        var result = await BuildScraper(responses).ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        result.ListingSucceeded.ShouldBeTrue();
        result.Items.ShouldHaveSingleItem();
        result.Items[0].Position.ShouldBe("WarmUp Project");
        result.Items[0].JobDescription.ShouldBe("Description after failed warm-up.");
    }

    [Fact]
    public async Task ScrapeAsync_AllItemsAreKnown_NoDetailFetchesOccur()
    {
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string knownUrl1 = "https://www.freelancermap.de/projekte/known-1";
        const string knownUrl2 = "https://www.freelancermap.de/projekte/known-2";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(
                CardHtml(knownUrl1, "Known 1", "Corp A"),
                CardHtml(knownUrl2, "Known 2", "Corp B"))
            // Detail pages are intentionally absent — they must never be requested
        };

        var requestedUrls = new List<string>();
        var handler = new FakeHttpMessageHandler(responses);
        var captureHandler = new CapturingHandler(handler, requestedUrls);
        var scraper = new FreelancermapScraper(
            new HttpClient(captureHandler),
            NullLogger<FreelancermapScraper>.Instance,
            (_, _) => Task.CompletedTask);

        var knownUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { knownUrl1, knownUrl2 };
        var result = await scraper.ScrapeAsync(portalUrl, knownUrls, CancellationToken.None);

        result.ListingSucceeded.ShouldBeTrue();
        result.ListingItemCount.ShouldBe(2);
        result.Items.ShouldBeEmpty();
        requestedUrls.ShouldNotContain(knownUrl1);
        requestedUrls.ShouldNotContain(knownUrl2);
    }

    [Fact]
    public async Task ScrapeAsync_SingleNewItem_OnlyWarmUpDelayFires()
    {
        // With only one new item, the "between detail pages" delay never fires —
        // only the single warm-up delay (after visiting the homepage) should occur.
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string detailUrl = "https://www.freelancermap.de/projekte/only-one";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(detailUrl, "Only Project", "Solo Corp")),
            [detailUrl] = DetailHtml("Solo description.")
        };

        var delayCallCount = 0;
        var scraper = BuildScraper(responses, (_, _) => { delayCallCount++; return Task.CompletedTask; });

        await scraper.ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        // Only the warm-up delay (1), no between-page delays (0) = 1 total
        delayCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task ScrapeAsync_AllItemsKnown_OnlyWarmUpDelayFires()
    {
        // When every listed item is already known, no detail pages are fetched,
        // so only the warm-up delay should be invoked.
        const string portalUrl = "https://www.freelancermap.de/projektboerse.html";
        const string knownUrl = "https://www.freelancermap.de/projekte/existing";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(knownUrl, "Existing", "Old Corp"))
        };

        var delayCallCount = 0;
        var knownUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { knownUrl };
        var scraper = BuildScraper(responses, (_, _) => { delayCallCount++; return Task.CompletedTask; });

        await scraper.ScrapeAsync(portalUrl, knownUrls, CancellationToken.None);

        // Only the warm-up delay (1) — no detail fetches
        delayCallCount.ShouldBe(1);
    }

    // -----------------------------------------------------------------------
    // Detail-page field extraction
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ScrapeAsync_ExtractsLocationFromDetailPage()
    {
        const string portalUrl = "https://www.freelancermap.de/projekte";
        const string detailUrl = "https://www.freelancermap.de/projekte/loc-test";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(detailUrl, "Location Project", "Corp")),
            [detailUrl] = DetailHtmlFull("Some description.", location: "Berlin, Deutschland")
        };

        var result = await BuildScraper(responses).ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        result.Items.ShouldHaveSingleItem();
        result.Items[0].Location.ShouldBe("Berlin, Deutschland");
    }

    [Fact]
    public async Task ScrapeAsync_ExtractsDurationInMonthsFromDetailPage()
    {
        const string portalUrl = "https://www.freelancermap.de/projekte";
        const string detailUrl = "https://www.freelancermap.de/projekte/dur-test";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(detailUrl, "Duration Project", "Corp")),
            [detailUrl] = DetailHtmlFull("Some description.", duration: "6 Monate+")
        };

        var result = await BuildScraper(responses).ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        result.Items.ShouldHaveSingleItem();
        result.Items[0].DurationInMonths.ShouldBe("6");
    }

    [Fact]
    public async Task ScrapeAsync_ExtractsStartDateTextFromDetailPage()
    {
        const string portalUrl = "https://www.freelancermap.de/projekte";
        const string detailUrl = "https://www.freelancermap.de/projekte/date-test";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(detailUrl, "Date Project", "Corp")),
            [detailUrl] = DetailHtmlFull("Some description.", startDateText: "ab sofort")
        };

        var result = await BuildScraper(responses).ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        result.Items.ShouldHaveSingleItem();
        result.Items[0].StartDateText.ShouldBe("ab sofort");
    }

    [Fact]
    public async Task ScrapeAsync_ExtractsContactPersonFromDetailPage()
    {
        const string portalUrl = "https://www.freelancermap.de/projekte";
        const string detailUrl = "https://www.freelancermap.de/projekte/contact-test";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(detailUrl, "Contact Project", "Corp")),
            [detailUrl] = DetailHtmlFull("Some description.", contactPerson: "Thomas Parsons")
        };

        var result = await BuildScraper(responses).ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        result.Items.ShouldHaveSingleItem();
        result.Items[0].ContactPerson.ShouldBe("Thomas Parsons");
    }

    [Fact]
    public async Task ScrapeAsync_WhenDetailFieldsAbsent_ReturnsEmptyStrings()
    {
        const string portalUrl = "https://www.freelancermap.de/projekte";
        const string detailUrl = "https://www.freelancermap.de/projekte/minimal";

        var responses = new Dictionary<string, string>
        {
            [portalUrl] = ListingHtml(CardHtml(detailUrl, "Minimal Project", "Corp")),
            [detailUrl] = DetailHtmlFull("Only description here.")
        };

        var result = await BuildScraper(responses).ScrapeAsync(portalUrl, NoKnownUrls, CancellationToken.None);

        result.Items.ShouldHaveSingleItem();
        result.Items[0].Location.ShouldBe(string.Empty);
        result.Items[0].DurationInMonths.ShouldBe(string.Empty);
        result.Items[0].StartDateText.ShouldBe(string.Empty);
        result.Items[0].ContactPerson.ShouldBe(string.Empty);
    }

    private sealed class CapturingHandler(HttpMessageHandler inner, List<string> captured) : DelegatingHandler(inner)
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            captured.Add(request.RequestUri!.ToString());
            return base.SendAsync(request, cancellationToken);
        }
    }
}
