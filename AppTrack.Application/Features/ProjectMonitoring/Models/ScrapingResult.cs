namespace AppTrack.Application.Features.ProjectMonitoring.Models;

public record ScrapingResult(
    bool ListingSucceeded,
    IReadOnlyList<ScrapedProjectData> Items,
    int ListingItemCount,
    string? ErrorMessage = null)
{
    public static ScrapingResult Success(IReadOnlyList<ScrapedProjectData> items, int listingItemCount)
        => new(true, items, listingItemCount);

    public static ScrapingResult Failure(string errorMessage)
        => new(false, [], 0, errorMessage);
}
