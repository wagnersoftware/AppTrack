# Design: Keyword-Based Project Matching & Notification Pipeline

**Date:** 2026-04-27
**Branch:** feature/project-monitoring-matching
**Status:** Approved

---

## Overview

Redesign the project assignment and notification pipeline so that keyword matching happens immediately after scraping, and email notifications are a separate lightweight step that reads pre-matched results from the database.

**Current flow:**
```
[Timer] ScrapePortalsFunction → store ScrapedProjects globally
[Timer] PollProjectsCommand   → per user: match + create JobApplication + notify (all in one step)
```

**New flow:**
```
[Timer]        ScrapePortalsFunction → store ScrapedProjects → publish to Service Bus
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

- Unique index on `(UserId, ScrapedProjectId)` — prevents duplicate matches on re-scrape.

### Modified: `ProjectMonitoringSettings`

Remove the following fields (obsolete in new design):
- `PollIntervalMinutes` — matching is now triggered by scrape events, not a per-user poll schedule
- `LastPolledAt` — `ProcessedProjectItem` tracks what each user has seen; a timestamp is redundant

### Modified: `ScrapedProject`

- Remove `ScrapedAt` — replaced by the existing `CreatedAt` base column.

### Unchanged: `ProcessedProjectItem`

Tracks which project URLs have been seen (matched or not) per user. Prevents re-processing existing projects on every scrape run. The `MatchProjectsCommandHandler` uses this to query only unseen projects per user.

---

## Application Layer

### New: `MatchProjectsCommand` + Handler

**Trigger:** Dispatched by `MatchProjectsFunction` (Service Bus trigger).
**Scope:** Not user-scoped — processes all users in one execution.

**Handler logic:**
1. Load all active `UserPortalSubscription` records (with portals), grouped by `UserId`.
2. For each user:
   a. Load `ProjectMonitoringSettings` — skip if no settings or no keywords.
   b. Call `IScrapedProjectRepository.GetUnprocessedForUserAsync(userId, portalIds)` — returns `ScrapedProject` records not yet in `ProcessedProjectItems` for this user.
   c. Run keyword matching: `project.Title.Contains(keyword, OrdinalIgnoreCase)` for any keyword.
   d. For each match: create `UserProjectMatch` (IsNotified = false) and `JobApplication` (Status = Discovered).
   e. Add all unseen projects (matched + unmatched) to `ProcessedProjectItems`.

### New: `SendProjectNotificationsCommand` + Handler

**Trigger:** Dispatched by `SendNotificationsFunction` (Timer trigger, e.g. every 5 minutes).
**Scope:** Not user-scoped — processes all eligible users in one execution.

**Handler logic:**
1. Load all `UserProjectMatch` records where `IsNotified = false`, grouped by `UserId` — include users with `NotifyByEmail = true` and a non-empty `NotificationEmail`.
2. For each user: check `NotificationIntervalMinutes` against `LastNotifiedAt` — skip if interval not reached.
3. Send email via `IProjectMatchNotifier` with the unnotified matches.
4. Set `IsNotified = true` on sent matches.
5. Update `ProjectMonitoringSettings.LastNotifiedAt = DateTime.UtcNow`.

### New Repository Interface: `IUserProjectMatchRepository`

```csharp
Task AddRangeAsync(IEnumerable<UserProjectMatch> matches, CancellationToken ct);
Task<List<UserProjectMatch>> GetUnnotifiedAsync(CancellationToken ct);
Task MarkNotifiedAsync(IEnumerable<int> matchIds, CancellationToken ct);
```

### Modified: `IScrapedProjectRepository`

New method:
```csharp
Task<List<ScrapedProject>> GetUnprocessedForUserAsync(
    string userId,
    IEnumerable<int> portalIds,
    CancellationToken ct);
```
Implementation: LEFT JOIN `ScrapedProjects` against `ProcessedProjectItems` on `(UserId, Url)` — returns only rows with no matching processed entry.

### Removed

- `PollProjectsCommand` + `PollProjectsCommandHandler` — logic split between the two new commands.

---

## Infrastructure Layer

### Removed

- `ServiceBusProjectNotifier` — notifications are now sent directly from `SendProjectNotificationsCommand` via `DirectEmailProjectNotifier`. The Service Bus queue is used only for scrape-to-match signaling, not for notification payloads.

---

## Azure Functions Layer

### Modified: `ScrapePortalsFunction`

After `ScrapePortalsCommand` completes successfully, publish an empty trigger message to the `scraping-completed` Service Bus queue.

### New: `MatchProjectsFunction`

```csharp
[Function("MatchProjectsFunction")]
public async Task Run(
    [ServiceBusTrigger("%ScrapingCompletedQueueName%", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message,
    CancellationToken ct)
{
    await _mediator.Send(new MatchProjectsCommand(), ct);
}
```

Service Bus provides automatic retry (3×) and dead-letter on repeated failure.

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

Runs frequently (e.g. every 5 minutes). The per-user `NotificationIntervalMinutes` check inside the handler ensures emails are not sent more often than configured.

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

## What is Removed

| Item | Reason |
|---|---|
| `PollProjectsCommand` + Handler | Replaced by `MatchProjectsCommand` + `SendProjectNotificationsCommand` |
| `ProjectMonitoringSettings.PollIntervalMinutes` | Matching driven by scrape events |
| `ProjectMonitoringSettings.LastPolledAt` | `ProcessedProjectItem` handles seen-tracking |
| `ScrapedProject.ScrapedAt` | Covered by `CreatedAt` base column |
| `ServiceBusProjectNotifier` | Service Bus used only for scrape signaling, not notification payloads |

---

## Migration Notes

- EF Core migration required for: new `UserProjectMatch` table, dropped columns on `ProjectMonitoringSettings` and `ScrapedProject`.
- Existing `ProcessedProjectItem` data is valid and can be retained.
- Existing `ScrapedProject` data is valid; migration drops only the `ScrapedAt` column.
