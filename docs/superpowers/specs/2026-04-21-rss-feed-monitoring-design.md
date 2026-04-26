# Project Scraping — Design Spec

**Date:** 2026-04-21 (updated 2026-04-26)
**Branch:** feature/project-scraping
**Status:** Implemented

---

## Overview

AppTrack periodically scrapes the freelancermap.de project portal and stores the results in a central `ScrapedProjects` table. The scraping trigger is designed for **cloud portability**: it runs as an embedded `BackgroundService` in development and can be replaced by an Azure Functions Timer Trigger in production without changes to the Application or Infrastructure layers.

User-specific features (keyword matching, per-user portal subscriptions, email notifications, auto-creation of `JobApplication` entries from matches) are intentionally **out of scope for v1** and will be implemented in a separate branch (`feature/project-monitoring-user-matching`).

---

## Requirements

| # | Requirement |
|---|---|
| R1 | The system provides a fixed list of project portals (name + URL). |
| R2 | A background service periodically scrapes all active portals. |
| R3 | Scraped results are persisted in a `ScrapedProjects` table (replacing previous results per portal on each run). |
| R4 | The scraping trigger (`BackgroundService` vs Azure Function) is swappable without Application or Infrastructure changes. |
| R5 | Each portal has a dedicated scraper type; adding a new scraper requires only a new `ScraperType` enum value and implementation. |
| R6 | `GET /api/projectmonitoring/portals` returns all active portals (read-only, no user-specific data). |

---

## Domain Model

### New Entities (inherit `BaseEntity`)

#### `ScraperType` — enum (Domain)
```csharp
public enum ScraperType
{
    FreelancerMap
}
```

#### `ProjectPortal` — system-managed
```csharp
public class ProjectPortal : BaseEntity
{
    public string Name { get; set; }        // e.g. "Freelancermap"
    public string Url { get; set; }         // portal URL to scrape
    public ScraperType ScraperType { get; set; }
    public bool IsActive { get; set; }
}
```
Populated via EF Core seed migration. Not user-editable.

#### `ScrapedProject` — scraping result
```csharp
public class ScrapedProject : BaseEntity
{
    public int ProjectPortalId { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public string CompanyName { get; set; }
    public DateTime ScrapedAt { get; set; }
    public ProjectPortal ProjectPortal { get; set; }
}
```

---

## Application Layer

### Contracts
```
AppTrack.Application/Contracts/ProjectMonitoring/
├── IProjectPortalRepository.cs     — GetAllActiveAsync()
├── IProjectScraper.cs              — ScrapeAsync(url, ct) → List<ScrapedProjectData>
├── IProjectScraperFactory.cs       — GetScraper(scraperType) → IProjectScraper
└── IScrapedProjectRepository.cs    — ReplaceForPortalAsync(portalId, projects, ct)
```

**`ScrapedProjectData`** (Application model, returned by scrapers):
```csharp
public record ScrapedProjectData(
    string Position,
    string Url,
    string JobDescription,
    string CompanyName,
    string PortalName);
```

### Commands

#### `ScrapePortalsCommand` *(internal — not exposed via API)*
1. Load all active `ProjectPortal` entries.
2. For each portal: call `IProjectScraperFactory.GetScraper(portal.ScraperType).ScrapeAsync(portal.Url, ct)`.
3. Replace stored `ScrapedProject` records for that portal via `IScrapedProjectRepository.ReplaceForPortalAsync`.

### Queries

#### `GetProjectPortalsQuery` → `List<ProjectPortalDto>`
Returns all active portals. Response DTO:
```csharp
public record ProjectPortalDto(int Id, string Name, string Url);
```

---

## Infrastructure Layer

```
AppTrack.Infrastructure/
└── ProjectScraping/
    ├── FreelancermapScraper.cs     — HttpClient + AngleSharp HTML parsing
    └── ProjectScraperFactory.cs   — selects scraper by ScraperType
```

### FreelancermapScraper

Uses AngleSharp to parse HTML from freelancermap.de. Selects `.project-card` elements and extracts title, URL, and company name.

---

## API Layer

### Controller: `ProjectMonitoringController`

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/projectmonitoring/portals` | Returns all active portals |

### BackgroundService: `ProjectMonitoringBackgroundService`

```csharp
public class ProjectMonitoringBackgroundService(IServiceScopeFactory scopeFactory, ILogger<...> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new ScrapePortalsCommand(), stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
```

**Azure Functions swap:** Replace with a Timer Trigger that resolves `IMediator` from DI and calls `mediator.Send(new ScrapePortalsCommand())`. No Application or Infrastructure changes needed.

---

## Cloud Portability Summary

| Concern | Dev | Production |
|---------|-----|------------|
| Scrape trigger | `ProjectMonitoringBackgroundService` (embedded) | Azure Functions Timer Trigger |
| Configuration | `appsettings.Development.json` | Azure Key Vault |

---

## Testing Strategy

### Unit Tests
- **`ScrapePortalsCommandHandlerTests`**: mock `IProjectPortalRepository`, `IProjectScraperFactory`, `IScrapedProjectRepository`. Verify scraper is called for each active portal and results are persisted.
- **`GetProjectPortalsQueryHandlerTests`**: mock `IProjectPortalRepository`. Verify correct DTO mapping.

### API Integration Tests
- **`ProjectMonitoringPortalsTests`**: `GET /api/projectmonitoring/portals` returns seeded portal, requires auth.

---

## Out of Scope (v1)

All user-specific features are deferred to `feature/project-monitoring-user-matching`:

- Per-user portal subscriptions
- Keyword matching and filtering
- Auto-creation of `JobApplication` entries for matches
- Email / Service Bus notifications
- Per-user poll intervals and settings
- Admin UI for managing portals
