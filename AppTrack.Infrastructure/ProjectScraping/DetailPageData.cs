namespace AppTrack.Infrastructure.ProjectScraping;

internal sealed record DetailPageData(
    string Description,
    string Location,
    string DurationInMonths,
    string StartDateText,
    string ContactPerson);
