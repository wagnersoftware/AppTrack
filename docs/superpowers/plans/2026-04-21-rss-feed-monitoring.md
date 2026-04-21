# RSS Feed Monitoring Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement automatic RSS feed monitoring that creates `JobApplication` entries with status `Discovered` when keywords match, with per-user configuration and cloud-portable notification.

**Architecture:** Clean Architecture (Domain → Application → Infrastructure → API). All business logic lives in Application CQRS handlers, Infrastructure provides RSS parsing and notification implementations, a `BackgroundService` in API triggers polling every 5 minutes and is designed to be replaced by an Azure Functions Timer Trigger without code changes.

**Tech Stack:** EF Core 10 + SQL Server, CodeHollow.FeedReader (RSS parsing), Azure.Messaging.ServiceBus (cloud notification), FluentValidation, xUnit + Moq + Shouldly (tests), Testcontainers (API integration tests).

---

## Chunk 1: Foundation — Domain, Persistence, Application Contracts

### Task 1: Add `RssParserType` enum and `Discovered` status

**Files:**
- Create: `AppTrack.Domain/Enums/RssParserType.cs`
- Modify: `AppTrack.Domain/Enums/JobApplicationStatus.cs`

- [ ] **Create `RssParserType` enum**

```csharp
// AppTrack.Domain/Enums/RssParserType.cs
namespace AppTrack.Domain.Enums;

public enum RssParserType
{
    Default,
    Stepstone
}
```

- [ ] **Add `Discovered` to `JobApplicationStatus`**

```csharp
// AppTrack.Domain/Enums/JobApplicationStatus.cs
namespace AppTrack.Domain.Enums;

public enum JobApplicationStatus
{
    New,
    WaitingForFeedback,
    Rejected,
    Discovered
}
```

- [ ] **Build and verify 0 errors**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Commit**

```bash
git add AppTrack.Domain/Enums/RssParserType.cs AppTrack.Domain/Enums/JobApplicationStatus.cs
git commit -m "feat: add RssParserType enum and Discovered job application status"
```

---

### Task 2: Create domain entities

**Files:**
- Create: `AppTrack.Domain/RssPortal.cs`
- Create: `AppTrack.Domain/UserRssSubscription.cs`
- Create: `AppTrack.Domain/RssMonitoringSettings.cs`
- Create: `AppTrack.Domain/ProcessedFeedItem.cs`

- [ ] **Create `RssPortal`**

```csharp
// AppTrack.Domain/RssPortal.cs
using AppTrack.Domain.Common;
using AppTrack.Domain.Enums;

namespace AppTrack.Domain;

public class RssPortal : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public RssParserType ParserType { get; set; } = RssParserType.Default;
    public bool IsActive { get; set; } = true;
}
```

- [ ] **Create `UserRssSubscription`**

```csharp
// AppTrack.Domain/UserRssSubscription.cs
using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class UserRssSubscription : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int RssPortalId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastPolledAt { get; set; }
    public RssPortal RssPortal { get; set; } = null!;
}
```

- [ ] **Create `RssMonitoringSettings`**

```csharp
// AppTrack.Domain/RssMonitoringSettings.cs
using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class RssMonitoringSettings : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = [];
    public int PollIntervalMinutes { get; set; } = 60;
}
```

- [ ] **Create `ProcessedFeedItem`**

```csharp
// AppTrack.Domain/ProcessedFeedItem.cs
using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class ProcessedFeedItem : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string FeedItemUrl { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
```

- [ ] **Build and verify 0 errors**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

- [ ] **Commit**

```bash
git add AppTrack.Domain/RssPortal.cs AppTrack.Domain/UserRssSubscription.cs AppTrack.Domain/RssMonitoringSettings.cs AppTrack.Domain/ProcessedFeedItem.cs
git commit -m "feat: add RSS feed monitoring domain entities"
```

---

### Task 3: Create application contracts

**Files:**
- Create: `AppTrack.Application/Contracts/RssFeed/IRssPortalRepository.cs`
- Create: `AppTrack.Application/Contracts/RssFeed/IUserRssSubscriptionRepository.cs`
- Create: `AppTrack.Application/Contracts/RssFeed/IRssMonitoringSettingsRepository.cs`
- Create: `AppTrack.Application/Contracts/RssFeed/IProcessedFeedItemRepository.cs`
- Create: `AppTrack.Application/Contracts/RssFeed/IRssFeedReader.cs`
- Create: `AppTrack.Application/Contracts/RssFeed/IRssFeedItemParser.cs`
- Create: `AppTrack.Application/Contracts/RssFeed/IRssMatchNotifier.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Models/RawFeedItem.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Models/RssJobApplicationData.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Dto/RssPortalDto.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Dto/RssMonitoringSettingsDto.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Dto/RssSubscriptionItemDto.cs`

- [ ] **Create repository interfaces**

```csharp
// AppTrack.Application/Contracts/RssFeed/IRssPortalRepository.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IRssPortalRepository : IGenericRepository<RssPortal>
{
    Task<List<RssPortal>> GetAllActiveAsync();
}
```

```csharp
// AppTrack.Application/Contracts/RssFeed/IUserRssSubscriptionRepository.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IUserRssSubscriptionRepository : IGenericRepository<UserRssSubscription>
{
    Task<List<UserRssSubscription>> GetActiveSubscriptionsWithPortalsAsync();
    Task<List<UserRssSubscription>> GetByUserIdAsync(string userId);
    Task<UserRssSubscription?> GetByUserAndPortalAsync(string userId, int portalId);
    Task UpsertAsync(string userId, int portalId, bool isActive);
}
```

```csharp
// AppTrack.Application/Contracts/RssFeed/IRssMonitoringSettingsRepository.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IRssMonitoringSettingsRepository : IGenericRepository<RssMonitoringSettings>
{
    Task<RssMonitoringSettings?> GetByUserIdAsync(string userId);
    Task UpsertAsync(RssMonitoringSettings settings);
}
```

```csharp
// AppTrack.Application/Contracts/RssFeed/IProcessedFeedItemRepository.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IProcessedFeedItemRepository : IGenericRepository<ProcessedFeedItem>
{
    Task<HashSet<string>> GetProcessedUrlsAsync(string userId, IEnumerable<string> urls);
    Task AddRangeAsync(IEnumerable<ProcessedFeedItem> items, CancellationToken ct);
}
```

- [ ] **Create service interfaces and shared types**

```csharp
// AppTrack.Application/Features/RssFeeds/Models/RawFeedItem.cs
namespace AppTrack.Application.Features.RssFeeds.Models;

public record RawFeedItem(string Title, string Url, string Description, DateTime? PublishedAt);
```

```csharp
// AppTrack.Application/Features/RssFeeds/Models/RssJobApplicationData.cs
namespace AppTrack.Application.Features.RssFeeds.Models;

public record RssJobApplicationData(
    string Position,
    string Url,
    string JobDescription,
    string CompanyName,
    string PortalName);
```

```csharp
// AppTrack.Application/Contracts/RssFeed/IRssFeedReader.cs
using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IRssFeedReader
{
    Task<List<RawFeedItem>> ReadAsync(string feedUrl, CancellationToken ct);
}
```

```csharp
// AppTrack.Application/Contracts/RssFeed/IRssFeedItemParser.cs
using AppTrack.Application.Features.RssFeeds.Models;
using AppTrack.Domain.Enums;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IRssFeedItemParser
{
    RssJobApplicationData Parse(RawFeedItem item, RssParserType parserType, string portalName);
}
```

```csharp
// AppTrack.Application/Contracts/RssFeed/IRssMatchNotifier.cs
using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IRssMatchNotifier
{
    Task NotifyAsync(string userId, List<RssJobApplicationData> matches, CancellationToken ct);
}
```

- [ ] **Create DTOs**

```csharp
// AppTrack.Application/Features/RssFeeds/Dto/RssPortalDto.cs
namespace AppTrack.Application.Features.RssFeeds.Dto;

public record RssPortalDto(int Id, string Name, bool IsSubscribed);
```

```csharp
// AppTrack.Application/Features/RssFeeds/Dto/RssMonitoringSettingsDto.cs
namespace AppTrack.Application.Features.RssFeeds.Dto;

public record RssMonitoringSettingsDto(List<string> Keywords, int PollIntervalMinutes);
```

```csharp
// AppTrack.Application/Features/RssFeeds/Dto/RssSubscriptionItemDto.cs
namespace AppTrack.Application.Features.RssFeeds.Dto;

public record RssSubscriptionItemDto(int PortalId, bool IsActive);
```

- [ ] **Build and verify 0 errors**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

- [ ] **Commit**

```bash
git add AppTrack.Application/Contracts/RssFeed/ AppTrack.Application/Features/RssFeeds/
git commit -m "feat: add RSS feed application contracts and shared types"
```

---

### Task 4: Persistence — EF configurations, DbContext, repositories

**Files:**
- Create: `AppTrack.Persistance/Configurations/RssPortalConfiguration.cs`
- Create: `AppTrack.Persistance/Configurations/UserRssSubscriptionConfiguration.cs`
- Create: `AppTrack.Persistance/Configurations/RssMonitoringSettingsConfiguration.cs`
- Create: `AppTrack.Persistance/Configurations/ProcessedFeedItemConfiguration.cs`
- Modify: `AppTrack.Persistance/DatabaseContext/AppTrackDatabaseContext.cs`
- Create: `AppTrack.Persistance/Repositories/RssPortalRepository.cs`
- Create: `AppTrack.Persistance/Repositories/UserRssSubscriptionRepository.cs`
- Create: `AppTrack.Persistance/Repositories/RssMonitoringSettingsRepository.cs`
- Create: `AppTrack.Persistance/Repositories/ProcessedFeedItemRepository.cs`
- Modify: `AppTrack.Persistance/PersistanceServiceRegistration.cs`

- [ ] **Create EF configurations**

```csharp
// AppTrack.Persistance/Configurations/RssPortalConfiguration.cs
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class RssPortalConfiguration : IEntityTypeConfiguration<RssPortal>
{
    public void Configure(EntityTypeBuilder<RssPortal> builder)
    {
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Url).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.ParserType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasData(
            new RssPortal
            {
                Id = 1,
                Name = "Stepstone",
                Url = "https://www.stepstone.de/rss/stellenangebote.html",
                ParserType = RssParserType.Stepstone,
                IsActive = true
            });
    }
}
```

```csharp
// AppTrack.Persistance/Configurations/UserRssSubscriptionConfiguration.cs
using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class UserRssSubscriptionConfiguration : IEntityTypeConfiguration<UserRssSubscription>
{
    public void Configure(EntityTypeBuilder<UserRssSubscription> builder)
    {
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => new { x.UserId, x.RssPortalId }).IsUnique();
        builder.HasOne(x => x.RssPortal).WithMany().HasForeignKey(x => x.RssPortalId);
    }
}
```

```csharp
// AppTrack.Persistance/Configurations/RssMonitoringSettingsConfiguration.cs
using System.Text.Json;
using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class RssMonitoringSettingsConfiguration : IEntityTypeConfiguration<RssMonitoringSettings>
{
    public void Configure(EntityTypeBuilder<RssMonitoringSettings> builder)
    {
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.Property(x => x.Keywords)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? []);
    }
}
```

```csharp
// AppTrack.Persistance/Configurations/ProcessedFeedItemConfiguration.cs
using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class ProcessedFeedItemConfiguration : IEntityTypeConfiguration<ProcessedFeedItem>
{
    public void Configure(EntityTypeBuilder<ProcessedFeedItem> builder)
    {
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.FeedItemUrl).IsRequired().HasMaxLength(2000);
        builder.HasIndex(x => new { x.UserId, x.FeedItemUrl }).IsUnique();
    }
}
```

- [ ] **Add DbSets to `AppTrackDatabaseContext`**

Add these four `DbSet` properties after the existing ones (before `OnModelCreating`):

```csharp
public DbSet<RssPortal> RssPortals { get; set; }
public DbSet<UserRssSubscription> UserRssSubscriptions { get; set; }
public DbSet<RssMonitoringSettings> RssMonitoringSettings { get; set; }
public DbSet<ProcessedFeedItem> ProcessedFeedItems { get; set; }
```

Also add `using AppTrack.Domain;` at the top if not already present (it is).

- [ ] **Create repositories**

```csharp
// AppTrack.Persistance/Repositories/RssPortalRepository.cs
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class RssPortalRepository : GenericRepository<RssPortal>, IRssPortalRepository
{
    public RssPortalRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<List<RssPortal>> GetAllActiveAsync()
        => await _context.RssPortals.AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync();
}
```

```csharp
// AppTrack.Persistance/Repositories/UserRssSubscriptionRepository.cs
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class UserRssSubscriptionRepository : GenericRepository<UserRssSubscription>, IUserRssSubscriptionRepository
{
    public UserRssSubscriptionRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<List<UserRssSubscription>> GetActiveSubscriptionsWithPortalsAsync()
        => await _context.UserRssSubscriptions
            .Include(s => s.RssPortal)
            .Where(s => s.IsActive && s.RssPortal.IsActive)
            .ToListAsync();

    public async Task<List<UserRssSubscription>> GetByUserIdAsync(string userId)
        => await _context.UserRssSubscriptions.AsNoTracking()
            .Where(s => s.UserId == userId)
            .ToListAsync();

    public async Task<UserRssSubscription?> GetByUserAndPortalAsync(string userId, int portalId)
        => await _context.UserRssSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.RssPortalId == portalId);

    public async Task UpsertAsync(string userId, int portalId, bool isActive)
    {
        var existing = await GetByUserAndPortalAsync(userId, portalId);
        if (existing is null)
        {
            await _context.UserRssSubscriptions.AddAsync(
                new UserRssSubscription { UserId = userId, RssPortalId = portalId, IsActive = isActive });
        }
        else
        {
            existing.IsActive = isActive;
            _context.UserRssSubscriptions.Update(existing);
        }
        await _context.SaveChangesAsync();
    }
}
```

```csharp
// AppTrack.Persistance/Repositories/RssMonitoringSettingsRepository.cs
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class RssMonitoringSettingsRepository : GenericRepository<RssMonitoringSettings>, IRssMonitoringSettingsRepository
{
    public RssMonitoringSettingsRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<RssMonitoringSettings?> GetByUserIdAsync(string userId)
        => await _context.RssMonitoringSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task UpsertAsync(RssMonitoringSettings settings)
    {
        var existing = await _context.RssMonitoringSettings
            .FirstOrDefaultAsync(s => s.UserId == settings.UserId);
        if (existing is null)
        {
            await _context.RssMonitoringSettings.AddAsync(settings);
        }
        else
        {
            existing.Keywords = settings.Keywords;
            existing.PollIntervalMinutes = settings.PollIntervalMinutes;
            _context.RssMonitoringSettings.Update(existing);
        }
        await _context.SaveChangesAsync();
    }
}
```

```csharp
// AppTrack.Persistance/Repositories/ProcessedFeedItemRepository.cs
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class ProcessedFeedItemRepository : GenericRepository<ProcessedFeedItem>, IProcessedFeedItemRepository
{
    public ProcessedFeedItemRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<HashSet<string>> GetProcessedUrlsAsync(string userId, IEnumerable<string> urls)
    {
        var urlList = urls.ToList();
        var processed = await _context.ProcessedFeedItems.AsNoTracking()
            .Where(p => p.UserId == userId && urlList.Contains(p.FeedItemUrl))
            .Select(p => p.FeedItemUrl)
            .ToListAsync();
        return processed.ToHashSet();
    }

    public async Task AddRangeAsync(IEnumerable<ProcessedFeedItem> items, CancellationToken ct)
    {
        await _context.ProcessedFeedItems.AddRangeAsync(items, ct);
        await _context.SaveChangesAsync(ct);
    }
}
```

- [ ] **Register repositories in `PersistanceServiceRegistration.cs`**

Add these four lines after the existing `AddScoped` registrations:

```csharp
services.AddScoped<IRssPortalRepository, RssPortalRepository>();
services.AddScoped<IUserRssSubscriptionRepository, UserRssSubscriptionRepository>();
services.AddScoped<IRssMonitoringSettingsRepository, RssMonitoringSettingsRepository>();
services.AddScoped<IProcessedFeedItemRepository, ProcessedFeedItemRepository>();
```

Also add the using:
```csharp
using AppTrack.Application.Contracts.RssFeed;
```

- [ ] **Build and verify 0 errors**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

- [ ] **Generate EF Core migration**

```bash
dotnet ef migrations add AddRssFeedMonitoring --project AppTrack.Persistance --startup-project AppTrack.Api
```

Expected: Migration files created under `AppTrack.Persistance/Migrations/`. Review the generated migration to verify:
- 4 new tables: `RssPortals`, `UserRssSubscriptions`, `RssMonitoringSettings`, `ProcessedFeedItems`
- Unique indexes on `UserRssSubscriptions(UserId, RssPortalId)` and `ProcessedFeedItems(UserId, FeedItemUrl)`
- Seed data: 1 `RssPortal` entry for Stepstone
- `Keywords` column: `nvarchar(max)`

- [ ] **Commit**

```bash
git add AppTrack.Persistance/ AppTrack.Domain/RssPortal.cs AppTrack.Domain/UserRssSubscription.cs AppTrack.Domain/RssMonitoringSettings.cs AppTrack.Domain/ProcessedFeedItem.cs
git commit -m "feat: add RSS persistence layer — configurations, repositories, migration"
```

---

## Chunk 2: Application Layer — Commands and Queries

### Task 5: `UpdateRssMonitoringSettingsCommand`

**Files:**
- Create: `AppTrack.Application/Features/RssFeeds/Commands/UpdateRssMonitoringSettings/UpdateRssMonitoringSettingsCommand.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Commands/UpdateRssMonitoringSettings/UpdateRssMonitoringSettingsCommandHandler.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Commands/UpdateRssMonitoringSettings/UpdateRssMonitoringSettingsCommandValidator.cs`
- Create: `AppTrack.Application.UnitTests/Features/RssFeeds/Commands/UpdateRssMonitoringSettingsCommandHandlerTests.cs`
- Create: `AppTrack.Application.UnitTests/Features/RssFeeds/Validators/UpdateRssMonitoringSettingsCommandValidatorTests.cs`

- [ ] **Write failing unit tests first**

```csharp
// AppTrack.Application.UnitTests/Features/RssFeeds/Commands/UpdateRssMonitoringSettingsCommandHandlerTests.cs
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Commands.UpdateRssMonitoringSettings;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Commands;

public class UpdateRssMonitoringSettingsCommandHandlerTests
{
    private readonly Mock<IRssMonitoringSettingsRepository> _mockRepo;
    private readonly Mock<IValidator<UpdateRssMonitoringSettingsCommand>> _mockValidator;

    public UpdateRssMonitoringSettingsCommandHandlerTests()
    {
        _mockRepo = new Mock<IRssMonitoringSettingsRepository>();
        _mockValidator = new Mock<IValidator<UpdateRssMonitoringSettingsCommand>>();
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateRssMonitoringSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private UpdateRssMonitoringSettingsCommandHandler CreateHandler() =>
        new(_mockRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldCallUpsert_WhenCommandIsValid()
    {
        var command = new UpdateRssMonitoringSettingsCommand
        {
            UserId = "user-1",
            Keywords = ["dotnet", "azure"],
            PollIntervalMinutes = 60
        };

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockRepo.Verify(r => r.UpsertAsync(It.Is<RssMonitoringSettings>(
            s => s.UserId == "user-1" &&
                 s.Keywords.SequenceEqual(["dotnet", "azure"]) &&
                 s.PollIntervalMinutes == 60)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnit_WhenCommandIsValid()
    {
        var command = new UpdateRssMonitoringSettingsCommand
        {
            UserId = "user-1",
            Keywords = ["dotnet"],
            PollIntervalMinutes = 30
        };

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.ShouldBe(Unit.Value);
    }
}
```

```csharp
// AppTrack.Application.UnitTests/Features/RssFeeds/Validators/UpdateRssMonitoringSettingsCommandValidatorTests.cs
using AppTrack.Application.Features.RssFeeds.Commands.UpdateRssMonitoringSettings;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Validators;

public class UpdateRssMonitoringSettingsCommandValidatorTests
{
    private readonly UpdateRssMonitoringSettingsCommandValidator _sut = new();

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new UpdateRssMonitoringSettingsCommand
        {
            UserId = "user-1",
            Keywords = ["dotnet"],
            PollIntervalMinutes = 60
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenKeywordsIsNull()
    {
        var command = new UpdateRssMonitoringSettingsCommand
        {
            UserId = "user-1",
            Keywords = null!,
            PollIntervalMinutes = 60
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1441)]
    public async Task Validate_ShouldFail_WhenIntervalIsOutOfRange(int interval)
    {
        var command = new UpdateRssMonitoringSettingsCommand
        {
            UserId = "user-1",
            Keywords = [],
            PollIntervalMinutes = interval
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
    }
}
```

- [ ] **Run tests — expect compile failure (types don't exist yet)**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

Expected: Build errors referencing missing types.

- [ ] **Implement command, handler, and validator**

```csharp
// AppTrack.Application/Features/RssFeeds/Commands/UpdateRssMonitoringSettings/UpdateRssMonitoringSettingsCommand.cs
using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.RssFeeds.Commands.UpdateRssMonitoringSettings;

public class UpdateRssMonitoringSettingsCommand : IRequest<Unit>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = [];
    public int PollIntervalMinutes { get; set; }
}
```

```csharp
// AppTrack.Application/Features/RssFeeds/Commands/UpdateRssMonitoringSettings/UpdateRssMonitoringSettingsCommandHandler.cs
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using FluentValidation;

namespace AppTrack.Application.Features.RssFeeds.Commands.UpdateRssMonitoringSettings;

public class UpdateRssMonitoringSettingsCommandHandler
    : IRequestHandler<UpdateRssMonitoringSettingsCommand, Unit>
{
    private readonly IRssMonitoringSettingsRepository _repository;
    private readonly IValidator<UpdateRssMonitoringSettingsCommand> _validator;

    public UpdateRssMonitoringSettingsCommandHandler(
        IRssMonitoringSettingsRepository repository,
        IValidator<UpdateRssMonitoringSettingsCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Unit> Handle(UpdateRssMonitoringSettingsCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid request", validationResult);

        await _repository.UpsertAsync(new RssMonitoringSettings
        {
            UserId = request.UserId,
            Keywords = request.Keywords,
            PollIntervalMinutes = request.PollIntervalMinutes
        });

        return Unit.Value;
    }
}
```

```csharp
// AppTrack.Application/Features/RssFeeds/Commands/UpdateRssMonitoringSettings/UpdateRssMonitoringSettingsCommandValidator.cs
using FluentValidation;

namespace AppTrack.Application.Features.RssFeeds.Commands.UpdateRssMonitoringSettings;

public class UpdateRssMonitoringSettingsCommandValidator
    : AbstractValidator<UpdateRssMonitoringSettingsCommand>
{
    public UpdateRssMonitoringSettingsCommandValidator()
    {
        RuleFor(x => x.Keywords).NotNull();
        RuleFor(x => x.PollIntervalMinutes).InclusiveBetween(5, 1440);
    }
}
```

- [ ] **Run tests — expect all pass**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

Expected: All tests pass.

- [ ] **Commit**

```bash
git add AppTrack.Application/Features/RssFeeds/Commands/UpdateRssMonitoringSettings/ AppTrack.Application.UnitTests/Features/RssFeeds/
git commit -m "feat: add UpdateRssMonitoringSettingsCommand with handler, validator and tests"
```

---

### Task 6: `SetRssSubscriptionsCommand`

**Files:**
- Create: `AppTrack.Application/Features/RssFeeds/Commands/SetRssSubscriptions/SetRssSubscriptionsCommand.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Commands/SetRssSubscriptions/SetRssSubscriptionsCommandHandler.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Commands/SetRssSubscriptions/SetRssSubscriptionsCommandValidator.cs`
- Create: `AppTrack.Application.UnitTests/Features/RssFeeds/Commands/SetRssSubscriptionsCommandHandlerTests.cs`
- Create: `AppTrack.Application.UnitTests/Features/RssFeeds/Validators/SetRssSubscriptionsCommandValidatorTests.cs`

- [ ] **Write failing unit tests first**

```csharp
// AppTrack.Application.UnitTests/Features/RssFeeds/Commands/SetRssSubscriptionsCommandHandlerTests.cs
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Commands.SetRssSubscriptions;
using AppTrack.Application.Features.RssFeeds.Dto;
using AppTrack.Application.Shared;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Commands;

public class SetRssSubscriptionsCommandHandlerTests
{
    private readonly Mock<IUserRssSubscriptionRepository> _mockRepo;
    private readonly Mock<IValidator<SetRssSubscriptionsCommand>> _mockValidator;

    public SetRssSubscriptionsCommandHandlerTests()
    {
        _mockRepo = new Mock<IUserRssSubscriptionRepository>();
        _mockValidator = new Mock<IValidator<SetRssSubscriptionsCommand>>();
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<SetRssSubscriptionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private SetRssSubscriptionsCommandHandler CreateHandler() =>
        new(_mockRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldCallUpsert_ForEachSubscription()
    {
        var command = new SetRssSubscriptionsCommand
        {
            UserId = "user-1",
            Subscriptions = [new(1, true), new(2, false)]
        };

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockRepo.Verify(r => r.UpsertAsync("user-1", 1, true), Times.Once);
        _mockRepo.Verify(r => r.UpsertAsync("user-1", 2, false), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnit()
    {
        var command = new SetRssSubscriptionsCommand
        {
            UserId = "user-1",
            Subscriptions = [new(1, true)]
        };

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.ShouldBe(Unit.Value);
    }
}
```

```csharp
// AppTrack.Application.UnitTests/Features/RssFeeds/Validators/SetRssSubscriptionsCommandValidatorTests.cs
using AppTrack.Application.Features.RssFeeds.Commands.SetRssSubscriptions;
using AppTrack.Application.Features.RssFeeds.Dto;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Validators;

public class SetRssSubscriptionsCommandValidatorTests
{
    private readonly SetRssSubscriptionsCommandValidator _sut = new();

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new SetRssSubscriptionsCommand
        {
            UserId = "user-1",
            Subscriptions = [new(1, true)]
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenSubscriptionsIsNull()
    {
        var command = new SetRssSubscriptionsCommand
        {
            UserId = "user-1",
            Subscriptions = null!
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenPortalIdIsZero()
    {
        var command = new SetRssSubscriptionsCommand
        {
            UserId = "user-1",
            Subscriptions = [new(0, true)]
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
    }
}
```

- [ ] **Implement command, handler, validator**

```csharp
// AppTrack.Application/Features/RssFeeds/Commands/SetRssSubscriptions/SetRssSubscriptionsCommand.cs
using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.RssFeeds.Dto;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.RssFeeds.Commands.SetRssSubscriptions;

public class SetRssSubscriptionsCommand : IRequest<Unit>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
    public List<RssSubscriptionItemDto> Subscriptions { get; set; } = [];
}
```

```csharp
// AppTrack.Application/Features/RssFeeds/Commands/SetRssSubscriptions/SetRssSubscriptionsCommandHandler.cs
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;
using FluentValidation;

namespace AppTrack.Application.Features.RssFeeds.Commands.SetRssSubscriptions;

public class SetRssSubscriptionsCommandHandler : IRequestHandler<SetRssSubscriptionsCommand, Unit>
{
    private readonly IUserRssSubscriptionRepository _repository;
    private readonly IValidator<SetRssSubscriptionsCommand> _validator;

    public SetRssSubscriptionsCommandHandler(
        IUserRssSubscriptionRepository repository,
        IValidator<SetRssSubscriptionsCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Unit> Handle(SetRssSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid request", validationResult);

        foreach (var subscription in request.Subscriptions)
            await _repository.UpsertAsync(request.UserId, subscription.PortalId, subscription.IsActive);

        return Unit.Value;
    }
}
```

```csharp
// AppTrack.Application/Features/RssFeeds/Commands/SetRssSubscriptions/SetRssSubscriptionsCommandValidator.cs
using FluentValidation;

namespace AppTrack.Application.Features.RssFeeds.Commands.SetRssSubscriptions;

public class SetRssSubscriptionsCommandValidator : AbstractValidator<SetRssSubscriptionsCommand>
{
    public SetRssSubscriptionsCommandValidator()
    {
        RuleFor(x => x.Subscriptions).NotNull();
        RuleForEach(x => x.Subscriptions).ChildRules(sub =>
            sub.RuleFor(s => s.PortalId).GreaterThan(0));
    }
}
```

- [ ] **Run tests — expect all pass**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

- [ ] **Commit**

```bash
git add AppTrack.Application/Features/RssFeeds/Commands/SetRssSubscriptions/ AppTrack.Application.UnitTests/Features/RssFeeds/
git commit -m "feat: add SetRssSubscriptionsCommand with handler, validator and tests"
```

---

### Task 7: `GetRssPortalsQuery` and `GetRssMonitoringSettingsQuery`

**Files:**
- Create: `AppTrack.Application/Features/RssFeeds/Queries/GetRssPortals/GetRssPortalsQuery.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Queries/GetRssPortals/GetRssPortalsQueryHandler.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Queries/GetRssMonitoringSettings/GetRssMonitoringSettingsQuery.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Queries/GetRssMonitoringSettings/GetRssMonitoringSettingsQueryHandler.cs`
- Create: `AppTrack.Application.UnitTests/Features/RssFeeds/Queries/GetRssPortalsQueryHandlerTests.cs`
- Create: `AppTrack.Application.UnitTests/Features/RssFeeds/Queries/GetRssMonitoringSettingsQueryHandlerTests.cs`

- [ ] **Write failing unit tests**

```csharp
// AppTrack.Application.UnitTests/Features/RssFeeds/Queries/GetRssPortalsQueryHandlerTests.cs
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Dto;
using AppTrack.Application.Features.RssFeeds.Queries.GetRssPortals;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Queries;

public class GetRssPortalsQueryHandlerTests
{
    private readonly Mock<IRssPortalRepository> _mockPortalRepo;
    private readonly Mock<IUserRssSubscriptionRepository> _mockSubRepo;

    public GetRssPortalsQueryHandlerTests()
    {
        _mockPortalRepo = new Mock<IRssPortalRepository>();
        _mockSubRepo = new Mock<IUserRssSubscriptionRepository>();

        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([
            new RssPortal { Id = 1, Name = "Stepstone", Url = "https://stepstone.de/rss", ParserType = RssParserType.Stepstone, IsActive = true }
        ]);
    }

    private GetRssPortalsQueryHandler CreateHandler() =>
        new(_mockPortalRepo.Object, _mockSubRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnAllActivePortals()
    {
        _mockSubRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync([]);

        var result = await CreateHandler().Handle(
            new GetRssPortalsQuery { UserId = "user-1" }, CancellationToken.None);

        result.ShouldNotBeEmpty();
        result.ShouldContain(p => p.Name == "Stepstone");
    }

    [Fact]
    public async Task Handle_ShouldSetIsSubscribedTrue_WhenUserHasActiveSubscription()
    {
        _mockSubRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync([
            new UserRssSubscription { RssPortalId = 1, IsActive = true, UserId = "user-1" }
        ]);

        var result = await CreateHandler().Handle(
            new GetRssPortalsQuery { UserId = "user-1" }, CancellationToken.None);

        result.Single(p => p.Id == 1).IsSubscribed.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSetIsSubscribedFalse_WhenNoSubscriptionExists()
    {
        _mockSubRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync([]);

        var result = await CreateHandler().Handle(
            new GetRssPortalsQuery { UserId = "user-1" }, CancellationToken.None);

        result.Single(p => p.Id == 1).IsSubscribed.ShouldBeFalse();
    }
}
```

```csharp
// AppTrack.Application.UnitTests/Features/RssFeeds/Queries/GetRssMonitoringSettingsQueryHandlerTests.cs
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Dto;
using AppTrack.Application.Features.RssFeeds.Queries.GetRssMonitoringSettings;
using AppTrack.Domain;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Queries;

public class GetRssMonitoringSettingsQueryHandlerTests
{
    private readonly Mock<IRssMonitoringSettingsRepository> _mockRepo;

    public GetRssMonitoringSettingsQueryHandlerTests()
    {
        _mockRepo = new Mock<IRssMonitoringSettingsRepository>();
    }

    private GetRssMonitoringSettingsQueryHandler CreateHandler() =>
        new(_mockRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnDefaults_WhenNoSettingsExist()
    {
        _mockRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync((RssMonitoringSettings?)null);

        var result = await CreateHandler().Handle(
            new GetRssMonitoringSettingsQuery { UserId = "user-1" }, CancellationToken.None);

        result.Keywords.ShouldBeEmpty();
        result.PollIntervalMinutes.ShouldBe(60);
    }

    [Fact]
    public async Task Handle_ShouldReturnPersistedSettings_WhenSettingsExist()
    {
        _mockRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync(
            new RssMonitoringSettings { UserId = "user-1", Keywords = ["dotnet"], PollIntervalMinutes = 30 });

        var result = await CreateHandler().Handle(
            new GetRssMonitoringSettingsQuery { UserId = "user-1" }, CancellationToken.None);

        result.Keywords.ShouldBe(["dotnet"]);
        result.PollIntervalMinutes.ShouldBe(30);
    }
}
```

- [ ] **Implement queries and handlers**

```csharp
// AppTrack.Application/Features/RssFeeds/Queries/GetRssPortals/GetRssPortalsQuery.cs
using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.RssFeeds.Dto;

namespace AppTrack.Application.Features.RssFeeds.Queries.GetRssPortals;

public class GetRssPortalsQuery : IRequest<List<RssPortalDto>>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
```

```csharp
// AppTrack.Application/Features/RssFeeds/Queries/GetRssPortals/GetRssPortalsQueryHandler.cs
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Dto;

namespace AppTrack.Application.Features.RssFeeds.Queries.GetRssPortals;

public class GetRssPortalsQueryHandler : IRequestHandler<GetRssPortalsQuery, List<RssPortalDto>>
{
    private readonly IRssPortalRepository _portalRepository;
    private readonly IUserRssSubscriptionRepository _subscriptionRepository;

    public GetRssPortalsQueryHandler(
        IRssPortalRepository portalRepository,
        IUserRssSubscriptionRepository subscriptionRepository)
    {
        _portalRepository = portalRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<List<RssPortalDto>> Handle(GetRssPortalsQuery request, CancellationToken cancellationToken)
    {
        var portals = await _portalRepository.GetAllActiveAsync();
        var userSubscriptions = await _subscriptionRepository.GetByUserIdAsync(request.UserId);
        var activePortalIds = userSubscriptions
            .Where(s => s.IsActive)
            .Select(s => s.RssPortalId)
            .ToHashSet();

        return portals
            .Select(p => new RssPortalDto(p.Id, p.Name, activePortalIds.Contains(p.Id)))
            .ToList();
    }
}
```

```csharp
// AppTrack.Application/Features/RssFeeds/Queries/GetRssMonitoringSettings/GetRssMonitoringSettingsQuery.cs
using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.RssFeeds.Dto;

namespace AppTrack.Application.Features.RssFeeds.Queries.GetRssMonitoringSettings;

public class GetRssMonitoringSettingsQuery : IRequest<RssMonitoringSettingsDto>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
```

```csharp
// AppTrack.Application/Features/RssFeeds/Queries/GetRssMonitoringSettings/GetRssMonitoringSettingsQueryHandler.cs
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Dto;

namespace AppTrack.Application.Features.RssFeeds.Queries.GetRssMonitoringSettings;

public class GetRssMonitoringSettingsQueryHandler
    : IRequestHandler<GetRssMonitoringSettingsQuery, RssMonitoringSettingsDto>
{
    private readonly IRssMonitoringSettingsRepository _repository;

    public GetRssMonitoringSettingsQueryHandler(IRssMonitoringSettingsRepository repository)
        => _repository = repository;

    public async Task<RssMonitoringSettingsDto> Handle(
        GetRssMonitoringSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByUserIdAsync(request.UserId);
        return settings is null
            ? new RssMonitoringSettingsDto([], 60)
            : new RssMonitoringSettingsDto(settings.Keywords, settings.PollIntervalMinutes);
    }
}
```

- [ ] **Run tests — expect all pass**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

- [ ] **Commit**

```bash
git add AppTrack.Application/Features/RssFeeds/Queries/ AppTrack.Application.UnitTests/Features/RssFeeds/Queries/
git commit -m "feat: add GetRssPortals and GetRssMonitoringSettings queries with tests"
```

---

## Chunk 3: PollRssFeedsCommandHandler

### Task 8: `PollRssFeedsCommand` and handler

**Files:**
- Create: `AppTrack.Application/Features/RssFeeds/Commands/PollRssFeeds/PollRssFeedsCommand.cs`
- Create: `AppTrack.Application/Features/RssFeeds/Commands/PollRssFeeds/PollRssFeedsCommandHandler.cs`
- Create: `AppTrack.Application.UnitTests/Features/RssFeeds/Commands/PollRssFeedsCommandHandlerTests.cs`

- [ ] **Write failing unit tests first**

```csharp
// AppTrack.Application.UnitTests/Features/RssFeeds/Commands/PollRssFeedsCommandHandlerTests.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Commands.PollRssFeeds;
using AppTrack.Application.Features.RssFeeds.Models;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Commands;

public class PollRssFeedsCommandHandlerTests
{
    private readonly Mock<IUserRssSubscriptionRepository> _mockSubRepo;
    private readonly Mock<IRssMonitoringSettingsRepository> _mockSettingsRepo;
    private readonly Mock<IJobApplicationRepository> _mockJobAppRepo;
    private readonly Mock<IProcessedFeedItemRepository> _mockProcessedRepo;
    private readonly Mock<IRssFeedReader> _mockFeedReader;
    private readonly Mock<IRssFeedItemParser> _mockParser;
    private readonly Mock<IRssMatchNotifier> _mockNotifier;
    private readonly Mock<IUnitOfWork> _mockUow;

    private const string UserId = "user-1";

    private static readonly RssPortal Portal = new()
    {
        Id = 1, Name = "Stepstone", Url = "https://stepstone.de/rss",
        ParserType = RssParserType.Stepstone, IsActive = true
    };

    private static readonly UserRssSubscription Subscription = new()
    {
        Id = 1, UserId = UserId, RssPortalId = 1, IsActive = true,
        LastPolledAt = null, RssPortal = Portal
    };

    private static readonly RssMonitoringSettings Settings = new()
    {
        UserId = UserId, Keywords = ["dotnet"], PollIntervalMinutes = 60
    };

    private static readonly RawFeedItem MatchingItem = new(
        "Senior .NET Developer", "https://stepstone.de/job/1", "dotnet azure", DateTime.UtcNow);

    private static readonly RawFeedItem NonMatchingItem = new(
        "Marketing Manager", "https://stepstone.de/job/2", "brand strategy", DateTime.UtcNow);

    private static readonly RssJobApplicationData ParsedData = new(
        "Senior .NET Developer", "https://stepstone.de/job/1", "dotnet azure", "", "Stepstone");

    public PollRssFeedsCommandHandlerTests()
    {
        _mockSubRepo = new Mock<IUserRssSubscriptionRepository>();
        _mockSettingsRepo = new Mock<IRssMonitoringSettingsRepository>();
        _mockJobAppRepo = new Mock<IJobApplicationRepository>();
        _mockProcessedRepo = new Mock<IProcessedFeedItemRepository>();
        _mockFeedReader = new Mock<IRssFeedReader>();
        _mockParser = new Mock<IRssFeedItemParser>();
        _mockNotifier = new Mock<IRssMatchNotifier>();
        _mockUow = new Mock<IUnitOfWork>();

        _mockSubRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync())
            .ReturnsAsync([Subscription]);
        _mockSettingsRepo.Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync(Settings);
        _mockFeedReader.Setup(r => r.ReadAsync(Portal.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync([MatchingItem, NonMatchingItem]);
        _mockProcessedRepo.Setup(r => r.GetProcessedUrlsAsync(UserId, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([]);
        _mockParser.Setup(p => p.Parse(MatchingItem, RssParserType.Stepstone, "Stepstone"))
            .Returns(ParsedData);
        _mockParser.Setup(p => p.Parse(NonMatchingItem, RssParserType.Stepstone, "Stepstone"))
            .Returns(new RssJobApplicationData("Marketing Manager", "https://stepstone.de/job/2", "brand strategy", "", "Stepstone"));
        _mockUow.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(async (action, ct) => await action(ct));
        _mockJobAppRepo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>()))
            .Returns(Task.CompletedTask);
        _mockProcessedRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProcessedFeedItem>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockSubRepo.Setup(r => r.UpdateAsync(It.IsAny<UserRssSubscription>()))
            .Returns(Task.CompletedTask);
    }

    private PollRssFeedsCommandHandler CreateHandler() => new(
        _mockSubRepo.Object,
        _mockSettingsRepo.Object,
        _mockJobAppRepo.Object,
        _mockProcessedRepo.Object,
        _mockFeedReader.Object,
        _mockParser.Object,
        _mockNotifier.Object,
        _mockUow.Object);

    [Fact]
    public async Task Handle_ShouldCreateJobApplication_ForMatchingItem()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(
            j => j.Position == "Senior .NET Developer" &&
                 j.Status == JobApplicationStatus.Discovered &&
                 j.UserId == UserId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCreateJobApplication_ForNonMatchingItem()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(
            j => j.Position == "Marketing Manager")), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipAlreadyProcessedItems()
    {
        _mockProcessedRepo.Setup(r => r.GetProcessedUrlsAsync(UserId, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(["https://stepstone.de/job/1"]);

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.IsAny<JobApplication>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSendNotification_WhenMatchesFound()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockNotifier.Verify(n => n.NotifyAsync(
            UserId,
            It.Is<List<RssJobApplicationData>>(m => m.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotSendNotification_WhenNoMatchesFound()
    {
        _mockFeedReader.Setup(r => r.ReadAsync(Portal.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync([NonMatchingItem]);

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockNotifier.Verify(n => n.NotifyAsync(
            It.IsAny<string>(), It.IsAny<List<RssJobApplicationData>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipUser_WhenNoSettingsExist()
    {
        _mockSettingsRepo.Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync((RssMonitoringSettings?)null);

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockFeedReader.Verify(r => r.ReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipUser_WhenKeywordsAreEmpty()
    {
        _mockSettingsRepo.Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync(new RssMonitoringSettings { UserId = UserId, Keywords = [], PollIntervalMinutes = 60 });

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockFeedReader.Verify(r => r.ReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipSubscription_WhenNotDue()
    {
        var notDueSub = new UserRssSubscription
        {
            Id = 1, UserId = UserId, RssPortalId = 1, IsActive = true,
            LastPolledAt = DateTime.UtcNow.AddMinutes(-30),
            RssPortal = Portal
        };
        _mockSubRepo.Setup(r => r.GetActiveSubscriptionsWithPortalsAsync()).ReturnsAsync([notDueSub]);

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockFeedReader.Verify(r => r.ReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMarkItemsAsProcessed_AfterMatching()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockProcessedRepo.Verify(r => r.AddRangeAsync(
            It.Is<IEnumerable<ProcessedFeedItem>>(items =>
                items.Any(i => i.FeedItemUrl == "https://stepstone.de/job/1" && i.UserId == UserId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateLastPolledAt_AfterProcessing()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockSubRepo.Verify(r => r.UpdateAsync(It.Is<UserRssSubscription>(
            s => s.LastPolledAt != null)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseTransaction_ForPersistenceOperations()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockUow.Verify(u => u.ExecuteInTransactionAsync(
            It.IsAny<Func<CancellationToken, Task>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetJobApplicationName_ToPortalName_WhenCompanyNameEmpty()
    {
        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(
            j => j.Name == "Stepstone")), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetJobApplicationName_ToCompanyName_WhenAvailable()
    {
        _mockParser.Setup(p => p.Parse(MatchingItem, RssParserType.Stepstone, "Stepstone"))
            .Returns(ParsedData with { CompanyName = "Acme GmbH" });

        await CreateHandler().Handle(new PollRssFeedsCommand(), CancellationToken.None);

        _mockJobAppRepo.Verify(r => r.CreateAsync(It.Is<JobApplication>(
            j => j.Name == "Acme GmbH")), Times.Once);
    }
}
```

- [ ] **Run tests — expect compile failure (types don't exist yet)**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

- [ ] **Implement `PollRssFeedsCommand` and handler**

```csharp
// AppTrack.Application/Features/RssFeeds/Commands/PollRssFeeds/PollRssFeedsCommand.cs
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.RssFeeds.Commands.PollRssFeeds;

public class PollRssFeedsCommand : IRequest<Unit> { }
```

```csharp
// AppTrack.Application/Features/RssFeeds/Commands/PollRssFeeds/PollRssFeedsCommandHandler.cs
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using AppTrack.Domain.Enums;

namespace AppTrack.Application.Features.RssFeeds.Commands.PollRssFeeds;

public class PollRssFeedsCommandHandler : IRequestHandler<PollRssFeedsCommand, Unit>
{
    private readonly IUserRssSubscriptionRepository _subscriptionRepository;
    private readonly IRssMonitoringSettingsRepository _settingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IProcessedFeedItemRepository _processedRepository;
    private readonly IRssFeedReader _feedReader;
    private readonly IRssFeedItemParser _parser;
    private readonly IRssMatchNotifier _notifier;
    private readonly IUnitOfWork _unitOfWork;

    public PollRssFeedsCommandHandler(
        IUserRssSubscriptionRepository subscriptionRepository,
        IRssMonitoringSettingsRepository settingsRepository,
        IJobApplicationRepository jobApplicationRepository,
        IProcessedFeedItemRepository processedRepository,
        IRssFeedReader feedReader,
        IRssFeedItemParser parser,
        IRssMatchNotifier notifier,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _settingsRepository = settingsRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _processedRepository = processedRepository;
        _feedReader = feedReader;
        _parser = parser;
        _notifier = notifier;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(PollRssFeedsCommand request, CancellationToken cancellationToken)
    {
        var allActiveSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsWithPortalsAsync();
        var byUser = allActiveSubscriptions.GroupBy(s => s.UserId);

        foreach (var userGroup in byUser)
        {
            var userId = userGroup.Key;
            var settings = await _settingsRepository.GetByUserIdAsync(userId);

            if (settings is null || settings.Keywords.Count == 0)
                continue;

            var now = DateTime.UtcNow;
            var dueSubscriptions = userGroup
                .Where(s => s.LastPolledAt is null ||
                            now >= s.LastPolledAt.Value.AddMinutes(settings.PollIntervalMinutes))
                .ToList();

            if (dueSubscriptions.Count == 0)
                continue;

            var allMatches = new List<RssJobApplicationData>();

            foreach (var subscription in dueSubscriptions)
            {
                var items = await _feedReader.ReadAsync(subscription.RssPortal.Url, cancellationToken);
                var urls = items.Select(i => i.Url).ToList();
                var processedUrls = await _processedRepository.GetProcessedUrlsAsync(userId, urls);

                var newItems = items.Where(i => !processedUrls.Contains(i.Url)).ToList();
                var matches = newItems
                    .Where(i => MatchesKeywords(i, settings.Keywords))
                    .Select(i => _parser.Parse(i, subscription.RssPortal.ParserType, subscription.RssPortal.Name))
                    .ToList();

                allMatches.AddRange(matches);

                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    foreach (var match in matches)
                    {
                        await _jobApplicationRepository.CreateAsync(new JobApplication
                        {
                            UserId = userId,
                            Name = string.IsNullOrEmpty(match.CompanyName) ? match.PortalName : match.CompanyName,
                            Position = match.Position,
                            URL = match.Url,
                            JobDescription = match.JobDescription,
                            Location = string.Empty,
                            ContactPerson = string.Empty,
                            DurationInMonths = string.Empty,
                            StartDate = DateTime.UtcNow,
                            Status = JobApplicationStatus.Discovered
                        });
                    }

                    var processedItems = newItems.Select(i => new ProcessedFeedItem
                    {
                        UserId = userId,
                        FeedItemUrl = i.Url,
                        ProcessedAt = DateTime.UtcNow
                    });
                    await _processedRepository.AddRangeAsync(processedItems, ct);

                    subscription.LastPolledAt = DateTime.UtcNow;
                    await _subscriptionRepository.UpdateAsync(subscription);
                }, cancellationToken);
            }

            if (allMatches.Count > 0)
                await _notifier.NotifyAsync(userId, allMatches, cancellationToken);
        }

        return Unit.Value;
    }

    private static bool MatchesKeywords(RawFeedItem item, List<string> keywords) =>
        keywords.Any(kw =>
            item.Title.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
            item.Description.Contains(kw, StringComparison.OrdinalIgnoreCase));
}
```

- [ ] **Run tests — expect all pass**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

Expected: All tests pass including the 11 new handler tests.

- [ ] **Commit**

```bash
git add AppTrack.Application/Features/RssFeeds/Commands/PollRssFeeds/ AppTrack.Application.UnitTests/Features/RssFeeds/Commands/PollRssFeedsCommandHandlerTests.cs
git commit -m "feat: add PollRssFeedsCommandHandler with comprehensive unit tests"
```

---

## Chunk 4: Infrastructure + API Layer

### Task 9: NuGet packages and parser infrastructure

**Files:**
- Modify: `AppTrack.Infrastructure/AppTrack.Infrastructure.csproj`
- Create: `AppTrack.Infrastructure/RssFeed/Parsers/IRssFeedParser.cs`
- Create: `AppTrack.Infrastructure/RssFeed/Parsers/DefaultFeedParser.cs`
- Create: `AppTrack.Infrastructure/RssFeed/Parsers/StepstoneFeedParser.cs`
- Create: `AppTrack.Infrastructure/RssFeed/Parsers/RssFeedParserFactory.cs`
- Create: `AppTrack.Infrastructure/RssFeed/RssFeedItemParser.cs`
- Create: `AppTrack.Application.UnitTests/Features/RssFeeds/Parsers/DefaultFeedParserTests.cs`

- [ ] **Add NuGet packages to Infrastructure**

In `AppTrack.Infrastructure/AppTrack.Infrastructure.csproj`, add inside the existing `<ItemGroup>` with `PackageReference` entries:

```xml
<PackageReference Include="CodeHollow.FeedReader" Version="1.2.6" />
<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.18.4" />
```

- [ ] **Add `AppTrack.Infrastructure` reference to unit test project**

In `AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj`, add:

```xml
<ProjectReference Include="..\AppTrack.Infrastructure\AppTrack.Infrastructure.csproj" />
```

- [ ] **Write failing parser unit tests**

```csharp
// AppTrack.Application.UnitTests/Features/RssFeeds/Parsers/DefaultFeedParserTests.cs
using AppTrack.Application.Features.RssFeeds.Models;
using AppTrack.Infrastructure.RssFeed.Parsers;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Parsers;

public class DefaultFeedParserTests
{
    private readonly DefaultFeedParser _sut = new();

    [Fact]
    public void Parse_ShouldMapTitleToPosition()
    {
        var item = new RawFeedItem("Senior .NET Developer", "https://example.com/1", "Great job", DateTime.UtcNow);
        var result = _sut.Parse(item, "Stepstone");
        result.Position.ShouldBe("Senior .NET Developer");
    }

    [Fact]
    public void Parse_ShouldMapUrlToUrl()
    {
        var item = new RawFeedItem("Title", "https://example.com/1", "Desc", DateTime.UtcNow);
        var result = _sut.Parse(item, "Stepstone");
        result.Url.ShouldBe("https://example.com/1");
    }

    [Fact]
    public void Parse_ShouldStripHtmlFromDescription()
    {
        var item = new RawFeedItem("Title", "https://example.com/1", "<p>Great <b>job</b></p>", DateTime.UtcNow);
        var result = _sut.Parse(item, "Stepstone");
        result.JobDescription.ShouldBe("Great job");
    }

    [Fact]
    public void Parse_ShouldSetCompanyNameToEmpty()
    {
        var item = new RawFeedItem("Title", "https://example.com/1", "Desc", DateTime.UtcNow);
        var result = _sut.Parse(item, "Stepstone");
        result.CompanyName.ShouldBeEmpty();
    }

    [Fact]
    public void Parse_ShouldSetPortalName()
    {
        var item = new RawFeedItem("Title", "https://example.com/1", "Desc", DateTime.UtcNow);
        var result = _sut.Parse(item, "Stepstone");
        result.PortalName.ShouldBe("Stepstone");
    }
}
```

- [ ] **Run tests — expect compile failure (types don't exist yet)**

```bash
dotnet build AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

- [ ] **Implement parser infrastructure**

```csharp
// AppTrack.Infrastructure/RssFeed/Parsers/IRssFeedParser.cs
using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Infrastructure.RssFeed.Parsers;

internal interface IRssFeedParser
{
    RssJobApplicationData Parse(RawFeedItem item, string portalName);
}
```

```csharp
// AppTrack.Infrastructure/RssFeed/Parsers/DefaultFeedParser.cs
using System.Text.RegularExpressions;
using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Infrastructure.RssFeed.Parsers;

public class DefaultFeedParser : IRssFeedParser
{
    public RssJobApplicationData Parse(RawFeedItem item, string portalName) =>
        new(
            Position: item.Title,
            Url: item.Url,
            JobDescription: StripHtml(item.Description),
            CompanyName: string.Empty,
            PortalName: portalName);

    private static string StripHtml(string html) =>
        Regex.Replace(html, "<[^>]*>", string.Empty).Trim();
}
```

```csharp
// AppTrack.Infrastructure/RssFeed/Parsers/StepstoneFeedParser.cs
using System.Text.RegularExpressions;
using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Infrastructure.RssFeed.Parsers;

/// <summary>
/// Stepstone-specific parser. Extends DefaultFeedParser behaviour
/// with any portal-specific field extraction. Placeholder for now.
/// </summary>
public class StepstoneFeedParser : IRssFeedParser
{
    public RssJobApplicationData Parse(RawFeedItem item, string portalName) =>
        new(
            Position: item.Title,
            Url: item.Url,
            JobDescription: Regex.Replace(item.Description, "<[^>]*>", string.Empty).Trim(),
            CompanyName: string.Empty,
            PortalName: portalName);
}
```

```csharp
// AppTrack.Infrastructure/RssFeed/Parsers/RssFeedParserFactory.cs
using AppTrack.Domain.Enums;

namespace AppTrack.Infrastructure.RssFeed.Parsers;

internal static class RssFeedParserFactory
{
    public static IRssFeedParser GetParser(RssParserType parserType) =>
        parserType switch
        {
            RssParserType.Stepstone => new StepstoneFeedParser(),
            _ => new DefaultFeedParser()
        };
}
```

```csharp
// AppTrack.Infrastructure/RssFeed/RssFeedItemParser.cs
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;
using AppTrack.Domain.Enums;
using AppTrack.Infrastructure.RssFeed.Parsers;

namespace AppTrack.Infrastructure.RssFeed;

public class RssFeedItemParser : IRssFeedItemParser
{
    public RssJobApplicationData Parse(RawFeedItem item, RssParserType parserType, string portalName)
    {
        var parser = RssFeedParserFactory.GetParser(parserType);
        return parser.Parse(item, portalName);
    }
}
```

- [ ] **Run parser tests — expect all pass**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

- [ ] **Commit**

```bash
git add AppTrack.Infrastructure/RssFeed/Parsers/ AppTrack.Infrastructure/RssFeed/RssFeedItemParser.cs AppTrack.Infrastructure/AppTrack.Infrastructure.csproj AppTrack.Application.UnitTests/Features/RssFeeds/Parsers/ AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj
git commit -m "feat: add RSS parser infrastructure (DefaultFeedParser, StepstoneFeedParser, factory)"
```

---

### Task 10: `RssFeedReader` and notification implementations

**Files:**
- Create: `AppTrack.Infrastructure/RssFeed/RssFeedReader.cs`
- Create: `AppTrack.Infrastructure/Notifications/DirectEmailNotifier.cs`
- Create: `AppTrack.Infrastructure/Notifications/ServiceBusNotifier.cs`

- [ ] **Implement `RssFeedReader`**

```csharp
// AppTrack.Infrastructure/RssFeed/RssFeedReader.cs
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;
using CodeHollow.FeedReader;

namespace AppTrack.Infrastructure.RssFeed;

public class RssFeedReader : IRssFeedReader
{
    public async Task<List<RawFeedItem>> ReadAsync(string feedUrl, CancellationToken ct)
    {
        var feed = await FeedReader.ReadAsync(feedUrl);
        return feed.Items
            .Select(item => new RawFeedItem(
                Title: item.Title ?? string.Empty,
                Url: item.Link ?? string.Empty,
                Description: item.Description ?? string.Empty,
                PublishedAt: item.PublishingDate))
            .Where(item => !string.IsNullOrEmpty(item.Url))
            .ToList();
    }
}
```

- [ ] **Implement `DirectEmailNotifier`**

```csharp
// AppTrack.Infrastructure/Notifications/DirectEmailNotifier.cs
using AppTrack.Application.Contracts.Email;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;
using AppTrack.Application.Models.Email;
using Microsoft.Extensions.Logging;

namespace AppTrack.Infrastructure.Notifications;

public class DirectEmailNotifier : IRssMatchNotifier
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<DirectEmailNotifier> _logger;

    public DirectEmailNotifier(IEmailSender emailSender, ILogger<DirectEmailNotifier> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task NotifyAsync(string userId, List<RssJobApplicationData> matches, CancellationToken ct)
    {
        var body = string.Join("\n", matches.Select(m => $"- {m.Position} ({m.PortalName}): {m.Url}"));
        var email = new EmailMessage
        {
            To = userId,
            Subject = $"{matches.Count} new job(s) discovered",
            Body = $"The following jobs matched your keywords:\n\n{body}"
        };

        var sent = await _emailSender.SendEmail(email);
        if (!sent)
            _logger.LogWarning("Failed to send RSS match notification email for user {UserId}", userId);
    }
}
```

- [ ] **Implement `ServiceBusNotifier`**

```csharp
// AppTrack.Infrastructure/Notifications/ServiceBusNotifier.cs
using System.Text.Json;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AppTrack.Infrastructure.Notifications;

public class ServiceBusNotifier : IRssMatchNotifier
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceBusNotifier> _logger;

    public ServiceBusNotifier(IConfiguration configuration, ILogger<ServiceBusNotifier> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task NotifyAsync(string userId, List<RssJobApplicationData> matches, CancellationToken ct)
    {
        var connectionString = _configuration["ServiceBus:ConnectionString"];
        var queueName = _configuration["RssNotification:QueueName"] ?? "rss-matches";

        await using var client = new ServiceBusClient(connectionString);
        var sender = client.CreateSender(queueName);

        var payload = JsonSerializer.Serialize(new { UserId = userId, Matches = matches });
        await sender.SendMessageAsync(new ServiceBusMessage(payload), ct);

        _logger.LogInformation("Published {Count} RSS matches for user {UserId} to Service Bus",
            matches.Count, userId);
    }
}
```

- [ ] **Build and verify 0 errors**

```bash
dotnet build AppTrack.sln --configuration Release
```

- [ ] **Commit**

```bash
git add AppTrack.Infrastructure/RssFeed/RssFeedReader.cs AppTrack.Infrastructure/Notifications/
git commit -m "feat: add RssFeedReader, DirectEmailNotifier and ServiceBusNotifier"
```

---

### Task 11: DI registration, appsettings, API controller, and BackgroundService

**Files:**
- Modify: `AppTrack.Infrastructure/InfrastructureServicesRegistration.cs`
- Modify: `AppTrack.Api/appsettings.json`
- Modify: `AppTrack.Api/appsettings.Development.json`
- Create: `AppTrack.Api/Controllers/RssFeedController.cs`
- Create: `AppTrack.Api/BackgroundServices/RssFeedBackgroundService.cs`
- Modify: `AppTrack.Api/Program.cs`

- [ ] **Register Infrastructure services**

In `InfrastructureServicesRegistration.cs`, add these using directives:

```csharp
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Infrastructure.Notifications;
using AppTrack.Infrastructure.RssFeed;
```

Add these registrations inside `AddInfrastructureServices`:

```csharp
// RSS feed services
services.AddScoped<IRssFeedReader, RssFeedReader>();
services.AddScoped<IRssFeedItemParser, RssFeedItemParser>();

var rssNotificationProvider = configuration["RssNotification:Provider"];
if (rssNotificationProvider == "ServiceBus")
    services.AddScoped<IRssMatchNotifier, ServiceBusNotifier>();
else
    services.AddScoped<IRssMatchNotifier, DirectEmailNotifier>();
```

- [ ] **Update appsettings**

Add to `AppTrack.Api/appsettings.json` (before `"AllowedHosts"`):

```json
"RssNotification": {
  "Provider": "Direct"
},
```

Add to `AppTrack.Api/appsettings.Development.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  },
  "RssNotification": {
    "Provider": "Direct"
  }
}
```

- [ ] **Create `RssFeedController`**

```csharp
// AppTrack.Api/Controllers/RssFeedController.cs
using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.RssFeeds.Commands.SetRssSubscriptions;
using AppTrack.Application.Features.RssFeeds.Commands.UpdateRssMonitoringSettings;
using AppTrack.Application.Features.RssFeeds.Dto;
using AppTrack.Application.Features.RssFeeds.Queries.GetRssMonitoringSettings;
using AppTrack.Application.Features.RssFeeds.Queries.GetRssPortals;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/rssfeeds")]
[ApiController]
public class RssFeedController : ControllerBase
{
    private readonly IMediator _mediator;

    public RssFeedController(IMediator mediator) => _mediator = mediator;

    [HttpGet("portals")]
    [ProducesResponseType(typeof(List<RssPortalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<RssPortalDto>>> GetPortals()
        => Ok(await _mediator.Send(new GetRssPortalsQuery()));

    [HttpPut("subscriptions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> SetSubscriptions([FromBody] SetRssSubscriptionsCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("settings")]
    [ProducesResponseType(typeof(RssMonitoringSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RssMonitoringSettingsDto>> GetSettings()
        => Ok(await _mediator.Send(new GetRssMonitoringSettingsQuery()));

    [HttpPut("settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateSettings([FromBody] UpdateRssMonitoringSettingsCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}
```

- [ ] **Create `RssFeedBackgroundService`**

```csharp
// AppTrack.Api/BackgroundServices/RssFeedBackgroundService.cs
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.RssFeeds.Commands.PollRssFeeds;

namespace AppTrack.Api.BackgroundServices;

public class RssFeedBackgroundService(IServiceScopeFactory scopeFactory, ILogger<RssFeedBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new PollRssFeedsCommand(), ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error during RSS feed polling");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), ct);
        }
    }
}
```

- [ ] **Register BackgroundService in `Program.cs`**

Add after `builder.Services.AddInfrastructureServices(builder.Configuration)`:

```csharp
builder.Services.AddHostedService<AppTrack.Api.BackgroundServices.RssFeedBackgroundService>();
```

- [ ] **Build and verify 0 errors, 0 warnings**

```bash
dotnet build AppTrack.sln --configuration Release
```

- [ ] **Run unit tests to confirm nothing broken**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

- [ ] **Commit**

```bash
git add AppTrack.Infrastructure/InfrastructureServicesRegistration.cs AppTrack.Api/Controllers/RssFeedController.cs AppTrack.Api/BackgroundServices/ AppTrack.Api/Program.cs AppTrack.Api/appsettings.json AppTrack.Api/appsettings.Development.json
git commit -m "feat: add RssFeedController, RssFeedBackgroundService, DI registration and appsettings"
```

---

## Chunk 5: API Integration Tests

### Task 12: Test infrastructure — stubs, factory, seed helpers

**Files:**
- Create: `AppTrack.Api.IntegrationTests/WebApplicationFactory/StubRssFeedReader.cs`
- Create: `AppTrack.Api.IntegrationTests/WebApplicationFactory/StubRssMatchNotifier.cs`
- Create: `AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeRssFeedWebApplicationFactory.cs`
- Create: `AppTrack.Api.IntegrationTests/SeedData/RssFeed/RssMonitoringSettingsSeedsHelper.cs`
- Create: `AppTrack.Api.IntegrationTests/SeedData/RssFeed/RssSubscriptionSeedsHelper.cs`
- Modify: `AppTrack.Api.IntegrationTests/SeedData/SeedHelper.cs`

- [ ] **Create stubs**

```csharp
// AppTrack.Api.IntegrationTests/WebApplicationFactory/StubRssFeedReader.cs
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

public class StubRssFeedReader : IRssFeedReader
{
    public static readonly List<RawFeedItem> FakeItems =
    [
        new("Senior .NET Developer - Berlin", "https://example.com/job/1", ".NET Core, Azure", DateTime.UtcNow),
        new("Marketing Manager", "https://example.com/job/2", "Brand strategy", DateTime.UtcNow)
    ];

    public Task<List<RawFeedItem>> ReadAsync(string feedUrl, CancellationToken ct)
        => Task.FromResult(FakeItems);
}
```

```csharp
// AppTrack.Api.IntegrationTests/WebApplicationFactory/StubRssMatchNotifier.cs
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

public class StubRssMatchNotifier : IRssMatchNotifier
{
    public Task NotifyAsync(string userId, List<RssJobApplicationData> matches, CancellationToken ct)
        => Task.CompletedTask;
}
```

- [ ] **Create `FakeRssFeedWebApplicationFactory`**

```csharp
// AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeRssFeedWebApplicationFactory.cs
using AppTrack.Application.Contracts.RssFeed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

public class FakeRssFeedWebApplicationFactory : FakeAuthWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRssFeedReader>();
            services.AddScoped<IRssFeedReader, StubRssFeedReader>();
            services.RemoveAll<IRssMatchNotifier>();
            services.AddScoped<IRssMatchNotifier, StubRssMatchNotifier>();
        });
    }
}
```

- [ ] **Create seed helpers**

```csharp
// AppTrack.Api.IntegrationTests/SeedData/RssFeed/RssMonitoringSettingsSeedsHelper.cs
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.SeedData.RssFeed;

internal static class RssMonitoringSettingsSeedsHelper
{
    public static async Task<int> CreateForTestUserAsync(
        IServiceProvider services, List<string> keywords, int intervalMinutes = 60)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var settings = new RssMonitoringSettings
        {
            UserId = Auth.TestAuthHandler.TestUserId,
            Keywords = keywords,
            PollIntervalMinutes = intervalMinutes
        };
        db.RssMonitoringSettings.Add(settings);
        await db.SaveChangesAsync();
        return settings.Id;
    }
}
```

```csharp
// AppTrack.Api.IntegrationTests/SeedData/RssFeed/RssSubscriptionSeedsHelper.cs
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.SeedData.RssFeed;

internal static class RssSubscriptionSeedsHelper
{
    public static async Task<int> CreateForTestUserAsync(
        IServiceProvider services, int portalId, bool isActive = true)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var subscription = new UserRssSubscription
        {
            UserId = Auth.TestAuthHandler.TestUserId,
            RssPortalId = portalId,
            IsActive = isActive
        };
        db.UserRssSubscriptions.Add(subscription);
        await db.SaveChangesAsync();
        return subscription.Id;
    }

    public static async Task<int> GetFirstPortalIdAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();
        var portal = db.RssPortals.First(p => p.IsActive);
        return portal.Id;
    }
}
```

- [ ] **Add helpers to `SeedHelper` facade**

Add to `AppTrack.Api.IntegrationTests/SeedData/SeedHelper.cs`:

```csharp
using AppTrack.Api.IntegrationTests.SeedData.RssFeed;

// Inside the SeedHelper class:
internal static Task<int> CreateRssMonitoringSettingsForTestUserAsync(
    IServiceProvider services, List<string> keywords, int intervalMinutes = 60) =>
    RssMonitoringSettingsSeedsHelper.CreateForTestUserAsync(services, keywords, intervalMinutes);

internal static Task<int> CreateRssSubscriptionForTestUserAsync(
    IServiceProvider services, int portalId, bool isActive = true) =>
    RssSubscriptionSeedsHelper.CreateForTestUserAsync(services, portalId, isActive);

internal static Task<int> GetFirstRssPortalIdAsync(IServiceProvider services) =>
    RssSubscriptionSeedsHelper.GetFirstPortalIdAsync(services);
```

- [ ] **Build and verify 0 errors**

```bash
dotnet build AppTrack.Api.IntegrationTests/AppTrack.Api.IntegrationTests.csproj --configuration Release
```

- [ ] **Commit**

```bash
git add AppTrack.Api.IntegrationTests/WebApplicationFactory/Stub* AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeRssFeedWebApplicationFactory.cs AppTrack.Api.IntegrationTests/SeedData/RssFeed/ AppTrack.Api.IntegrationTests/SeedData/SeedHelper.cs
git commit -m "test: add RSS feed integration test infrastructure (stubs, factory, seed helpers)"
```

---

### Task 13: API integration tests

**Files:**
- Create: `AppTrack.Api.IntegrationTests/RssFeedControllerTests/RssFeedPortalsTests.cs`
- Create: `AppTrack.Api.IntegrationTests/RssFeedControllerTests/RssFeedSubscriptionsTests.cs`
- Create: `AppTrack.Api.IntegrationTests/RssFeedControllerTests/RssFeedSettingsTests.cs`

- [ ] **Write `RssFeedPortalsTests`**

```csharp
// AppTrack.Api.IntegrationTests/RssFeedControllerTests/RssFeedPortalsTests.cs
using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.RssFeeds.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.RssFeedControllerTests;

public class RssFeedPortalsTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RssFeedPortalsTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetPortals_ShouldReturn200_WithAllSystemPortals()
    {
        var response = await _client.GetAsync("api/rssfeeds/portals");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var portals = await response.Content.ReadFromJsonAsync<List<RssPortalDto>>();
        portals.ShouldNotBeNull();
        portals.ShouldNotBeEmpty();
        portals.ShouldContain(p => p.Name == "Stepstone");
    }

    [Fact]
    public async Task GetPortals_ShouldReturn200_WithIsSubscribedFalse_WhenNoSubscription()
    {
        var response = await _client.GetAsync("api/rssfeeds/portals");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var portals = await response.Content.ReadFromJsonAsync<List<RssPortalDto>>();
        portals.ShouldNotBeNull();
        portals.ShouldAllBe(p => !p.IsSubscribed);
    }

    [Fact]
    public async Task GetPortals_ShouldReturn200_WithIsSubscribedTrue_WhenSubscriptionExists()
    {
        var portalId = await SeedHelper.GetFirstRssPortalIdAsync(_factory.Services);
        await SeedHelper.CreateRssSubscriptionForTestUserAsync(_factory.Services, portalId, isActive: true);

        var response = await _client.GetAsync("api/rssfeeds/portals");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var portals = await response.Content.ReadFromJsonAsync<List<RssPortalDto>>();
        portals.ShouldNotBeNull();
        portals.ShouldContain(p => p.Id == portalId && p.IsSubscribed);
    }

    [Fact]
    public async Task GetPortals_ShouldReturn401_WhenUnauthenticated()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("api/rssfeeds/portals");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
```

- [ ] **Write `RssFeedSubscriptionsTests`**

```csharp
// AppTrack.Api.IntegrationTests/RssFeedControllerTests/RssFeedSubscriptionsTests.cs
using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.RssFeeds.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.RssFeedControllerTests;

public class RssFeedSubscriptionsTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RssFeedSubscriptionsTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task SetSubscriptions_ShouldReturn204_WhenPortalIdIsValid()
    {
        var portalId = await SeedHelper.GetFirstRssPortalIdAsync(_factory.Services);
        var command = new { Subscriptions = new[] { new { PortalId = portalId, IsActive = true } } };

        var response = await _client.PutAsJsonAsync("api/rssfeeds/subscriptions", command);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetSubscriptions_ShouldReturn400_WhenPortalIdIsZero()
    {
        var command = new { Subscriptions = new[] { new { PortalId = 0, IsActive = true } } };

        var response = await _client.PutAsJsonAsync("api/rssfeeds/subscriptions", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetSubscriptions_ShouldPersistActivation_WhenCalledWithIsActiveTrue()
    {
        var portalId = await SeedHelper.GetFirstRssPortalIdAsync(_factory.Services);
        var command = new { Subscriptions = new[] { new { PortalId = portalId, IsActive = true } } };

        await _client.PutAsJsonAsync("api/rssfeeds/subscriptions", command);

        var response = await _client.GetAsync("api/rssfeeds/portals");
        var portals = await response.Content.ReadFromJsonAsync<List<RssPortalDto>>();
        portals.ShouldNotBeNull();
        portals.ShouldContain(p => p.Id == portalId && p.IsSubscribed);
    }

    [Fact]
    public async Task SetSubscriptions_ShouldReturn401_WhenUnauthenticated()
    {
        var unauthClient = _factory.CreateClient();
        var command = new { Subscriptions = new[] { new { PortalId = 1, IsActive = true } } };

        var response = await unauthClient.PutAsJsonAsync("api/rssfeeds/subscriptions", command);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
```

- [ ] **Write `RssFeedSettingsTests`**

```csharp
// AppTrack.Api.IntegrationTests/RssFeedControllerTests/RssFeedSettingsTests.cs
using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.RssFeeds.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.RssFeedControllerTests;

public class RssFeedSettingsTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RssFeedSettingsTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetSettings_ShouldReturn200_WithDefaults_WhenNoSettingsExist()
    {
        var response = await _client.GetAsync("api/rssfeeds/settings");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var settings = await response.Content.ReadFromJsonAsync<RssMonitoringSettingsDto>();
        settings.ShouldNotBeNull();
        settings.Keywords.ShouldBeEmpty();
        settings.PollIntervalMinutes.ShouldBe(60);
    }

    [Fact]
    public async Task GetSettings_ShouldReturn200_WithPersistedSettings_WhenSettingsExist()
    {
        await SeedHelper.CreateRssMonitoringSettingsForTestUserAsync(
            _factory.Services, ["dotnet", "azure"], 30);

        var response = await _client.GetAsync("api/rssfeeds/settings");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var settings = await response.Content.ReadFromJsonAsync<RssMonitoringSettingsDto>();
        settings.ShouldNotBeNull();
        settings.Keywords.ShouldBe(["dotnet", "azure"]);
        settings.PollIntervalMinutes.ShouldBe(30);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn204_WhenRequestIsValid()
    {
        var command = new { Keywords = new[] { "dotnet" }, PollIntervalMinutes = 60 };

        var response = await _client.PutAsJsonAsync("api/rssfeeds/settings", command);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn400_WhenKeywordsIsNull()
    {
        var command = new { Keywords = (string[]?)null, PollIntervalMinutes = 60 };

        var response = await _client.PutAsJsonAsync("api/rssfeeds/settings", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn400_WhenIntervalIsBelowMinimum()
    {
        var command = new { Keywords = new[] { "dotnet" }, PollIntervalMinutes = 0 };

        var response = await _client.PutAsJsonAsync("api/rssfeeds/settings", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSettings_ShouldPersistSettings_WhenCalledWithValidData()
    {
        var command = new { Keywords = new[] { "blazor", "csharp" }, PollIntervalMinutes = 120 };

        await _client.PutAsJsonAsync("api/rssfeeds/settings", command);
        var response = await _client.GetAsync("api/rssfeeds/settings");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var settings = await response.Content.ReadFromJsonAsync<RssMonitoringSettingsDto>();
        settings.ShouldNotBeNull();
        settings.Keywords.ShouldBe(["blazor", "csharp"]);
        settings.PollIntervalMinutes.ShouldBe(120);
    }

    [Fact]
    public async Task GetSettings_ShouldReturn401_WhenUnauthenticated()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("api/rssfeeds/settings");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn401_WhenUnauthenticated()
    {
        var unauthClient = _factory.CreateClient();
        var command = new { Keywords = new[] { "dotnet" }, PollIntervalMinutes = 60 };

        var response = await unauthClient.PutAsJsonAsync("api/rssfeeds/settings", command);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
```

- [ ] **Run API integration tests (requires Docker)**

```bash
dotnet test AppTrack.Api.IntegrationTests/AppTrack.Api.IntegrationTests.csproj --configuration Release
```

Expected: All new RSS feed tests pass. Docker must be running for Testcontainers.

- [ ] **Run full test suite**

```bash
dotnet test AppTrack.sln --configuration Release
```

Expected: All tests pass (unit + integration).

- [ ] **Final commit**

```bash
git add AppTrack.Api.IntegrationTests/RssFeedControllerTests/
git commit -m "test: add API integration tests for RssFeedController (portals, subscriptions, settings)"
```
