namespace AppTrack.Application.Features.ProjectMonitoring.Models;

public record ScrapedProjectData(
    string Position,
    string Url,
    string JobDescription,
    string CompanyName,
    string PortalName);
