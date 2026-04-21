# RSS Feed Monitoring — Design Spec

**Date:** 2026-04-21
**Branch:** feature/structured-logging (to be continued on a dedicated feature branch)
**Status:** Approved by user

---

## Overview

AppTrack shall monitor predefined job-portal RSS feeds on a per-user schedule. When a feed item matches a user's configured keywords, a `JobApplication` with status `Discovered` is automatically created. The user then decides whether to pursue the application. A notification email summarising new matches is sent after each poll run.

The feature is designed for **cloud portability**: the background polling can run as an embedded `BackgroundService` (development) or be replaced by an Azure Functions Timer Trigger (production) without changes to the Application or Infrastructure layers. Email notification is similarly decoupled via a configurable `IRssMatchNotifier` abstraction.

---

## Requirements

| # | Requirement |
|---|---|
| R1 | The system provides a fixed list of job portals (name + RSS URL). Users cannot add custom URLs. |
| R2 | Each user can activate/deactivate individual portals from the system list. |
| R3 | Each user configures a global keyword list (case-insensitive OR match on title and description). |
| R4 | Each user configures a poll interval (minutes). |
| R5 | A background service polls due feeds automatically. |
| R6 | Feed items already processed for a user are not reprocessed (deduplication by URL). |
| R7 | A matching feed item creates a `JobApplication` with status `Discovered`. |
| R8 | After each poll run with matches, one summary email is sent to the user. |
| R9 | The notification mechanism is configurable per environment (`Direct` or `ServiceBus`). |
| R10 | Each portal can have a dedicated parser; a `Default` parser handles unspecified portals. |
| R11 | The poll trigger (BackgroundService vs Azure Function) is swappable without Application changes. |

---

## Domain Model

### New Entities (all inherit `BaseEntity`)

#### `RssPortal` — system-managed
```csharp
public class RssPortal : BaseEntity
{
    public string Name { get; set; }       // e.g. "Stepstone"
    public string Url { get; set; }        // RSS feed URL
    public string ParserType { get; set; } // e.g. "Stepstone", "Default"
    public bool IsActive { get; set; }     // system-level kill switch
}
```
Populated via EF Core seed migration. Not exposed for user editing.

#### `UserRssSubscription` — user activation of a portal
```csharp
public class UserRssSubscription : BaseEntity
{
    public string UserId { get; set; }
    public int RssPortalId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastPolledAt { get; set; }
    public RssPortal RssPortal { get; set; }
}
```
Unique index: `(UserId, RssPortalId)`.

#### `RssMonitoringSettings` — global user preferences
```csharp
public class RssMonitoringSettings : BaseEntity
{
    public string UserId { get; set; }
    public List<string> Keywords { get; set; } = [];
    public int PollIntervalMinutes { get; set; } // e.g. 60
}
```
One record per user. Created on first save.

**EF Core mapping for `Keywords`:** configured via `HasConversion` with `JsonSerializer`:
```csharp
builder.Property(x => x.Keywords)
    .HasColumnType("nvarchar(max)")
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? []);
```

#### `ProcessedFeedItem` — deduplication log
```csharp
public class ProcessedFeedItem : BaseEntity
{
    public string UserId { get; set; }
    public string FeedItemUrl { get; set; }
    public DateTime ProcessedAt { get; set; }
}
```
Unique index: `(UserId, FeedItemUrl)`.

### Modified Enum

`JobApplicationStatus` gains a new value:
```csharp
public enum JobApplicationStatus
{
    // existing values ...
    Discovered  // automatically created from RSS match
}
```

---

## Application Layer

### Contracts (interfaces consumed by handlers)

```
AppTrack.Application/Contracts/RssFeed/
├── IRssPortalRepository.cs
├── IUserRssSubscriptionRepository.cs
├── IRssMonitoringSettingsRepository.cs
├── IProcessedFeedItemRepository.cs
├── IRssFeedReader.cs          — ReadAsync(url) → List<RawFeedItem>
├── IRssFeedItemParser.cs      — Parse(item, parserType, portalName) → RssJobApplicationData
└── IRssMatchNotifier.cs       — NotifyAsync(userId, matches)
```

**Note on parser architecture:** `IRssFeedItemParser` is the only parser-related interface visible to Application. The internal `IRssFeedParser` strategy and `RssFeedParserFactory` are Infrastructure-internal details — Application only calls the single `IRssFeedItemParser.Parse(...)` method and receives `RssJobApplicationData`. This keeps the parser strategy pattern fully contained in Infrastructure.

#### Key types (defined in Application)
```csharp
public record RawFeedItem(string Title, string Url, string Description, DateTime? PublishedAt);

public record RssJobApplicationData(
    string Position,       // job title
    string Url,            // link to posting
    string JobDescription, // HTML-stripped description
    string CompanyName,    // extracted if available, otherwise empty string
    string PortalName);    // e.g. "Stepstone" — used as fallback for JobApplication.Name

public interface IRssFeedReader
{
    Task<List<RawFeedItem>> ReadAsync(string feedUrl, CancellationToken ct);
}

public interface IRssFeedItemParser
{
    RssJobApplicationData Parse(RawFeedItem item, string parserType, string portalName);
}

public interface IRssMatchNotifier
{
    Task NotifyAsync(string userId, List<RssJobApplicationData> matches, CancellationToken ct);
}
```

#### `JobApplication.Name` for discovered applications
`JobApplication.Name` holds the company name. Since RSS feeds rarely provide a reliable company name, the handler sets:
```
Name = CompanyName if non-empty, otherwise PortalName
```
This satisfies the required/max-length constraint while remaining honest about the data source.

### Commands

#### `UpdateRssMonitoringSettingsCommand`
Updates keywords and poll interval for the authenticated user (`IUserScopedRequest`).

#### `SetRssSubscriptionsCommand`
Activates or deactivates a set of portals for the authenticated user (`IUserScopedRequest`).

#### `PollRssFeedsCommand` *(internal — not exposed via API, no `IUserScopedRequest`)*

**Handler logic (`PollRssFeedsCommandHandler`):**
1. Load all `UserRssSubscription` where `IsActive = true` and `now >= LastPolledAt + PollIntervalMinutes` (or `LastPolledAt` is null), including `RssPortal` navigation.
2. Group by user.
3. For each user:
   a. Load `RssMonitoringSettings` (keywords, interval).
   b. For each active subscription:
      - Read feed via `IRssFeedReader`.
      - Parse each item via `IRssFeedItemParser.Parse(item, portal.ParserType, portal.Name)`.
      - Filter out URLs already present in `ProcessedFeedItem` for this user.
      - Apply keyword matching: case-insensitive, OR logic, on `RawFeedItem.Title + Description`.
   c. **Within a single transaction** (via `IUnitOfWork.ExecuteInTransactionAsync`):
      - Create `JobApplication` for each match (Status = `Discovered`, Name = company or portal name).
      - Insert corresponding `ProcessedFeedItem` records.
      - Update `UserRssSubscription.LastPolledAt = DateTime.UtcNow`.
   d. After the transaction commits: if matches were found, call `IRssMatchNotifier.NotifyAsync`.

Keeping the notification call outside the transaction ensures a DB failure does not silently suppress the notification, and a notification failure does not roll back persisted data.

### Queries

#### `GetRssPortalsQuery` → `List<RssPortalDto>`
Returns all active `RssPortal` entries with subscription status for the authenticated user.

#### `GetRssMonitoringSettingsQuery` → `RssMonitoringSettingsDto`
Returns keywords and poll interval for the authenticated user.

#### Response DTOs
```csharp
public record RssPortalDto(int Id, string Name, bool IsSubscribed);

public record RssMonitoringSettingsDto(List<string> Keywords, int PollIntervalMinutes);
```

---

## Infrastructure Layer

```
AppTrack.Infrastructure/
├── RssFeed/
│   ├── RssFeedReader.cs              — HTTP client + CodeHollow.FeedReader NuGet package
│   ├── RssFeedItemParser.cs          — implements IRssFeedItemParser; delegates to internal parsers
│   ├── Parsers/                       — Infrastructure-internal, not visible to Application
│   │   ├── IRssFeedParser.cs         — internal interface: Parse(RawFeedItem, portalName) → RssJobApplicationData
│   │   ├── RssFeedParserFactory.cs   — selects parser by parserType string
│   │   ├── DefaultFeedParser.cs
│   │   └── StepstoneFeedParser.cs
├── Notifications/
│   ├── DirectEmailNotifier.cs        — delegates to IEmailSender (SendGrid)
│   └── ServiceBusNotifier.cs         — publishes JSON message to Azure Service Bus queue
```

### Default field mapping (`DefaultFeedParser`)

| `RssJobApplicationData` field | Source |
|---|---|
| `Position` | `RawFeedItem.Title` |
| `Url` | `RawFeedItem.Url` |
| `JobDescription` | `RawFeedItem.Description` (HTML stripped) |
| `CompanyName` | `""` (not reliably available in generic RSS) |
| `PortalName` | passed in by caller |

Portal-specific parsers override fields where the portal provides richer structured data.

### Notification Configuration

`appsettings.Development.json`:
```json
"RssNotification": { "Provider": "Direct" }
```

`appsettings.Production.json`:
```json
"RssNotification": {
  "Provider": "ServiceBus",
  "QueueName": "rss-matches"
}
```

DI registration in `InfrastructureServicesRegistration`:
```csharp
var provider = configuration["RssNotification:Provider"];
if (provider == "ServiceBus")
    services.AddScoped<IRssMatchNotifier, ServiceBusNotifier>();
else
    services.AddScoped<IRssMatchNotifier, DirectEmailNotifier>();
```

### New NuGet packages required

Add to `AppTrack.Infrastructure.csproj`:
```xml
<PackageReference Include="CodeHollow.FeedReader" Version="1.2.6" />
<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.18.4" />
```

`Azure.Messaging.ServiceBus` is only required when `ServiceBusNotifier` is in scope; it may be conditionally omitted in a purely dev-targeted build if desired.

---

## Persistence Layer

New repositories (all follow existing `GenericRepository<T>` pattern):
- `RssPortalRepository` : `IRssPortalRepository`
- `UserRssSubscriptionRepository` : `IUserRssSubscriptionRepository`
- `RssMonitoringSettingsRepository` : `IRssMonitoringSettingsRepository`
- `ProcessedFeedItemRepository` : `IProcessedFeedItemRepository`

New `DbSet` entries in `AppTrackDatabaseContext`:
```csharp
public DbSet<RssPortal> RssPortals { get; set; }
public DbSet<UserRssSubscription> UserRssSubscriptions { get; set; }
public DbSet<RssMonitoringSettings> RssMonitoringSettings { get; set; }
public DbSet<ProcessedFeedItem> ProcessedFeedItems { get; set; }
```

EF Core migration includes:
- Unique indexes on `UserRssSubscription(UserId, RssPortalId)` and `ProcessedFeedItem(UserId, FeedItemUrl)`.
- `HasConversion` for `RssMonitoringSettings.Keywords` (see Domain Model section).
- Seed data for initial `RssPortal` entries (at minimum: one Stepstone entry).

---

## API Layer

### Controller: `RssFeedController`

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/rssfeeds/portals` | Returns portals with user subscription status |
| `PUT` | `/api/rssfeeds/subscriptions` | Activate/deactivate portals |
| `GET` | `/api/rssfeeds/settings` | Returns user's keywords + interval |
| `PUT` | `/api/rssfeeds/settings` | Updates keywords + interval |

All endpoints require JWT authentication (global policy). `UserId` injected from claims via `IUserScopedRequest`.

### BackgroundService: `RssFeedBackgroundService`

`BackgroundService` is a singleton in ASP.NET Core. Because `IMediator` is registered as scoped, the service must **not** constructor-inject `IMediator` directly — doing so would be caught immediately by `ValidateOnBuild = true` and crash startup.

Instead, inject `IServiceScopeFactory` and create a scope per poll iteration:

```csharp
public class RssFeedBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new PollRssFeedsCommand(), ct);
            await Task.Delay(TimeSpan.FromMinutes(5), ct);
        }
    }
}
```

Registered in `Program.cs`:
```csharp
builder.Services.AddHostedService<RssFeedBackgroundService>();
```

**Azure Functions swap:** Replace `RssFeedBackgroundService` with an Azure Functions Timer Trigger. The function resolves `IMediator` from the DI container (scoped per invocation by default in Azure Functions isolated worker) and calls `mediator.Send(new PollRssFeedsCommand())`. No Application or Infrastructure changes required.

---

## Cloud Portability Summary

| Concern | Dev | Production |
|---------|-----|------------|
| Poll trigger | `RssFeedBackgroundService` (embedded) | Azure Functions Timer Trigger |
| Email notification | `DirectEmailNotifier` → SendGrid | `ServiceBusNotifier` → Azure Service Bus → separate consumer |
| Configuration | `appsettings.Development.json` | `appsettings.Production.json` + Azure Key Vault |

---

## Testing Strategy

- **Unit tests** for `PollRssFeedsCommandHandler`: mock all repositories, `IRssFeedReader`, `IRssFeedItemParser`, `IRssMatchNotifier`, `IUnitOfWork`. Verify correct `JobApplication` creation, deduplication, transaction usage, notification dispatch only on matches.
- **Unit tests** for `DefaultFeedParser` and `StepstoneFeedParser`: fixed `RawFeedItem` input → expected `RssJobApplicationData` output.
- **Unit tests** for `UpdateRssMonitoringSettingsCommand` and `SetRssSubscriptionsCommand` validators.
- **Persistence integration tests** for unique index enforcement on `ProcessedFeedItem(UserId, FeedItemUrl)` and `UserRssSubscription(UserId, RssPortalId)`.

---

## Out of Scope (v1)

- Admin UI for managing `RssPortal` entries (use seed migrations).
- Cleanup job for old `ProcessedFeedItem` records.
- Per-portal keyword overrides.
- In-app notification (push/SignalR).
- Retry logic for failed feed reads.
