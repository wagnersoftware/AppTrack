# Design: Keyword-Based Project Matching & Notification Pipeline

**Date:** 2026-04-27
**Branch:** feature/project-monitoring-matching
**Status:** Approved

---

## Overview

Redesign the project assignment and notification pipeline so that keyword matching happens immediately after scraping (triggered via Service Bus), and email notifications are a separate lightweight step that reads pre-matched results from the database.

**Current flow:**
```
[Timer] ScrapePortalsFunction → store ScrapedProjects globally
[Timer] PollProjectsCommand   → per user: match + create JobApplication + notify (all in one step)
```

**New flow:**
```
[Timer]        ScrapePortalsFunction → store ScrapedProjects → publish scraping-completed signal
[Service Bus]  MatchProjectsFunction → per user: match → UserProjectMatch + JobApplication
[Timer]        SendNotificationsFunction → per user: email unnotified matches → mark IsNotified
```

---

## Data Model Changes

### New Entity: `UserProjectMatch`

```csharp
public class UserProjectMatch
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int ScrapedProjectId { get; set; }
    public ScrapedProject ScrapedProject { get; set; }
    public bool IsNotified { get; set; }
}
```

- Unique index on `(UserId, ScrapedProjectId)` — prevents duplicate matches on re-run.
- FK from `UserProjectMatch → ScrapedProject`: `DeleteBehavior.Restrict` — matches must not be cascade-deleted when a portal or scraped project is removed. Matches are user data.

### Modified: `ProjectMonitoringSettings`

Remove the following fields (obsolete in new design):
- `PollIntervalMinutes` — matching is now triggered by scrape events, not a per-user poll schedule.
- `LastPolledAt` — `ProcessedProjectItem` tracks what each user has seen; a timestamp is redundant.

This removal must be propagated to all dependent code:
- `UpdateProjectMonitoringSettingsCommand` — remove `PollIntervalMinutes` property
- `UpdateProjectMonitoringSettingsCommandValidator` — remove the `PollIntervalMinutes` rule
- `ProjectMonitoringSettingsDto` — remove `PollIntervalMinutes` from the record
- `GetProjectMonitoringSettingsQueryHandler` — remove field from DTO construction
- `ProjectMonitoringSettingsRepository.UpsertAsync` — remove mapping
- `ProjectMonitoringSettingsConfiguration` — remove column mapping
- NSwag `ServiceClient.cs` must be regenerated after API contract changes

### Modified: `ScrapedProject`

Remove `ScrapedAt` — the base class `BaseEntity` already provides `CreationDate` for this purpose. Remove from:
- `ScrapedProject` entity
- `ScrapedProjectConfiguration`
- `ScrapePortalsCommandHandler` (currently sets `ScrapedAt = DateTime.UtcNow` on construction)

### Unchanged: `ProcessedProjectItem`

Tracks which project URLs have been seen (matched or not) per user. Prevents re-processing existing projects on every scrape run. Existing data is valid carry-over — no data migration needed at cutover.

---

## Application Layer

### New Interface: `IScrapingEventPublisher`

In `Application/Contracts/ProjectMonitoring/`:

```csharp
public interface IScrapingEventPublisher
{
    Task PublishScrapingCompletedAsync(CancellationToken ct);
}
```

Keeps the Service Bus concern behind an Application-layer interface, consistent with existing patterns (`IProjectMatchNotifier`, `IEmailSender`). Implemented in `AppTrack.Infrastructure`.

### New: `MatchProjectsCommand` + Handler

**Trigger:** Dispatched by `MatchProjectsFunction` (Service Bus trigger).
**Scope:** Not user-scoped — processes all users in one execution.

**Handler logic (per user, wrapped in a transaction):**
1. Load all active `UserPortalSubscription` records (with portals), grouped by `UserId`.
2. For each user:
   a. Load `ProjectMonitoringSettings` — skip if no settings or no keywords.
   b. Call `IScrapedProjectRepository.GetUnprocessedForUserAsync(userId, portalIds, ct)` — returns `ScrapedProject` records with no entry in `ProcessedProjectItems` for this user.
   c. Run keyword matching: `project.Title.Contains(keyword, OrdinalIgnoreCase)` for any keyword (Title only, not Description — keeps existing behavior).
   d. **In a single transaction per user:**
      - For each match: create `UserProjectMatch` (IsNotified = false) and `JobApplication` (Status = Discovered).
      - Add all unseen projects (matched + unmatched) to `ProcessedProjectItems`.
   e. The unique index on `(UserId, ScrapedProjectId)` acts as a safety net against duplicate `UserProjectMatch` rows if the Service Bus message is retried. Duplicate inserts are caught and ignored (idempotent via insert-if-not-exists pattern, same approach as `AddNewForPortalAsync`).

### New: `SendProjectNotificationsCommand` + Handler

**Trigger:** Dispatched by `SendNotificationsFunction` (Timer trigger, e.g. every 5 minutes).
**Scope:** Not user-scoped — processes all eligible users in one execution.

**Handler logic:**
1. Call `IUserProjectMatchRepository.GetUnnotifiedAsync(ct)` — returns `UserProjectMatch` rows where `IsNotified = false`, with `ScrapedProject` and `ScrapedProject.ProjectPortal` eager-loaded, grouped by `UserId`. Only includes users with `NotifyByEmail = true` and a non-empty `NotificationEmail` (join with `ProjectMonitoringSettings`).
2. For each user: check `NotificationIntervalMinutes` against `LastNotifiedAt` — skip if interval not reached.
3. Build `List<ScrapedProjectData>` from matches (using `ScrapedProject.Title`, `ProjectPortal.Name`, `ScrapedProject.Url`).
4. Send email via `IEmailSender` directly (see Infrastructure section).
5. Call `IUserProjectMatchRepository.MarkNotifiedAsync(matchIds, ct)` — sets `IsNotified = true` for the sent batch.
6. Update `ProjectMonitoringSettings.LastNotifiedAt = DateTime.UtcNow`.

**Concurrency note:** If two `SendNotificationsFunction` invocations overlap (possible in Azure Functions), the same rows could be emailed twice. Mitigation: configure the function's `maxConcurrentCalls = 1` in `host.json`, which serializes timer invocations. No application-level locking is needed.

**`NotificationEmail` source:** Loaded from `ProjectMonitoringSettings.NotificationEmail` in the DB — no JWT claim needed since the command is not user-scoped.

### New Repository Interface: `IUserProjectMatchRepository`

```csharp
Task AddRangeAsync(IEnumerable<UserProjectMatch> matches, CancellationToken ct);

// Returns all IsNotified=false matches, grouped by UserId,
// with ScrapedProject and ScrapedProject.ProjectPortal eager-loaded.
// Filters to users with NotifyByEmail=true and non-empty NotificationEmail.
Task<List<UserProjectMatch>> GetUnnotifiedAsync(CancellationToken ct);

// Sets IsNotified=true for the given match IDs in a single UPDATE statement.
Task MarkNotifiedAsync(IEnumerable<int> matchIds, CancellationToken ct);
```

### Modified: `IScrapedProjectRepository`

**New method** (replaces usage of `GetByPortalIdsAsync` in matching context):
```csharp
// Returns ScrapedProjects for the given portal IDs that have no ProcessedProjectItem
// entry for the given userId (LEFT JOIN on UserId + Url).
Task<List<ScrapedProject>> GetUnprocessedForUserAsync(
    string userId,
    IEnumerable<int> portalIds,
    CancellationToken ct);
```

The existing `GetByPortalIdsAsync` method remains for other consumers (e.g. query handlers that list projects for display).

### Removed from Application Layer

- `PollProjectsCommand` + `PollProjectsCommandHandler` — logic split between the two new commands.
- `IProjectMatchNotifier` interface — no longer called by any handler; replaced by direct `IEmailSender` usage in `SendProjectNotificationsCommand`.

---

## Infrastructure Layer

### New: `ServiceBusScrapingEventPublisher`

Implements `IScrapingEventPublisher`. Sends an empty trigger message to the `scraping-completed` Service Bus queue.

**Publish failure strategy:** If the Service Bus publish fails after `ScrapePortalsCommand` completes successfully, the exception is caught, logged as a warning, and swallowed. The scraping result is not rolled back. The next scrape cycle will naturally pick up where this one left off (unmatched projects stay in `ScrapedProject` and are processed by subsequent `MatchProjectsCommand` runs, since `ProcessedProjectItems` has no entry for them yet).

### Removed from Infrastructure Layer

- `ServiceBusProjectNotifier` — Service Bus is now used only for scrape-to-match signaling.
- `DirectEmailProjectNotifier` — `SendProjectNotificationsCommand` calls `IEmailSender` directly.
- `IProjectMatchNotifier` registration block in `InfrastructureServicesRegistration` — remove the `ProjectNotification:Provider` conditional registration entirely.

### New registration in `InfrastructureServicesRegistration`

```csharp
services.AddScoped<IScrapingEventPublisher, ServiceBusScrapingEventPublisher>();
```

---

## Azure Functions Layer (`AppTrack.Functions`)

### New NuGet dependency

Add to `AppTrack.Functions.csproj` (version managed in `Directory.Packages.props`):
```
Microsoft.Azure.Functions.Worker.Extensions.ServiceBus
```

### Modified: `ScrapePortalsFunction`

After `ScrapePortalsCommand` completes, call `IScrapingEventPublisher.PublishScrapingCompletedAsync(ct)`. Failure is logged and swallowed (see Infrastructure section).

### New: `MatchProjectsFunction`

```csharp
[Function("MatchProjectsFunction")]
public async Task Run(
    [ServiceBusTrigger("%ScrapingCompletedQueueName%", Connection = "ServiceBusConnection")]
    ServiceBusReceivedMessage message,
    CancellationToken ct)
{
    await _mediator.Send(new MatchProjectsCommand(), ct);
}
```

Service Bus provides automatic retry (up to `maxDeliveryCount`, configurable on the queue) and dead-letter on repeated failure. Dead-lettered messages should be monitored via Azure Portal or Application Insights alerts.

### New: `SendNotificationsFunction`

```csharp
[Function("SendNotificationsFunction")]
public async Task Run(
    [TimerTrigger("%NotificationSchedule%")] TimerInfo timerInfo,
    CancellationToken ct)
{
    await _mediator.Send(new SendProjectNotificationsCommand(), ct);
}
```

Configured with `maxConcurrentCalls = 1` in `host.json` to prevent overlapping invocations.

---

## Configuration

### `local.settings.json` additions

```json
{
  "Values": {
    "ServiceBusConnection": "<connection-string>",
    "ScrapingCompletedQueueName": "scraping-completed",
    "NotificationSchedule": "*/5 * * * *"
  }
}
```

---

## EF Core Migrations

Three migrations are required, applied in this order:

1. **`RemoveScrapedAtFromScrapedProject`** — drop `ScrapedAt` column from `ScrapedProjects`.
2. **`RemovePollFieldsFromProjectMonitoringSettings`** — drop `PollIntervalMinutes` and `LastPolledAt` columns from `ProjectMonitoringSettings`.
3. **`AddUserProjectMatch`** — create `UserProjectMatches` table with unique index on `(UserId, ScrapedProjectId)` and FK to `ScrapedProjects` with `DeleteBehavior.Restrict`.

---

## What is Removed

| Item | Reason |
|---|---|
| `PollProjectsCommand` + Handler | Replaced by `MatchProjectsCommand` + `SendProjectNotificationsCommand` |
| `ProjectMonitoringSettings.PollIntervalMinutes` | Matching driven by scrape events |
| `ProjectMonitoringSettings.LastPolledAt` | `ProcessedProjectItem` handles seen-tracking |
| `ScrapedProject.ScrapedAt` | Covered by `BaseEntity.CreationDate` |
| `IProjectMatchNotifier` | Replaced by direct `IEmailSender` usage |
| `DirectEmailProjectNotifier` | Superseded by inline email logic in handler |
| `ServiceBusProjectNotifier` | Service Bus used only for scrape signaling, not notification payloads |
| `InfrastructureServicesRegistration` notifier block | Dead code after `IProjectMatchNotifier` removal |
