# Keyword-Based Project Matching & Notification Pipeline — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Decouple keyword matching from scraping (via Service Bus) and from email notifications (via a separate timer function), replacing the monolithic `PollProjectsCommand` with two focused commands.

**Architecture:** After scraping, `ScrapePortalsFunction` publishes a signal to a Service Bus queue. `MatchProjectsFunction` consumes it, runs keyword matching for all users, creates `UserProjectMatch` and `JobApplication` records. A separate `SendNotificationsFunction` runs on a timer, reads unnotified matches, and sends emails.

**Tech Stack:** .NET 10, EF Core 10, Azure Functions v4 Isolated Worker, Azure Service Bus (`Azure.Messaging.ServiceBus`), MediatR, FluentValidation, xUnit + Moq + Shouldly.

---

## File Map

### New files
| File | Responsibility |
|---|---|
| `AppTrack.Domain/UserProjectMatch.cs` | New domain entity |
| `AppTrack.Application/Contracts/ProjectMonitoring/IScrapingEventPublisher.cs` | Contract for Service Bus publish |
| `AppTrack.Application/Contracts/ProjectMonitoring/IUserProjectMatchRepository.cs` | Repository contract |
| `AppTrack.Application/Features/ProjectMonitoring/Commands/MatchProjects/MatchProjectsCommand.cs` | Command marker |
| `AppTrack.Application/Features/ProjectMonitoring/Commands/MatchProjects/MatchProjectsCommandHandler.cs` | Per-user matching logic |
| `AppTrack.Application/Features/ProjectMonitoring/Commands/SendProjectNotifications/SendProjectNotificationsCommand.cs` | Command marker |
| `AppTrack.Application/Features/ProjectMonitoring/Commands/SendProjectNotifications/SendProjectNotificationsCommandHandler.cs` | Email send logic |
| `AppTrack.Persistance/Configurations/UserProjectMatchConfiguration.cs` | EF Core mapping |
| `AppTrack.Persistance/Repositories/UserProjectMatchRepository.cs` | Repository implementation |
| `AppTrack.Infrastructure/Notifications/ServiceBusScrapingEventPublisher.cs` | Service Bus publisher |
| `AppTrack.Functions/MatchProjectsFunction.cs` | Service Bus triggered function |
| `AppTrack.Functions/SendNotificationsFunction.cs` | Timer triggered function |
| `AppTrack.Application.UnitTests/Features/ProjectMonitoring/Commands/MatchProjectsCommandHandlerTests.cs` | Unit tests |
| `AppTrack.Application.UnitTests/Features/ProjectMonitoring/Commands/SendProjectNotificationsCommandHandlerTests.cs` | Unit tests |

### Modified files
| File | Change |
|---|---|
| `AppTrack.Domain/ScrapedProject.cs` | Remove `ScrapedAt` |
| `AppTrack.Domain/ProjectMonitoringSettings.cs` | Remove `PollIntervalMinutes`, `LastPolledAt` |
| `AppTrack.Application/Contracts/ProjectMonitoring/IScrapedProjectRepository.cs` | Add `GetUnprocessedForUserAsync` |
| `AppTrack.Application/Features/ProjectMonitoring/Commands/UpdateProjectMonitoringSettings/UpdateProjectMonitoringSettingsCommand.cs` | Remove `PollIntervalMinutes` |
| `AppTrack.Application/Features/ProjectMonitoring/Commands/UpdateProjectMonitoringSettings/UpdateProjectMonitoringSettingsCommandValidator.cs` | Remove `PollIntervalMinutes` rule |
| `AppTrack.Application/Features/ProjectMonitoring/Commands/UpdateProjectMonitoringSettings/UpdateProjectMonitoringSettingsCommandHandler.cs` | Remove `PollIntervalMinutes` mapping |
| `AppTrack.Application/Features/ProjectMonitoring/Dto/ProjectMonitoringSettingsDto.cs` | Remove `PollIntervalMinutes` |
| `AppTrack.Application/Features/ProjectMonitoring/Queries/GetProjectMonitoringSettings/GetProjectMonitoringSettingsQueryHandler.cs` | Remove `PollIntervalMinutes` from DTO construction |
| `AppTrack.Persistance/Configurations/ScrapedProjectConfiguration.cs` | Remove `ScrapedAt` column mapping |
| `AppTrack.Persistance/Repositories/ScrapedProjectRepository.cs` | Add `GetUnprocessedForUserAsync` |
| `AppTrack.Persistance/Repositories/ProjectMonitoringSettingsRepository.cs` | Remove `PollIntervalMinutes` mapping |
| `AppTrack.Persistance/DatabaseContext/AppTrackDatabaseContext.cs` | Add `UserProjectMatches` DbSet |
| `AppTrack.Persistance/PersistanceServiceRegistration.cs` | Register `IUserProjectMatchRepository` |
| `AppTrack.Infrastructure/InfrastructureServicesRegistration.cs` | Remove notifier block, add `IScrapingEventPublisher` |
| `AppTrack.Infrastructure/AppTrack.Infrastructure.csproj` | No change needed (`Azure.Messaging.ServiceBus` already in `Directory.Packages.props`) |
| `AppTrack.Functions/ScrapePortalsFunction.cs` | Inject + call `IScrapingEventPublisher` after scrape |
| `AppTrack.Functions/AppTrack.Functions.csproj` | Add `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus` |
| `AppTrack.Functions/host.json` | Add `extensions.serviceBus.maxConcurrentCalls = 1` for timer function |
| `AppTrack.Functions/local.settings.json` | Add `ServiceBusConnection`, `ScrapingCompletedQueueName`, `NotificationSchedule` |
| `Directory.Packages.props` | Add `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus` version |
| `AppTrack.Application.UnitTests/Features/ProjectMonitoring/Commands/ScrapePortalsCommandHandlerTests.cs` | Remove assertion on `ScrapedAt` |

### Deleted files
| File | Reason |
|---|---|
| `AppTrack.Application/Features/ProjectMonitoring/Commands/PollProjects/PollProjectsCommand.cs` | Replaced |
| `AppTrack.Application/Features/ProjectMonitoring/Commands/PollProjects/PollProjectsCommandHandler.cs` | Replaced |
| `AppTrack.Application/Contracts/ProjectMonitoring/IProjectMatchNotifier.cs` | No longer used |
| `AppTrack.Infrastructure/Notifications/DirectEmailProjectNotifier.cs` | No longer used |
| `AppTrack.Infrastructure/Notifications/ServiceBusProjectNotifier.cs` | No longer used |

---

## Chunk 1: Domain & Schema Cleanup

### Task 1: Remove `ScrapedAt` from `ScrapedProject`

**Files:**
- Modify: `AppTrack.Domain/ScrapedProject.cs`
- Modify: `AppTrack.Persistance/Configurations/ScrapedProjectConfiguration.cs`
- Modify: `AppTrack.Application/Features/ProjectMonitoring/Commands/ScrapePortals/ScrapePortalsCommandHandler.cs`
- Modify: `AppTrack.Application.UnitTests/Features/ProjectMonitoring/Commands/ScrapePortalsCommandHandlerTests.cs`

- [ ] **Remove `ScrapedAt` property from `ScrapedProject`**

  In `AppTrack.Domain/ScrapedProject.cs`, delete line:
  ```csharp
  public DateTime ScrapedAt { get; set; }
  ```

- [ ] **Remove `ScrapedAt` assignment in `ScrapePortalsCommandHandler`**

  In `ScrapePortalsCommandHandler.cs`, remove `ScrapedAt = DateTime.UtcNow` from the `new ScrapedProject { ... }` initializer.

- [ ] **Build to confirm no references to `ScrapedAt` remain**

  Run: `dotnet build AppTrack.sln --configuration Release`
  Expected: 0 errors, 0 warnings.

- [ ] **Run existing scraping tests to confirm they still pass**

  Run: `dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~ScrapePortalsCommandHandlerTests" --configuration Release`
  Expected: All pass.

- [ ] **Commit**

  ```bash
  git add AppTrack.Domain/ScrapedProject.cs \
          AppTrack.Application/Features/ProjectMonitoring/Commands/ScrapePortals/ScrapePortalsCommandHandler.cs
  git commit -m "refactor: remove ScrapedAt from ScrapedProject (covered by CreationDate)"
  ```

---

### Task 2: Remove `PollIntervalMinutes` and `LastPolledAt` from `ProjectMonitoringSettings`

**Files:**
- Delete: `AppTrack.Application/Features/ProjectMonitoring/Commands/PollProjects/PollProjectsCommand.cs`
- Delete: `AppTrack.Application/Features/ProjectMonitoring/Commands/PollProjects/PollProjectsCommandHandler.cs`
- Modify: `AppTrack.Domain/ProjectMonitoringSettings.cs`
- Modify: `AppTrack.Persistance/Configurations/ProjectMonitoringSettingsConfiguration.cs`
- Modify: `AppTrack.Application/Features/ProjectMonitoring/Commands/UpdateProjectMonitoringSettings/UpdateProjectMonitoringSettingsCommand.cs`
- Modify: `AppTrack.Application/Features/ProjectMonitoring/Commands/UpdateProjectMonitoringSettings/UpdateProjectMonitoringSettingsCommandValidator.cs`
- Modify: `AppTrack.Application/Features/ProjectMonitoring/Commands/UpdateProjectMonitoringSettings/UpdateProjectMonitoringSettingsCommandHandler.cs`
- Modify: `AppTrack.Application/Features/ProjectMonitoring/Dto/ProjectMonitoringSettingsDto.cs`
- Modify: `AppTrack.Application/Features/ProjectMonitoring/Queries/GetProjectMonitoringSettings/GetProjectMonitoringSettingsQueryHandler.cs`
- Modify: `AppTrack.Persistance/Repositories/ProjectMonitoringSettingsRepository.cs`

- [ ] **Delete `PollProjectsCommand` and `PollProjectsCommandHandler` first**

  `PollProjectsCommandHandler` references `settings.PollIntervalMinutes` and `settings.LastPolledAt` — it must be removed before those fields are deleted from the domain entity, or the build will fail.

  ```bash
  git rm "AppTrack.Application/Features/ProjectMonitoring/Commands/PollProjects/PollProjectsCommand.cs"
  git rm "AppTrack.Application/Features/ProjectMonitoring/Commands/PollProjects/PollProjectsCommandHandler.cs"
  ```

- [ ] **Remove fields from domain entity**

  In `ProjectMonitoringSettings.cs`, delete:
  ```csharp
  public int PollIntervalMinutes { get; set; } = 60;
  public DateTime? LastPolledAt { get; set; }
  ```

- [ ] **Remove `PollIntervalMinutes` from `UpdateProjectMonitoringSettingsCommand`**

  Delete:
  ```csharp
  public int PollIntervalMinutes { get; set; }
  ```

- [ ] **Remove `PollIntervalMinutes` validation rule**

  In `UpdateProjectMonitoringSettingsCommandValidator.cs`, delete:
  ```csharp
  RuleFor(x => x.PollIntervalMinutes).InclusiveBetween(5, 1440);
  ```

- [ ] **Remove `PollIntervalMinutes` from `ProjectMonitoringSettingsDto`**

  Replace the record with:
  ```csharp
  public record ProjectMonitoringSettingsDto(List<string> Keywords, int NotificationIntervalMinutes, bool NotifyByEmail);
  ```

- [ ] **Update `GetProjectMonitoringSettingsQueryHandler`**

  Update both DTO constructor calls to remove `PollIntervalMinutes`:
  ```csharp
  return settings is null
      ? new ProjectMonitoringSettingsDto([], 60, false)
      : new ProjectMonitoringSettingsDto(settings.Keywords, settings.NotificationIntervalMinutes, settings.NotifyByEmail);
  ```

- [ ] **Remove `PollIntervalMinutes` mapping from `ProjectMonitoringSettingsRepository.UpsertAsync`**

  Delete:
  ```csharp
  existing.PollIntervalMinutes = settings.PollIntervalMinutes;
  ```

- [ ] **Remove `PollIntervalMinutes` mapping from `UpdateProjectMonitoringSettingsCommandHandler`**

  Open `UpdateProjectMonitoringSettingsCommandHandler.cs` and remove `PollIntervalMinutes` from the settings mapping.

- [ ] **Remove `PollIntervalMinutes` and `LastPolledAt` from `ProjectMonitoringSettingsConfiguration`**

  In `AppTrack.Persistance/Configurations/ProjectMonitoringSettingsConfiguration.cs`, delete lines:
  ```csharp
  builder.Property(x => x.PollIntervalMinutes).IsRequired().HasDefaultValue(60);
  builder.Property(x => x.LastPolledAt).HasColumnType("datetime2");
  ```

- [ ] **Build to confirm clean**

  Run: `dotnet build AppTrack.sln --configuration Release`
  Expected: 0 errors, 0 warnings.

- [ ] **Commit**

  ```bash
  git add AppTrack.Domain/ProjectMonitoringSettings.cs \
          AppTrack.Persistance/Configurations/ProjectMonitoringSettingsConfiguration.cs \
          AppTrack.Application/Features/ProjectMonitoring/Commands/UpdateProjectMonitoringSettings/UpdateProjectMonitoringSettingsCommand.cs \
          AppTrack.Application/Features/ProjectMonitoring/Commands/UpdateProjectMonitoringSettings/UpdateProjectMonitoringSettingsCommandValidator.cs \
          AppTrack.Application/Features/ProjectMonitoring/Dto/ProjectMonitoringSettingsDto.cs \
          AppTrack.Application/Features/ProjectMonitoring/Queries/GetProjectMonitoringSettings/GetProjectMonitoringSettingsQueryHandler.cs \
          AppTrack.Persistance/Repositories/ProjectMonitoringSettingsRepository.cs
  git commit -m "refactor: remove PollIntervalMinutes and LastPolledAt (matching now event-driven)"
  ```

---

### Task 3: Add `UserProjectMatch` domain entity

**Files:**
- Create: `AppTrack.Domain/UserProjectMatch.cs`
- Create: `AppTrack.Persistance/Configurations/UserProjectMatchConfiguration.cs`
- Modify: `AppTrack.Persistance/DatabaseContext/AppTrackDatabaseContext.cs`

- [ ] **Create domain entity**

  Create `AppTrack.Domain/UserProjectMatch.cs`:
  ```csharp
  using AppTrack.Domain.Common;

  namespace AppTrack.Domain;

  public class UserProjectMatch : BaseEntity
  {
      public string UserId { get; set; } = string.Empty;
      public int ScrapedProjectId { get; set; }
      public ScrapedProject ScrapedProject { get; set; } = null!;
      public bool IsNotified { get; set; }
  }
  ```

- [ ] **Create EF Core configuration**

  Create `AppTrack.Persistance/Configurations/UserProjectMatchConfiguration.cs`:
  ```csharp
  using AppTrack.Domain;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Metadata.Builders;

  namespace AppTrack.Persistance.Configurations;

  public class UserProjectMatchConfiguration : IEntityTypeConfiguration<UserProjectMatch>
  {
      public void Configure(EntityTypeBuilder<UserProjectMatch> builder)
      {
          builder.ToTable("UserProjectMatches");
          builder.Property(x => x.UserId).IsRequired().HasMaxLength(450);
          builder.HasIndex(x => new { x.UserId, x.ScrapedProjectId }).IsUnique();
          builder.HasOne(x => x.ScrapedProject)
              .WithMany()
              .HasForeignKey(x => x.ScrapedProjectId)
              .OnDelete(DeleteBehavior.Restrict);
      }
  }
  ```

- [ ] **Register DbSet in `AppTrackDatabaseContext`**

  Add after the `ScrapedProjects` DbSet:
  ```csharp
  public DbSet<UserProjectMatch> UserProjectMatches { get; set; }
  ```

- [ ] **Build to confirm clean**

  Run: `dotnet build AppTrack.sln --configuration Release`
  Expected: 0 errors, 0 warnings.

- [ ] **Commit**

  ```bash
  git add AppTrack.Domain/UserProjectMatch.cs \
          AppTrack.Persistance/Configurations/UserProjectMatchConfiguration.cs \
          AppTrack.Persistance/DatabaseContext/AppTrackDatabaseContext.cs
  git commit -m "feat: add UserProjectMatch entity with EF Core configuration"
  ```

---

### Task 4: Run EF Core migrations

**Files:**
- Create: 3 migration files (auto-generated by `dotnet ef`)

- [ ] **Migration 1: Remove `ScrapedAt`**

  Run:
  ```bash
  dotnet ef migrations add RemoveScrapedAtFromScrapedProject \
    --project AppTrack.Persistance \
    --startup-project AppTrack.Api
  ```
  Expected: Migration files created. Open the `.cs` file and verify it contains a `DropColumn` for `ScrapedAt` on `ScrapedProjects`.

- [ ] **Migration 2: Remove poll fields**

  Run:
  ```bash
  dotnet ef migrations add RemovePollFieldsFromProjectMonitoringSettings \
    --project AppTrack.Persistance \
    --startup-project AppTrack.Api
  ```
  Expected: Migration drops `PollIntervalMinutes` and `LastPolledAt` from `ProjectMonitoringSettings`.

- [ ] **Migration 3: Add `UserProjectMatches` table**

  Run:
  ```bash
  dotnet ef migrations add AddUserProjectMatch \
    --project AppTrack.Persistance \
    --startup-project AppTrack.Api
  ```
  Expected: Migration creates `UserProjectMatches` table with unique index on `(UserId, ScrapedProjectId)` and FK to `ScrapedProjects` with `Restrict` delete behavior.

- [ ] **Build to confirm generated code compiles**

  Run: `dotnet build AppTrack.sln --configuration Release`
  Expected: 0 errors, 0 warnings.

- [ ] **Commit**

  ```bash
  git add AppTrack.Persistance/Migrations/
  git commit -m "feat: add migrations for ScrapedAt removal, poll fields removal, and UserProjectMatch table"
  ```

---

## Chunk 2: Repository & Application Contracts

### Task 5: Add `GetUnprocessedForUserAsync` to scraped project repository

**Files:**
- Modify: `AppTrack.Application/Contracts/ProjectMonitoring/IScrapedProjectRepository.cs`
- Modify: `AppTrack.Persistance/Repositories/ScrapedProjectRepository.cs`
- Test: `AppTrack.Application.UnitTests/Features/ProjectMonitoring/Commands/MatchProjectsCommandHandlerTests.cs` (used in Task 8)

- [ ] **Add method to interface**

  In `IScrapedProjectRepository.cs`, add:
  ```csharp
  Task<List<ScrapedProject>> GetUnprocessedForUserAsync(
      string userId,
      IEnumerable<int> portalIds,
      CancellationToken ct);
  ```

- [ ] **Implement in repository**

  In `ScrapedProjectRepository.cs`, add:
  ```csharp
  public async Task<List<ScrapedProject>> GetUnprocessedForUserAsync(
      string userId,
      IEnumerable<int> portalIds,
      CancellationToken ct)
  {
      var processedUrls = await _context.ProcessedProjectItems
          .Where(p => p.UserId == userId)
          .Select(p => p.ProjectItemUrl)
          .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, ct);

      return await _context.ScrapedProjects
          .AsNoTracking()
          .Include(p => p.ProjectPortal)
          .Where(p => portalIds.Contains(p.ProjectPortalId)
                   && !processedUrls.Contains(p.Url))
          .ToListAsync(ct);
  }
  ```

- [ ] **Build to confirm clean**

  Run: `dotnet build AppTrack.sln --configuration Release`
  Expected: 0 errors, 0 warnings.

- [ ] **Commit**

  ```bash
  git add AppTrack.Application/Contracts/ProjectMonitoring/IScrapedProjectRepository.cs \
          AppTrack.Persistance/Repositories/ScrapedProjectRepository.cs
  git commit -m "feat: add GetUnprocessedForUserAsync to IScrapedProjectRepository"
  ```

---

### Task 6: Add `IUserProjectMatchRepository` contract and implementation

**Files:**
- Create: `AppTrack.Application/Contracts/ProjectMonitoring/IUserProjectMatchRepository.cs`
- Create: `AppTrack.Persistance/Repositories/UserProjectMatchRepository.cs`
- Modify: `AppTrack.Persistance/PersistanceServiceRegistration.cs`

- [ ] **Create interface**

  Create `AppTrack.Application/Contracts/ProjectMonitoring/IUserProjectMatchRepository.cs`:
  ```csharp
  using AppTrack.Domain;

  namespace AppTrack.Application.Contracts.ProjectMonitoring;

  public interface IUserProjectMatchRepository
  {
      Task AddRangeAsync(IEnumerable<UserProjectMatch> matches, CancellationToken ct);

      /// <summary>
      /// Returns all unnotified matches (IsNotified=false) for users with NotifyByEmail=true
      /// and a non-empty NotificationEmail. Eager-loads ScrapedProject and ProjectPortal.
      /// </summary>
      Task<List<UserProjectMatch>> GetUnnotifiedAsync(CancellationToken ct);

      /// <summary>Sets IsNotified=true for the given match IDs.</summary>
      Task MarkNotifiedAsync(IEnumerable<int> matchIds, CancellationToken ct);
  }
  ```

- [ ] **Create repository implementation**

  Create `AppTrack.Persistance/Repositories/UserProjectMatchRepository.cs`:
  ```csharp
  using AppTrack.Application.Contracts.ProjectMonitoring;
  using AppTrack.Domain;
  using AppTrack.Persistance.DatabaseContext;
  using Microsoft.EntityFrameworkCore;

  namespace AppTrack.Persistance.Repositories;

  public class UserProjectMatchRepository : IUserProjectMatchRepository
  {
      private readonly AppTrackDatabaseContext _context;

      public UserProjectMatchRepository(AppTrackDatabaseContext context)
          => _context = context;

      public async Task AddRangeAsync(IEnumerable<UserProjectMatch> matches, CancellationToken ct)
      {
          await _context.UserProjectMatches.AddRangeAsync(matches, ct);
          await _context.SaveChangesAsync(ct);
      }

      public async Task<List<UserProjectMatch>> GetUnnotifiedAsync(CancellationToken ct)
      {
          var eligibleUserIds = await _context.ProjectMonitoringSettings
              .Where(s => s.NotifyByEmail && !string.IsNullOrEmpty(s.NotificationEmail))
              .Select(s => s.UserId)
              .ToListAsync(ct);

          return await _context.UserProjectMatches
              .Include(m => m.ScrapedProject)
                  .ThenInclude(p => p.ProjectPortal)
              .Where(m => !m.IsNotified && eligibleUserIds.Contains(m.UserId))
              .ToListAsync(ct);
      }

      public async Task MarkNotifiedAsync(IEnumerable<int> matchIds, CancellationToken ct)
      {
          await _context.UserProjectMatches
              .Where(m => matchIds.Contains(m.Id))
              .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsNotified, true), ct);
      }
  }
  ```

- [ ] **Register in DI**

  In `PersistanceServiceRegistration.cs`, add:
  ```csharp
  services.AddScoped<IUserProjectMatchRepository, UserProjectMatchRepository>();
  ```

- [ ] **Build to confirm clean**

  Run: `dotnet build AppTrack.sln --configuration Release`
  Expected: 0 errors, 0 warnings.

- [ ] **Commit**

  ```bash
  git add AppTrack.Application/Contracts/ProjectMonitoring/IUserProjectMatchRepository.cs \
          AppTrack.Persistance/Repositories/UserProjectMatchRepository.cs \
          AppTrack.Persistance/PersistanceServiceRegistration.cs
  git commit -m "feat: add IUserProjectMatchRepository contract and implementation"
  ```

---

### Task 7: Add `IScrapingEventPublisher` contract and Service Bus implementation

**Files:**
- Create: `AppTrack.Application/Contracts/ProjectMonitoring/IScrapingEventPublisher.cs`
- Create: `AppTrack.Infrastructure/Notifications/ServiceBusScrapingEventPublisher.cs`
- Modify: `AppTrack.Infrastructure/InfrastructureServicesRegistration.cs`

- [ ] **Create interface**

  Create `AppTrack.Application/Contracts/ProjectMonitoring/IScrapingEventPublisher.cs`:
  ```csharp
  namespace AppTrack.Application.Contracts.ProjectMonitoring;

  public interface IScrapingEventPublisher
  {
      Task PublishScrapingCompletedAsync(CancellationToken ct);
  }
  ```

- [ ] **Create Service Bus publisher**

  Create `AppTrack.Infrastructure/Notifications/ServiceBusScrapingEventPublisher.cs`:
  ```csharp
  using AppTrack.Application.Contracts.ProjectMonitoring;
  using Azure.Messaging.ServiceBus;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;

  namespace AppTrack.Infrastructure.Notifications;

  public class ServiceBusScrapingEventPublisher : IScrapingEventPublisher
  {
      private readonly ServiceBusClient _client;
      private readonly string _queueName;
      private readonly ILogger<ServiceBusScrapingEventPublisher> _logger;

      public ServiceBusScrapingEventPublisher(
          IConfiguration configuration,
          ILogger<ServiceBusScrapingEventPublisher> logger)
      {
          var connectionString = configuration["ServiceBusConnection"]
              ?? throw new InvalidOperationException("ServiceBusConnection is not configured.");
          _queueName = configuration["ScrapingCompletedQueueName"] ?? "scraping-completed";
          _client = new ServiceBusClient(connectionString);
          _logger = logger;
      }

      public async Task PublishScrapingCompletedAsync(CancellationToken ct)
      {
          try
          {
              var sender = _client.CreateSender(_queueName);
              await sender.SendMessageAsync(new ServiceBusMessage(), ct);
          }
          catch (Exception ex)
          {
              _logger.LogWarning(ex, "Failed to publish scraping-completed signal to Service Bus. Matching will run on next scrape cycle.");
          }
      }
  }
  ```

- [ ] **Update `InfrastructureServicesRegistration`**

  Remove the `ProjectNotification:Provider` conditional block (lines 44-49) and add:
  ```csharp
  services.AddScoped<IScrapingEventPublisher, ServiceBusScrapingEventPublisher>();
  ```
  Also remove the `using` for `AppTrack.Infrastructure.Notifications` if it becomes unused after removing the notifier registrations — check after deletion.

- [ ] **Build to confirm clean**

  Run: `dotnet build AppTrack.sln --configuration Release`
  Expected: 0 errors, 0 warnings.

- [ ] **Commit**

  ```bash
  git add AppTrack.Application/Contracts/ProjectMonitoring/IScrapingEventPublisher.cs \
          AppTrack.Infrastructure/Notifications/ServiceBusScrapingEventPublisher.cs \
          AppTrack.Infrastructure/InfrastructureServicesRegistration.cs
  git commit -m "feat: add IScrapingEventPublisher and ServiceBusScrapingEventPublisher"
  ```

---

## Chunk 3: New Command Handlers

### Task 8: Implement `MatchProjectsCommand` + handler + tests

**Files:**
- Create: `AppTrack.Application/Features/ProjectMonitoring/Commands/MatchProjects/MatchProjectsCommand.cs`
- Create: `AppTrack.Application/Features/ProjectMonitoring/Commands/MatchProjects/MatchProjectsCommandHandler.cs`
- Create: `AppTrack.Application.UnitTests/Features/ProjectMonitoring/Commands/MatchProjectsCommandHandlerTests.cs`

- [ ] **Write failing tests first**

  Create `AppTrack.Application.UnitTests/Features/ProjectMonitoring/Commands/MatchProjectsCommandHandlerTests.cs`:
  ```csharp
  using AppTrack.Application.Contracts.Mediator;
  using AppTrack.Application.Contracts.Persistance;
  using AppTrack.Application.Contracts.ProjectMonitoring;
  using AppTrack.Application.Features.ProjectMonitoring.Commands.MatchProjects;
  using AppTrack.Domain;
  using AppTrack.Domain.Enums;
  using Moq;
  using Shouldly;

  namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

  public class MatchProjectsCommandHandlerTests
  {
      private readonly Mock<IUserPortalSubscriptionRepository> _subscriptionRepo = new();
      private readonly Mock<IProjectMonitoringSettingsRepository> _settingsRepo = new();
      private readonly Mock<IScrapedProjectRepository> _scrapedProjectRepo = new();
      private readonly Mock<IUserProjectMatchRepository> _matchRepo = new();
      private readonly Mock<IJobApplicationRepository> _jobAppRepo = new();
      private readonly Mock<IProcessedProjectItemRepository> _processedRepo = new();
      private readonly Mock<IUnitOfWork> _unitOfWork = new();

      public MatchProjectsCommandHandlerTests()
      {
          _unitOfWork
              .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
              .Returns<Func<CancellationToken, Task>, CancellationToken>(async (fn, ct) => await fn(ct));
      }

      private MatchProjectsCommandHandler CreateHandler() => new(
          _subscriptionRepo.Object,
          _settingsRepo.Object,
          _scrapedProjectRepo.Object,
          _matchRepo.Object,
          _jobAppRepo.Object,
          _processedRepo.Object,
          _unitOfWork.Object);

      [Fact]
      public async Task Handle_ShouldSkipUser_WhenNoSettings()
      {
          _subscriptionRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync())
              .ReturnsAsync([new UserPortalSubscription { UserId = "u1", ProjectPortalId = 1 }]);
          _settingsRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync((ProjectMonitoringSettings?)null);

          await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

          _scrapedProjectRepo.Verify(r => r.GetUnprocessedForUserAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
      }

      [Fact]
      public async Task Handle_ShouldSkipUser_WhenNoKeywords()
      {
          _subscriptionRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync())
              .ReturnsAsync([new UserPortalSubscription { UserId = "u1", ProjectPortalId = 1 }]);
          _settingsRepo.Setup(r => r.GetByUserIdAsync("u1"))
              .ReturnsAsync(new ProjectMonitoringSettings { UserId = "u1", Keywords = [] });

          await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

          _scrapedProjectRepo.Verify(r => r.GetUnprocessedForUserAsync(It.IsAny<string>(), It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
      }

      [Fact]
      public async Task Handle_ShouldCreateMatchAndJobApplication_WhenKeywordMatches()
      {
          var project = new ScrapedProject { Id = 10, Title = "Senior .NET Developer", Url = "https://x.de/1", CompanyName = "Acme" };
          SetupSingleUserWithProject("u1", [".NET"], project);

          List<UserProjectMatch>? capturedMatches = null;
          _matchRepo
              .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>()))
              .Callback<IEnumerable<UserProjectMatch>, CancellationToken>((m, _) => capturedMatches = m.ToList())
              .Returns(Task.CompletedTask);

          await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

          capturedMatches.ShouldNotBeNull();
          capturedMatches.ShouldHaveSingleItem();
          capturedMatches[0].UserId.ShouldBe("u1");
          capturedMatches[0].ScrapedProjectId.ShouldBe(10);
          capturedMatches[0].IsNotified.ShouldBeFalse();

          _jobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(j =>
              j.UserId == "u1" &&
              j.Status == JobApplicationStatus.Discovered &&
              j.URL == "https://x.de/1")), Times.Once);
      }

      [Fact]
      public async Task Handle_ShouldNotCreateMatch_WhenNoKeywordMatches()
      {
          var project = new ScrapedProject { Id = 11, Title = "Java Developer", Url = "https://x.de/2", CompanyName = "Acme" };
          SetupSingleUserWithProject("u1", [".NET"], project);

          await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

          _matchRepo.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>()), Times.Never);
          _jobAppRepo.Verify(r => r.CreateAsync(It.IsAny<JobApplication>()), Times.Never);
      }

      [Fact]
      public async Task Handle_ShouldMarkAllNewProjectsAsProcessed_IncludingNonMatches()
      {
          var matchingProject   = new ScrapedProject { Id = 1, Title = ".NET Dev", Url = "https://x.de/1", CompanyName = "A" };
          var unmatchedProject  = new ScrapedProject { Id = 2, Title = "Java Dev",  Url = "https://x.de/2", CompanyName = "B" };

          _subscriptionRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync())
              .ReturnsAsync([new UserPortalSubscription { UserId = "u1", ProjectPortalId = 1 }]);
          _settingsRepo.Setup(r => r.GetByUserIdAsync("u1"))
              .ReturnsAsync(new ProjectMonitoringSettings { UserId = "u1", Keywords = [".NET"] });
          _scrapedProjectRepo.Setup(r => r.GetUnprocessedForUserAsync("u1", It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync([matchingProject, unmatchedProject]);
          _matchRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
          _jobAppRepo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>())).Returns(Task.CompletedTask);

          List<ProcessedProjectItem>? capturedProcessed = null;
          _processedRepo
              .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedProjectItem>>(), It.IsAny<CancellationToken>()))
              .Callback<IEnumerable<ProcessedProjectItem>, CancellationToken>((items, _) => capturedProcessed = items.ToList())
              .Returns(Task.CompletedTask);

          await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

          capturedProcessed.ShouldNotBeNull();
          capturedProcessed.Count.ShouldBe(2);
          capturedProcessed.Select(p => p.ProjectItemUrl).ShouldContain("https://x.de/1");
          capturedProcessed.Select(p => p.ProjectItemUrl).ShouldContain("https://x.de/2");
      }

      [Fact]
      public async Task Handle_ShouldMatchCaseInsensitively()
      {
          var project = new ScrapedProject { Id = 12, Title = "senior dotnet developer", Url = "https://x.de/3", CompanyName = "A" };
          SetupSingleUserWithProject("u1", [".NET"], project);
          _matchRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
          _jobAppRepo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>())).Returns(Task.CompletedTask);
          _processedRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedProjectItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

          // "dotnet" contains ".net" case-insensitively — no match expected for ".NET" in "senior dotnet developer"
          // Let's use a keyword that will actually match
          var project2 = new ScrapedProject { Id = 13, Title = "Senior .NET Developer", Url = "https://x.de/4", CompanyName = "B" };
          SetupSingleUserWithProject("u2", ["senior"], project2);
          _matchRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<UserProjectMatch>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
          _jobAppRepo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>())).Returns(Task.CompletedTask);

          await CreateHandler().Handle(new MatchProjectsCommand(), CancellationToken.None);

          _matchRepo.Verify(r => r.AddRangeAsync(
              It.Is<IEnumerable<UserProjectMatch>>(m => m.Any(x => x.ScrapedProjectId == 13)),
              It.IsAny<CancellationToken>()), Times.Once);
      }

      private void SetupSingleUserWithProject(string userId, List<string> keywords, ScrapedProject project)
      {
          _subscriptionRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync())
              .ReturnsAsync([new UserPortalSubscription { UserId = userId, ProjectPortalId = 1 }]);
          _settingsRepo.Setup(r => r.GetByUserIdAsync(userId))
              .ReturnsAsync(new ProjectMonitoringSettings { UserId = userId, Keywords = keywords });
          _scrapedProjectRepo.Setup(r => r.GetUnprocessedForUserAsync(userId, It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync([project]);
          _processedRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedProjectItem>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
      }
  }
  ```

- [ ] **Run tests to confirm they fail (handler doesn't exist yet)**

  Run: `dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~MatchProjectsCommandHandlerTests" --configuration Release`
  Expected: Build error — `MatchProjectsCommand` and `MatchProjectsCommandHandler` not found.

- [ ] **Create `MatchProjectsCommand`**

  Create `AppTrack.Application/Features/ProjectMonitoring/Commands/MatchProjects/MatchProjectsCommand.cs`:
  ```csharp
  using AppTrack.Application.Contracts.Mediator;
  using AppTrack.Application.Shared;

  namespace AppTrack.Application.Features.ProjectMonitoring.Commands.MatchProjects;

  public class MatchProjectsCommand : IRequest<Unit>;
  ```

- [ ] **Create `MatchProjectsCommandHandler`**

  Create `AppTrack.Application/Features/ProjectMonitoring/Commands/MatchProjects/MatchProjectsCommandHandler.cs`:
  ```csharp
  using AppTrack.Application.Contracts.Mediator;
  using AppTrack.Application.Contracts.Persistance;
  using AppTrack.Application.Contracts.ProjectMonitoring;
  using AppTrack.Application.Shared;
  using AppTrack.Domain;
  using AppTrack.Domain.Enums;

  namespace AppTrack.Application.Features.ProjectMonitoring.Commands.MatchProjects;

  public class MatchProjectsCommandHandler : IRequestHandler<MatchProjectsCommand, Unit>
  {
      private readonly IUserPortalSubscriptionRepository _subscriptionRepository;
      private readonly IProjectMonitoringSettingsRepository _settingsRepository;
      private readonly IScrapedProjectRepository _scrapedProjectRepository;
      private readonly IUserProjectMatchRepository _matchRepository;
      private readonly IJobApplicationRepository _jobApplicationRepository;
      private readonly IProcessedProjectItemRepository _processedRepository;
      private readonly IUnitOfWork _unitOfWork;

      public MatchProjectsCommandHandler(
          IUserPortalSubscriptionRepository subscriptionRepository,
          IProjectMonitoringSettingsRepository settingsRepository,
          IScrapedProjectRepository scrapedProjectRepository,
          IUserProjectMatchRepository matchRepository,
          IJobApplicationRepository jobApplicationRepository,
          IProcessedProjectItemRepository processedRepository,
          IUnitOfWork unitOfWork)
      {
          _subscriptionRepository = subscriptionRepository;
          _settingsRepository = settingsRepository;
          _scrapedProjectRepository = scrapedProjectRepository;
          _matchRepository = matchRepository;
          _jobApplicationRepository = jobApplicationRepository;
          _processedRepository = processedRepository;
          _unitOfWork = unitOfWork;
      }

      public async Task<Unit> Handle(MatchProjectsCommand request, CancellationToken cancellationToken)
      {
          var allSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsWithPortalsAsync();
          var byUser = allSubscriptions.GroupBy(s => s.UserId);

          foreach (var userGroup in byUser)
          {
              var userId = userGroup.Key;
              var settings = await _settingsRepository.GetByUserIdAsync(userId);

              if (settings is null || settings.Keywords.Count == 0)
                  continue;

              var portalIds = userGroup.Select(s => s.ProjectPortalId).ToList();
              var newProjects = await _scrapedProjectRepository.GetUnprocessedForUserAsync(userId, portalIds, cancellationToken);

              if (newProjects.Count == 0)
                  continue;

              var matches = newProjects
                  .Where(p => settings.Keywords.Any(kw => p.Title.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                  .ToList();

              await _unitOfWork.ExecuteInTransactionAsync(async ct =>
              {
                  if (matches.Count > 0)
                  {
                      var userMatches = matches.Select(m => new UserProjectMatch
                      {
                          UserId = userId,
                          ScrapedProjectId = m.Id,
                          IsNotified = false
                      }).ToList();
                      await _matchRepository.AddRangeAsync(userMatches, ct);

                      foreach (var match in matches)
                      {
                          await _jobApplicationRepository.CreateAsync(new JobApplication
                          {
                              UserId = userId,
                              Name = string.IsNullOrEmpty(match.CompanyName) ? match.Title : match.CompanyName,
                              Position = match.Title,
                              URL = match.Url,
                              JobDescription = string.Empty,
                              Location = string.Empty,
                              ContactPerson = string.Empty,
                              DurationInMonths = string.Empty,
                              StartDate = DateTime.UtcNow,
                              Status = JobApplicationStatus.Discovered
                          });
                      }
                  }

                  var processedItems = newProjects.Select(p => new ProcessedProjectItem
                  {
                      UserId = userId,
                      ProjectItemUrl = p.Url,
                      ProcessedAt = DateTime.UtcNow
                  });
                  await _processedRepository.AddRangeAsync(processedItems, ct);
              }, cancellationToken);
          }

          return Unit.Value;
      }
  }
  ```

- [ ] **Run tests to confirm they pass**

  Run: `dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~MatchProjectsCommandHandlerTests" --configuration Release`
  Expected: All pass.

- [ ] **Run full test suite**

  Run: `dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release`
  Expected: All pass.

- [ ] **Commit**

  ```bash
  git add AppTrack.Application/Features/ProjectMonitoring/Commands/MatchProjects/ \
          AppTrack.Application.UnitTests/Features/ProjectMonitoring/Commands/MatchProjectsCommandHandlerTests.cs
  git commit -m "feat: implement MatchProjectsCommand handler with tests"
  ```

---

### Task 9: Implement `SendProjectNotificationsCommand` + handler + tests

**Files:**
- Create: `AppTrack.Application/Features/ProjectMonitoring/Commands/SendProjectNotifications/SendProjectNotificationsCommand.cs`
- Create: `AppTrack.Application/Features/ProjectMonitoring/Commands/SendProjectNotifications/SendProjectNotificationsCommandHandler.cs`
- Create: `AppTrack.Application.UnitTests/Features/ProjectMonitoring/Commands/SendProjectNotificationsCommandHandlerTests.cs`

- [ ] **Write failing tests first**

  Create `AppTrack.Application.UnitTests/Features/ProjectMonitoring/Commands/SendProjectNotificationsCommandHandlerTests.cs`:
  ```csharp
  using AppTrack.Application.Contracts.Email;
  using AppTrack.Application.Contracts.ProjectMonitoring;
  using AppTrack.Application.Features.ProjectMonitoring.Commands.SendProjectNotifications;
  using AppTrack.Application.Models.Email;
  using AppTrack.Domain;
  using Moq;
  using Shouldly;

  namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

  public class SendProjectNotificationsCommandHandlerTests
  {
      private readonly Mock<IUserProjectMatchRepository> _matchRepo = new();
      private readonly Mock<IProjectMonitoringSettingsRepository> _settingsRepo = new();
      private readonly Mock<IEmailSender> _emailSender = new();

      private SendProjectNotificationsCommandHandler CreateHandler() => new(
          _matchRepo.Object,
          _settingsRepo.Object,
          _emailSender.Object);

      [Fact]
      public async Task Handle_ShouldSendEmail_WhenUnnotifiedMatchesExistAndIntervalReached()
      {
          var portal = new ProjectPortal { Name = "Freelancermap" };
          var project = new ScrapedProject { Title = ".NET Dev", Url = "https://x.de/1", ProjectPortal = portal };
          var match = new UserProjectMatch { Id = 1, UserId = "u1", ScrapedProject = project, IsNotified = false };

          _matchRepo.Setup(r => r.GetUnnotifiedAsync(It.IsAny<CancellationToken>()))
              .ReturnsAsync([match]);
          _settingsRepo.Setup(r => r.GetByUserIdAsync("u1"))
              .ReturnsAsync(new ProjectMonitoringSettings
              {
                  UserId = "u1",
                  NotificationEmail = "test@example.com",
                  NotificationIntervalMinutes = 60,
                  LastNotifiedAt = null
              });
          _emailSender.Setup(e => e.SendEmail(It.IsAny<EmailMessage>())).ReturnsAsync(true);
          _matchRepo.Setup(r => r.MarkNotifiedAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
          _settingsRepo.Setup(r => r.UpdateAsync(It.IsAny<ProjectMonitoringSettings>())).Returns(Task.CompletedTask);

          await CreateHandler().Handle(new SendProjectNotificationsCommand(), CancellationToken.None);

          _emailSender.Verify(e => e.SendEmail(It.Is<EmailMessage>(m => m.To == "test@example.com")), Times.Once);
          _matchRepo.Verify(r => r.MarkNotifiedAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(1)), It.IsAny<CancellationToken>()), Times.Once);
      }

      [Fact]
      public async Task Handle_ShouldNotSendEmail_WhenNotificationIntervalNotReached()
      {
          var portal = new ProjectPortal { Name = "Freelancermap" };
          var project = new ScrapedProject { Title = ".NET Dev", Url = "https://x.de/1", ProjectPortal = portal };
          var match = new UserProjectMatch { Id = 2, UserId = "u1", ScrapedProject = project, IsNotified = false };

          _matchRepo.Setup(r => r.GetUnnotifiedAsync(It.IsAny<CancellationToken>())).ReturnsAsync([match]);
          _settingsRepo.Setup(r => r.GetByUserIdAsync("u1"))
              .ReturnsAsync(new ProjectMonitoringSettings
              {
                  UserId = "u1",
                  NotificationEmail = "test@example.com",
                  NotificationIntervalMinutes = 60,
                  LastNotifiedAt = DateTime.UtcNow.AddMinutes(-10) // only 10 min ago, interval is 60
              });

          await CreateHandler().Handle(new SendProjectNotificationsCommand(), CancellationToken.None);

          _emailSender.Verify(e => e.SendEmail(It.IsAny<EmailMessage>()), Times.Never);
          _matchRepo.Verify(r => r.MarkNotifiedAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
      }

      [Fact]
      public async Task Handle_ShouldDoNothing_WhenNoUnnotifiedMatches()
      {
          _matchRepo.Setup(r => r.GetUnnotifiedAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

          await CreateHandler().Handle(new SendProjectNotificationsCommand(), CancellationToken.None);

          _emailSender.Verify(e => e.SendEmail(It.IsAny<EmailMessage>()), Times.Never);
      }

      [Fact]
      public async Task Handle_ShouldUpdateLastNotifiedAt_AfterSendingEmail()
      {
          var portal = new ProjectPortal { Name = "Portal" };
          var project = new ScrapedProject { Title = "Dev", Url = "https://x.de/1", ProjectPortal = portal };
          var match = new UserProjectMatch { Id = 3, UserId = "u1", ScrapedProject = project, IsNotified = false };

          _matchRepo.Setup(r => r.GetUnnotifiedAsync(It.IsAny<CancellationToken>())).ReturnsAsync([match]);

          ProjectMonitoringSettings? capturedSettings = null;
          var settings = new ProjectMonitoringSettings
          {
              UserId = "u1",
              NotificationEmail = "a@b.com",
              NotificationIntervalMinutes = 60,
              LastNotifiedAt = null
          };
          _settingsRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(settings);
          _settingsRepo
              .Setup(r => r.UpdateAsync(It.IsAny<ProjectMonitoringSettings>()))
              .Callback<ProjectMonitoringSettings>(s => capturedSettings = s)
              .Returns(Task.CompletedTask);
          _emailSender.Setup(e => e.SendEmail(It.IsAny<EmailMessage>())).ReturnsAsync(true);
          _matchRepo.Setup(r => r.MarkNotifiedAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

          await CreateHandler().Handle(new SendProjectNotificationsCommand(), CancellationToken.None);

          capturedSettings.ShouldNotBeNull();
          capturedSettings!.LastNotifiedAt.ShouldNotBeNull();
          capturedSettings.LastNotifiedAt!.Value.ShouldBeGreaterThan(DateTime.UtcNow.AddSeconds(-5));
      }
  }
  ```

- [ ] **Run tests to confirm they fail**

  Run: `dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~SendProjectNotificationsCommandHandlerTests" --configuration Release`
  Expected: Build error — command and handler not found.

- [ ] **Create `SendProjectNotificationsCommand`**

  Create `AppTrack.Application/Features/ProjectMonitoring/Commands/SendProjectNotifications/SendProjectNotificationsCommand.cs`:
  ```csharp
  using AppTrack.Application.Contracts.Mediator;
  using AppTrack.Application.Shared;

  namespace AppTrack.Application.Features.ProjectMonitoring.Commands.SendProjectNotifications;

  public class SendProjectNotificationsCommand : IRequest<Unit>;
  ```

- [ ] **Create `SendProjectNotificationsCommandHandler`**

  Create `AppTrack.Application/Features/ProjectMonitoring/Commands/SendProjectNotifications/SendProjectNotificationsCommandHandler.cs`:
  ```csharp
  using AppTrack.Application.Contracts.Email;
  using AppTrack.Application.Contracts.Mediator;
  using AppTrack.Application.Contracts.ProjectMonitoring;
  using AppTrack.Application.Models.Email;
  using AppTrack.Application.Shared;

  namespace AppTrack.Application.Features.ProjectMonitoring.Commands.SendProjectNotifications;

  public class SendProjectNotificationsCommandHandler : IRequestHandler<SendProjectNotificationsCommand, Unit>
  {
      private readonly IUserProjectMatchRepository _matchRepository;
      private readonly IProjectMonitoringSettingsRepository _settingsRepository;
      private readonly IEmailSender _emailSender;

      public SendProjectNotificationsCommandHandler(
          IUserProjectMatchRepository matchRepository,
          IProjectMonitoringSettingsRepository settingsRepository,
          IEmailSender emailSender)
      {
          _matchRepository = matchRepository;
          _settingsRepository = settingsRepository;
          _emailSender = emailSender;
      }

      public async Task<Unit> Handle(SendProjectNotificationsCommand request, CancellationToken cancellationToken)
      {
          var unnotified = await _matchRepository.GetUnnotifiedAsync(cancellationToken);
          if (unnotified.Count == 0)
              return Unit.Value;

          var byUser = unnotified.GroupBy(m => m.UserId);

          foreach (var userGroup in byUser)
          {
              var userId = userGroup.Key;
              var settings = await _settingsRepository.GetByUserIdAsync(userId);

              if (settings is null)
                  continue;

              var isIntervalReached = settings.LastNotifiedAt is null ||
                  DateTime.UtcNow >= settings.LastNotifiedAt.Value.AddMinutes(settings.NotificationIntervalMinutes);

              if (!isIntervalReached)
                  continue;

              var matches = userGroup.ToList();
              var body = string.Join("\n", matches.Select(m =>
                  $"- {m.ScrapedProject.Title} ({m.ScrapedProject.ProjectPortal?.Name ?? string.Empty}): {m.ScrapedProject.Url}"));

              var email = new EmailMessage
              {
                  To = settings.NotificationEmail,
                  Subject = $"{matches.Count} new job(s) discovered",
                  Body = $"The following jobs matched your keywords:\n\n{body}"
              };

              var sent = await _emailSender.SendEmail(email);
              if (!sent)
                  continue;

              await _matchRepository.MarkNotifiedAsync(matches.Select(m => m.Id), cancellationToken);

              settings.LastNotifiedAt = DateTime.UtcNow;
              await _settingsRepository.UpdateAsync(settings);
          }

          return Unit.Value;
      }
  }
  ```

- [ ] **Run tests to confirm they pass**

  Run: `dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~SendProjectNotificationsCommandHandlerTests" --configuration Release`
  Expected: All pass.

- [ ] **Run full test suite**

  Run: `dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release`
  Expected: All pass.

- [ ] **Commit**

  ```bash
  git add AppTrack.Application/Features/ProjectMonitoring/Commands/SendProjectNotifications/ \
          AppTrack.Application.UnitTests/Features/ProjectMonitoring/Commands/SendProjectNotificationsCommandHandlerTests.cs
  git commit -m "feat: implement SendProjectNotificationsCommand handler with tests"
  ```

---

## Chunk 4: Cleanup & Azure Functions Wiring

### Task 10: Delete obsolete Infrastructure notifier files

> Note: `PollProjectsCommand` and `PollProjectsCommandHandler` were already deleted in Task 2.

**Files:**
- Delete: `AppTrack.Application/Contracts/ProjectMonitoring/IProjectMatchNotifier.cs`
- Delete: `AppTrack.Infrastructure/Notifications/DirectEmailProjectNotifier.cs`
- Delete: `AppTrack.Infrastructure/Notifications/ServiceBusProjectNotifier.cs`

- [ ] **Delete obsolete notifier files**

  ```bash
  git rm "AppTrack.Application/Contracts/ProjectMonitoring/IProjectMatchNotifier.cs"
  git rm "AppTrack.Infrastructure/Notifications/DirectEmailProjectNotifier.cs"
  git rm "AppTrack.Infrastructure/Notifications/ServiceBusProjectNotifier.cs"
  ```

- [ ] **Build to confirm clean (no dangling references)**

  Run: `dotnet build AppTrack.sln --configuration Release`
  Expected: 0 errors, 0 warnings.

- [ ] **Run full test suite**

  Run: `dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release`
  Expected: All pass.

- [ ] **Commit**

  ```bash
  git commit -m "refactor: remove PollProjectsCommand, IProjectMatchNotifier and obsolete notifier implementations"
  ```

---

### Task 11: Wire up Azure Functions

**Files:**
- Modify: `AppTrack.Functions/AppTrack.Functions.csproj`
- Modify: `Directory.Packages.props`
- Modify: `AppTrack.Functions/ScrapePortalsFunction.cs`
- Create: `AppTrack.Functions/MatchProjectsFunction.cs`
- Create: `AppTrack.Functions/SendNotificationsFunction.cs`
- Modify: `AppTrack.Functions/host.json`
- Modify: `AppTrack.Functions/local.settings.json`

- [ ] **Add Service Bus NuGet package version to `Directory.Packages.props`**

  Under the `<!-- Azure Functions Isolated Worker -->` section, add:
  ```xml
  <PackageVersion Include="Microsoft.Azure.Functions.Worker.Extensions.ServiceBus" Version="4.3.1" />
  ```

- [ ] **Add package reference to `AppTrack.Functions.csproj`**

  In the first `<ItemGroup>`, add:
  ```xml
  <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.ServiceBus" />
  ```

- [ ] **Update `ScrapePortalsFunction` to publish after scraping**

  Replace the current `ScrapePortalsFunction.cs` with:
  ```csharp
  using AppTrack.Application.Contracts.Mediator;
  using AppTrack.Application.Contracts.ProjectMonitoring;
  using AppTrack.Application.Features.ProjectMonitoring.Commands.ScrapePortals;
  using Microsoft.Azure.Functions.Worker;
  using Microsoft.Extensions.Logging;

  namespace AppTrack.Functions;

  public sealed class ScrapePortalsFunction(
      IMediator mediator,
      IScrapingEventPublisher scrapingEventPublisher,
      ILogger<ScrapePortalsFunction> logger)
  {
      [Function(nameof(ScrapePortalsFunction))]
      public async Task Run(
          [TimerTrigger("%ScrapeSchedule%")] TimerInfo timer,
          CancellationToken cancellationToken)
      {
          var startedAt = DateTimeOffset.UtcNow;
          logger.LogInformation("ScrapePortalsFunction started at {StartedAt}", startedAt);

          await mediator.Send(new ScrapePortalsCommand(), cancellationToken);
          await scrapingEventPublisher.PublishScrapingCompletedAsync(cancellationToken);

          logger.LogInformation(
              "ScrapePortalsFunction completed. Duration: {Duration}ms",
              (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds);
      }
  }
  ```

- [ ] **Create `MatchProjectsFunction`**

  Create `AppTrack.Functions/MatchProjectsFunction.cs`:
  ```csharp
  using AppTrack.Application.Contracts.Mediator;
  using AppTrack.Application.Features.ProjectMonitoring.Commands.MatchProjects;
  using Azure.Messaging.ServiceBus;
  using Microsoft.Azure.Functions.Worker;
  using Microsoft.Extensions.Logging;

  namespace AppTrack.Functions;

  public sealed class MatchProjectsFunction(IMediator mediator, ILogger<MatchProjectsFunction> logger)
  {
      [Function(nameof(MatchProjectsFunction))]
      public async Task Run(
          [ServiceBusTrigger("%ScrapingCompletedQueueName%", Connection = "ServiceBusConnection")]
          ServiceBusReceivedMessage message,
          CancellationToken cancellationToken)
      {
          logger.LogInformation("MatchProjectsFunction triggered by Service Bus message {MessageId}", message.MessageId);
          await mediator.Send(new MatchProjectsCommand(), cancellationToken);
          logger.LogInformation("MatchProjectsFunction completed.");
      }
  }
  ```

- [ ] **Create `SendNotificationsFunction`**

  Create `AppTrack.Functions/SendNotificationsFunction.cs`:
  ```csharp
  using AppTrack.Application.Contracts.Mediator;
  using AppTrack.Application.Features.ProjectMonitoring.Commands.SendProjectNotifications;
  using Microsoft.Azure.Functions.Worker;
  using Microsoft.Extensions.Logging;

  namespace AppTrack.Functions;

  public sealed class SendNotificationsFunction(IMediator mediator, ILogger<SendNotificationsFunction> logger)
  {
      [Function(nameof(SendNotificationsFunction))]
      public async Task Run(
          [TimerTrigger("%NotificationSchedule%")] TimerInfo timer,
          CancellationToken cancellationToken)
      {
          logger.LogInformation("SendNotificationsFunction started.");
          await mediator.Send(new SendProjectNotificationsCommand(), cancellationToken);
          logger.LogInformation("SendNotificationsFunction completed.");
      }
  }
  ```

- [ ] **Update `host.json` to set `maxConcurrentCalls = 1` for Service Bus**

  Replace `host.json` contents:
  ```json
  {
    "version": "2.0",
    "logging": {
      "applicationInsights": {
        "samplingSettings": {
          "isEnabled": true,
          "excludedTypes": "Request"
        }
      },
      "logLevel": {
        "default": "Information",
        "Host.Results": "Error",
        "Function": "Information",
        "Host.Aggregator": "Trace"
      }
    },
    "extensions": {
      "serviceBus": {
        "maxConcurrentCalls": 1
      }
    }
  }
  ```

- [ ] **Update `local.settings.json`**

  Add the three new values:
  ```json
  {
    "IsEncrypted": false,
    "Values": {
      "AzureWebJobsStorage": "UseDevelopmentStorage=true",
      "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
      "ScrapeSchedule": "0 */10 * * * *",
      "ConnectionStrings__AppTrackConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=AppTrack_Local;Trusted_Connection=True;MultipleActiveResultSets=True;",
      "ServiceBusConnection": "<your-service-bus-connection-string>",
      "ScrapingCompletedQueueName": "scraping-completed",
      "NotificationSchedule": "*/5 * * * *"
    }
  }
  ```

- [ ] **Build to confirm clean**

  Run: `dotnet build AppTrack.sln --configuration Release`
  Expected: 0 errors, 0 warnings.

- [ ] **Run full test suite**

  Run: `dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release`
  Expected: All pass.

- [ ] **Commit**

  ```bash
  git add Directory.Packages.props \
          AppTrack.Functions/AppTrack.Functions.csproj \
          AppTrack.Functions/ScrapePortalsFunction.cs \
          AppTrack.Functions/MatchProjectsFunction.cs \
          AppTrack.Functions/SendNotificationsFunction.cs \
          AppTrack.Functions/host.json \
          AppTrack.Functions/local.settings.json
  git commit -m "feat: wire up MatchProjectsFunction and SendNotificationsFunction with Service Bus trigger"
  ```

---

### Task 12: Final verification

- [ ] **Run all tests (unit + persistence integration)**

  ```bash
  dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
  dotnet test AppTrack.Persistance.IntegrationTests/AppTrack.Persistance.IntegrationTests.csproj --configuration Release
  ```
  Expected: All pass.

- [ ] **Build Release**

  Run: `dotnet build AppTrack.sln --configuration Release`
  Expected: 0 errors, 0 warnings.

- [ ] **Verify migrations are applied correctly on startup**

  Start `AppTrack.Api` locally. Expected: startup logs show migrations applied without errors. The three new migrations should be listed.
