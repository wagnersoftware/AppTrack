# Project Scraping — Implementation Plan

**Updated:** 2026-04-26 (scope reduced from RSS feed monitoring to freelancermap.de scraping only)

**Goal:** Periodically scrape freelancermap.de and store results in `ScrapedProjects`. No user-specific matching or notifications in v1.

**Architecture:** Clean Architecture. Application defines CQRS command + repository interfaces. Infrastructure implements the HTML scraper. A `BackgroundService` in API triggers scraping and is designed to be swapped for an Azure Functions Timer Trigger without code changes.

**Full design spec:** `docs/superpowers/specs/2026-04-21-rss-feed-monitoring-design.md`

> **Note:** User-specific features (subscriptions, keyword matching, notifications) are tracked in branch `feature/project-monitoring-user-matching`.

---

## Status: Complete ✅

All tasks below are implemented on branch `feature/project-scraping`.

---

## Chunk 1: Domain & Persistence

- [x] Add `ScraperType` enum (`AppTrack.Domain/Enums/ScraperType.cs`)
- [x] Add `Discovered` value to `JobApplicationStatus` enum
- [x] Create `ProjectPortal` entity (`AppTrack.Domain/ProjectPortal.cs`)
- [x] Create `ScrapedProject` entity (`AppTrack.Domain/ScrapedProject.cs`)
- [x] Add EF Core configurations for `ProjectPortal` and `ScrapedProject`
- [x] Add `ProjectPortals` and `ScrapedProjects` DbSets to `AppTrackDatabaseContext`
- [x] Create migration `AddProjectScraping` (seeds Freelancermap portal, drops leftover user-matching tables if present)
- [x] Add `IProjectPortalRepository` and `IScrapedProjectRepository` contracts
- [x] Implement `ProjectPortalRepository` and `ScrapedProjectRepository`
- [x] Register repositories in `PersistanceServiceRegistration`

---

## Chunk 2: Application Layer

- [x] Add `ScrapedProjectData` model (`AppTrack.Application/Features/ProjectMonitoring/Models/`)
- [x] Add `IProjectScraper` and `IProjectScraperFactory` contracts
- [x] Implement `ScrapePortalsCommand` and `ScrapePortalsCommandHandler`
- [x] Implement `GetProjectPortalsQuery` and `GetProjectPortalsQueryHandler`
- [x] Add `ProjectPortalDto` (Id, Name, Url)

---

## Chunk 3: Infrastructure Layer

- [x] Implement `FreelancermapScraper` (AngleSharp HTML parsing of `.project-card` elements)
- [x] Implement `ProjectScraperFactory` (switch on `ScraperType`)
- [x] Register in `InfrastructureServicesRegistration` via `AddHttpClient<FreelancermapScraper>()`

---

## Chunk 4: API Layer

- [x] Add `ProjectMonitoringController` with `GET /api/projectmonitoring/portals`
- [x] Add `ProjectMonitoringBackgroundService` (scrapes every 15 minutes)
- [x] Register background service in `Program.cs`

---

## Chunk 5: Tests

- [x] Unit tests: `ScrapePortalsCommandHandlerTests`
- [x] Unit tests: `GetProjectPortalsQueryHandlerTests`
- [x] API integration tests: `ProjectMonitoringPortalsTests` (GET returns portals, 401 when unauthenticated)

---

## Future Work (feature/project-monitoring-user-matching)

- Per-user portal subscriptions (`UserPortalSubscription`, `SetPortalSubscriptionsCommand`)
- User monitoring settings: keywords, poll interval (`ProjectMonitoringSettings`, `UpdateProjectMonitoringSettingsCommand`)
- Keyword matching against scraped projects (`PollProjectsCommand`)
- Auto-creation of `JobApplication` with status `Discovered` on match
- Email / Azure Service Bus notifications (`IProjectMatchNotifier`)
- Blazor UI: RSS Options page with subscription toggles, keyword management, notification settings
