using AppTrack.Application.Features.ProjectMonitoring.Models;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Models;

public class ScrapingResultTests
{
    // -----------------------------------------------------------------------
    // Success factory method
    // -----------------------------------------------------------------------

    [Fact]
    public void Success_ShouldSetListingSucceeded_True()
    {
        var result = ScrapingResult.Success([], 0);

        result.ListingSucceeded.ShouldBeTrue();
    }

    [Fact]
    public void Success_ShouldSetItems_ToProvidedList()
    {
        var items = new List<ScrapedProjectData>
        {
            new("Dev", "https://x.de/1", "desc", "Acme", "Freelancermap")
        };

        var result = ScrapingResult.Success(items, 1);

        result.Items.ShouldBe(items);
    }

    [Fact]
    public void Success_ShouldSetListingItemCount()
    {
        var result = ScrapingResult.Success([], 42);

        result.ListingItemCount.ShouldBe(42);
    }

    [Fact]
    public void Success_ShouldSetErrorMessage_Null()
    {
        var result = ScrapingResult.Success([], 0);

        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void Success_ShouldSetListingItemCount_IndependentlyFromItemsCount()
    {
        // Items may be fewer than ListingItemCount when known URLs are filtered
        var result = ScrapingResult.Success([], 10);

        result.Items.ShouldBeEmpty();
        result.ListingItemCount.ShouldBe(10);
    }

    // -----------------------------------------------------------------------
    // Failure factory method
    // -----------------------------------------------------------------------

    [Fact]
    public void Failure_ShouldSetListingSucceeded_False()
    {
        var result = ScrapingResult.Failure("timeout");

        result.ListingSucceeded.ShouldBeFalse();
    }

    [Fact]
    public void Failure_ShouldSetItems_ToEmptyList()
    {
        var result = ScrapingResult.Failure("timeout");

        result.Items.ShouldBeEmpty();
    }

    [Fact]
    public void Failure_ShouldSetListingItemCount_Zero()
    {
        var result = ScrapingResult.Failure("timeout");

        result.ListingItemCount.ShouldBe(0);
    }

    [Fact]
    public void Failure_ShouldSetErrorMessage_ToProvidedMessage()
    {
        const string error = "Connection refused by host";

        var result = ScrapingResult.Failure(error);

        result.ErrorMessage.ShouldBe(error);
    }
}
