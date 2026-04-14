# Default Prompt Templates Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a global `BuiltInPrompt` table seeded with 4 German dummy prompts that every user sees (read-only) alongside their own prompts in AI Settings and the Generate Text dropdown.

**Architecture:** New `BuiltInPrompt` domain entity with its own EF Core table; handlers for `GetAiSettings`, `GetPromptNames`, and `GeneratePrompt` are extended to include defaults alongside user prompts. Frontend shows defaults in a separate read-only section. Language column is present for future i18n but no filtering is applied yet.

**Tech Stack:** .NET 10, EF Core 10, xUnit, Moq, Shouldly, MudBlazor, NSwag

---

## File Map

| Action | File |
|--------|------|
| Create | `AppTrack.Domain/BuiltInPrompt.cs` |
| Create | `AppTrack.Application/Contracts/Persistance/IBuiltInPromptRepository.cs` |
| Create | `AppTrack.Persistance/Configurations/BuiltInPromptConfiguration.cs` |
| Modify | `AppTrack.Persistance/DatabaseContext/AppTrackDatabaseContext.cs` |
| Create | `AppTrack.Persistance/Repositories/BuiltInPromptRepository.cs` |
| Modify | `AppTrack.Persistance/PersistanceServiceRegistration.cs` |
| Modify | `AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs` |
| Modify | `AppTrack.Application/Mappings/AiSettingsMappings.cs` |
| Modify | `AppTrack.Application/Features/AiSettings/Queries/GetAiSettingsByUserId/GetAiSettingsByUserIdQueryHandler.cs` |
| Modify | `AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/GetPromptNamesQueryHandler.cs` |
| Modify | `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryValidator.cs` |
| Modify | `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryHandler.cs` |
| Create | `AppTrack.Application.UnitTests/Domain/BuiltInPromptFactoryTests.cs` |
| Modify | `AppTrack.Application.UnitTests/Features/AiSettings/Queries/GetAiSettingsByUserIdQueryHandlerTests.cs` |
| Modify | `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GetPromptNamesQueryHandlerTests.cs` |
| Modify | `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryValidatorTests.cs` |
| Modify | `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryHandlerTests.cs` |
| Modify | `AppTrack.BlazorUi/ApiService/Base/ServiceClient.cs` *(NSwag auto-generated — do not edit manually)* |
| Modify | `Models/AiSettingsModel.cs` |
| Modify | `ApiService/Mappings/AiSettingsMappings.cs` |
| Modify | `AppTrack.BlazorUi/Components/Pages/AiSettings.razor` |

---

## Chunk 1: Domain + Persistence

### Task 1: `BuiltInPrompt` domain entity + factory tests

**Files:**
- Create: `AppTrack.Domain/BuiltInPrompt.cs`
- Create: `AppTrack.Application.UnitTests/Domain/BuiltInPromptFactoryTests.cs`

- [ ] **Step 1: Write the failing factory tests**

```csharp
// AppTrack.Application.UnitTests/Domain/BuiltInPromptFactoryTests.cs
using AppTrack.Domain;
using Shouldly;

namespace AppTrack.Application.UnitTests.Domain;

public class BuiltInPromptFactoryTests
{
    [Fact]
    public void Create_ShouldReturnBuiltInPrompt_WhenAllArgumentsAreValid()
    {
        var result = BuiltInPrompt.Create("Anschreiben", "Template text", "de");

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Anschreiben");
        result.PromptTemplate.ShouldBe("Template text");
        result.Language.ShouldBe("de");
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        Should.Throw<ArgumentNullException>(() => BuiltInPrompt.Create(null, "template", "de"));
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenPromptTemplateIsNull()
    {
        Should.Throw<ArgumentNullException>(() => BuiltInPrompt.Create("Name", null, "de"));
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenLanguageIsNull()
    {
        Should.Throw<ArgumentNullException>(() => BuiltInPrompt.Create("Name", "template", null));
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~BuiltInPromptFactoryTests" --configuration Release
```
Expected: compile error — `BuiltInPrompt` does not exist.

- [ ] **Step 3: Create `BuiltInPrompt.cs`**

Pattern: same as `AppTrack.Domain/Prompt.cs` — public setters (required for EF Core), private constructor, static `Create` factory with null checks.

```csharp
// AppTrack.Domain/BuiltInPrompt.cs
using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class BuiltInPrompt : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty; // ISO 639-1, e.g. "de", "en"

    private DefaultPrompt() { }

    public static BuiltInPrompt Create(string? name, string? promptTemplate, string? language)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(promptTemplate);
        ArgumentNullException.ThrowIfNull(language);

        return new BuiltInPrompt
        {
            Name = name,
            PromptTemplate = promptTemplate,
            Language = language
        };
    }
}
```

- [ ] **Step 4: Run tests — expect pass**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~BuiltInPromptFactoryTests" --configuration Release
```
Expected: 4 tests pass.

- [ ] **Step 5: Commit**

```bash
git add AppTrack.Domain/BuiltInPrompt.cs test/AppTrack.Application.UnitTests/Domain/BuiltInPromptFactoryTests.cs
git commit -m "feat: add BuiltInPrompt domain entity with Create factory"
```

---

### Task 2: `IBuiltInPromptRepository` interface

**Files:**
- Create: `AppTrack.Application/Contracts/Persistance/IBuiltInPromptRepository.cs`

- [ ] **Step 1: Create the interface**

Pattern: same as `AppTrack.Application/Contracts/Persistance/IAiSettingsRepository.cs`.

```csharp
// AppTrack.Application/Contracts/Persistance/IBuiltInPromptRepository.cs
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IBuiltInPromptRepository : IGenericRepository<BuiltInPrompt>
{
    Task<IReadOnlyList<BuiltInPrompt>> GetByLanguageAsync(string language);
}
```

- [ ] **Step 2: Build to verify no errors**

```bash
dotnet build AppTrack.sln --configuration Release
```
Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Application/Contracts/Persistance/IBuiltInPromptRepository.cs
git commit -m "feat: add IBuiltInPromptRepository interface"
```

---

### Task 3: Persistence — configuration, repository, DbContext, DI, EF migration

**Files:**
- Create: `AppTrack.Persistance/Configurations/BuiltInPromptConfiguration.cs`
- Modify: `AppTrack.Persistance/DatabaseContext/AppTrackDatabaseContext.cs`
- Create: `AppTrack.Persistance/Repositories/BuiltInPromptRepository.cs`
- Modify: `AppTrack.Persistance/PersistanceServiceRegistration.cs`

> **Note on `HasData` seed IDs:** EF Core `HasData` requires explicit primary key values. Use the `Create` factory and then set `Id` via the public `BaseEntity.Id` property. `CreationDate`/`ModifiedDate` will be `null` for seed rows — this is fine since `BaseEntity` declares them as `DateTime?`.

- [ ] **Step 1: Create `BuiltInPromptConfiguration.cs`**

Reference: `AppTrack.Persistance/Configurations/PromptConfiguration.cs` for property/index patterns, `AppTrack.Persistance/Configurations/ChatModelsConfiguration.cs` for `HasData` pattern.

```csharp
// AppTrack.Persistance/Configurations/BuiltInPromptConfiguration.cs
using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class BuiltInPromptConfiguration : IEntityTypeConfiguration<BuiltInPrompt>
{
    public void Configure(EntityTypeBuilder<BuiltInPrompt> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PromptTemplate)
            .IsRequired();

        builder.Property(x => x.Language)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(x => new { x.Name, x.Language })
            .IsUnique();

        builder.HasData(
            Seed(1, "Anschreiben",
                "Schreibe ein professionelles Anschreiben für die Stelle {Position} bei {Company}. Stellenbeschreibung: {JobDescription}"),
            Seed(2, "LinkedIn Nachricht",
                "Schreibe eine kurze LinkedIn-Nachricht an {ContactPerson} bezüglich der Stelle {Position} bei {Company}."),
            Seed(3, "Vorstellung",
                "Stelle mich in ein paar Sätzen als Bewerber für die Stelle {Position} bei {Company} vor."),
            Seed(4, "Nachfassen",
                "Schreibe eine kurze Follow-up-E-Mail an {ContactPerson} bezüglich meiner Bewerbung für die Stelle {Position} bei {Company}.")
        );
    }

    private static BuiltInPrompt Seed(int id, string name, string promptTemplate)
    {
        var p = BuiltInPrompt.Create(name, promptTemplate, "de");
        p.Id = id;
        return p;
    }
}
```

- [ ] **Step 2: Add `DbSet<BuiltInPrompt>` to `AppTrackDatabaseContext`**

File: `AppTrack.Persistance/DatabaseContext/AppTrackDatabaseContext.cs`

Add after the existing `DbSet` declarations (after `public DbSet<ChatModel> ChatModels { get; set; }`):

```csharp
public DbSet<BuiltInPrompt> BuiltInPrompts { get; set; }
```

- [ ] **Step 3: Create `BuiltInPromptRepository.cs`**

Pattern: same as `AppTrack.Persistance/Repositories/AiSettingsRepository.cs`.

```csharp
// AppTrack.Persistance/Repositories/BuiltInPromptRepository.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class BuiltInPromptRepository : GenericRepository<BuiltInPrompt>, IBuiltInPromptRepository
{
    public BuiltInPromptRepository(AppTrackDatabaseContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<BuiltInPrompt>> GetByLanguageAsync(string language)
    {
        return await _context.BuiltInPrompts
            .AsNoTracking()
            .Where(p => p.Language == language)
            .ToListAsync();
    }
}
```

- [ ] **Step 4: Register in DI**

File: `AppTrack.Persistance/PersistanceServiceRegistration.cs`

Add after `services.AddScoped<IChatModelRepository, ChatModelRepository>();`:

```csharp
services.AddScoped<IBuiltInPromptRepository, BuiltInPromptRepository>();
```

- [ ] **Step 5: Build to verify no errors**

```bash
dotnet build AppTrack.sln --configuration Release
```
Expected: 0 errors, 0 warnings.

- [ ] **Step 6: Create EF Core migration**

Run from the solution root:

```bash
dotnet ef migrations add AddDefaultPromptsTable --project AppTrack.Persistance --startup-project AppTrack.Api
```

Verify that a new migration file was created in `AppTrack.Persistance/Migrations/` containing:
- `CreateTable` for `BuiltInPrompts`
- `InsertData` for all 4 seed rows
- `CreateIndex` for the unique `(Name, Language)` index

- [ ] **Step 7: Run the app to apply migration**

Start `AppTrack.Api` (`dotnet run --project AppTrack.Api`). The migration runs automatically on startup via `MigrationsHelper.TryApplyDatabaseMigrations`. Check the logs for "Database migration applied successfully" (or similar). Stop the app after confirming.

- [ ] **Step 8: Run all unit tests — expect all pass**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```
Expected: all existing tests + 4 new factory tests pass.

- [ ] **Step 9: Commit**

```bash
git add AppTrack.Persistance/ AppTrack.Application/Contracts/Persistance/IBuiltInPromptRepository.cs
git commit -m "feat: add BuiltInPrompt persistence — configuration, repository, DI, migration"
```

---

## Chunk 2: Application Layer

### Task 4: `AiSettingsDto` + `GetAiSettingsByUserIdQueryHandler`

**Files:**
- Modify: `AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs`
- Modify: `AppTrack.Application/Mappings/AiSettingsMappings.cs`
- Modify: `AppTrack.Application/Features/AiSettings/Queries/GetAiSettingsByUserId/GetAiSettingsByUserIdQueryHandler.cs`
- Modify: `AppTrack.Application.UnitTests/Features/AiSettings/Queries/GetAiSettingsByUserIdQueryHandlerTests.cs`

- [ ] **Step 1: Write the failing test**

The existing `GetAiSettingsByUserIdQueryHandlerTests.cs` constructor creates the handler with one argument. After our change, it takes two. Add the new mock and update `CreateHandler`. Also add a new test case.

Replace the entire file content:

```csharp
// AppTrack.Application.UnitTests/Features/AiSettings/Queries/GetAiSettingsByUserIdQueryHandlerTests.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;
using AppTrack.Domain;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Queries;

public class GetAiSettingsByUserIdQueryHandlerTests
{
    private readonly Mock<IAiSettingsRepository> _mockRepo = new();
    private readonly Mock<IBuiltInPromptRepository> _mockBuiltInPromptRepo = new();

    public GetAiSettingsByUserIdQueryHandlerTests()
    {
        // Default: return empty list so existing tests are unaffected
        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>());
    }

    private GetAiSettingsByUserIdQueryHandler CreateHandler() =>
        new(_mockRepo.Object, _mockBuiltInPromptRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnExistingAiSettings_WhenAiSettingsExistForUser()
    {
        const string userId = "user-1";
        var existingSettings = new DomainAiSettings { Id = 1, UserId = userId, SelectedChatModelId = 3 };

        _mockRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userId))
            .ReturnsAsync(existingSettings);

        var result = await CreateHandler().Handle(new GetAiSettingsByUserIdQuery { UserId = userId }, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<AiSettingsDto>();
        result.UserId.ShouldBe(userId);
        result.Id.ShouldBe(1);
        result.SelectedChatModelId.ShouldBe(3);
        _mockRepo.Verify(r => r.CreateAsync(It.IsAny<DomainAiSettings>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateAndReturnNewAiSettings_WhenNoAiSettingsExistForUser()
    {
        const string userId = "new-user";

        _mockRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userId))
            .ReturnsAsync((DomainAiSettings?)null);

        _mockRepo
            .Setup(r => r.CreateAsync(It.IsAny<DomainAiSettings>()))
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(new GetAiSettingsByUserIdQuery { UserId = userId }, CancellationToken.None);

        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        _mockRepo.Verify(r => r.CreateAsync(It.Is<DomainAiSettings>(s => s.UserId == userId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPopulateBuiltInPrompts_InReturnedDto()
    {
        const string userId = "user-1";
        _mockRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = userId });

        var defaults = new List<BuiltInPrompt>
        {
            BuiltInPrompt.Create("Anschreiben", "Template A", "de"),
            BuiltInPrompt.Create("Vorstellung", "Template B", "de"),
        };
        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(defaults);

        var result = await CreateHandler().Handle(new GetAiSettingsByUserIdQuery { UserId = userId }, CancellationToken.None);

        result.BuiltInPrompts.ShouldNotBeNull();
        result.BuiltInPrompts.Count.ShouldBe(2);
        result.BuiltInPrompts[0].Name.ShouldBe("Anschreiben");
        result.BuiltInPrompts[1].Name.ShouldBe("Vorstellung");
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~GetAiSettingsByUserIdQueryHandlerTests" --configuration Release
```
Expected: compile errors — `IBuiltInPromptRepository` not recognized by handler constructor, `AiSettingsDto.BuiltInPrompts` doesn't exist.

- [ ] **Step 3: Add `BuiltInPrompts` to `AiSettingsDto`**

File: `AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs`

Add after `public List<PromptDto> Prompts { get; set; } = new List<PromptDto>();`:

```csharp
public List<PromptDto> BuiltInPrompts { get; set; } = new List<PromptDto>();
```

- [ ] **Step 4: Add `ToDto` extension for `BuiltInPrompt` in `AiSettingsMappings.cs`**

File: `AppTrack.Application/Mappings/AiSettingsMappings.cs`

Add a new extension method after `internal static PromptDto ToDto(this Prompt entity)`:

```csharp
internal static PromptDto ToDto(this BuiltInPrompt entity) => new()
{
    Id = entity.Id,
    Name = entity.Name,
    PromptTemplate = entity.PromptTemplate,
};
```

Also add `using AppTrack.Domain;` at the top if not already present (it already is — `BuiltInPrompt` is in `AppTrack.Domain`).

- [ ] **Step 5: Update `GetAiSettingsByUserIdQueryHandler`**

Full replacement of `AppTrack.Application/Features/AiSettings/Queries/GetAiSettingsByUserId/GetAiSettingsByUserIdQueryHandler.cs`:

```csharp
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Mappings;

namespace AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;

public class GetAiSettingsByUserIdQueryHandler : IRequestHandler<GetAiSettingsByUserIdQuery, AiSettingsDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IBuiltInPromptRepository _builtInPromptRepository;

    public GetAiSettingsByUserIdQueryHandler(IAiSettingsRepository aiSettingsRepository, IBuiltInPromptRepository builtInPromptRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;
        _builtInPromptRepository = builtInPromptRepository;
    }

    /// <summary>
    /// Gets the AI settings for the specified user. Creates and returns a new instance if the entity doesn't exist.
    /// </summary>
    public async Task<AiSettingsDto> Handle(GetAiSettingsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetAiSettingsByUserIdQueryValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException($"Invalid request", validationResult);

        var entity = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId);

        if (entity == null)
        {
            entity = request.ToDomain();
            await _aiSettingsRepository.CreateAsync(entity);
        }

        var dto = entity.ToDto();
        var defaults = await _builtInPromptRepository.GetAsync();
        dto.BuiltInPrompts = defaults.Select(d => d.ToDto()).ToList();
        return dto;
    }
}
```

- [ ] **Step 6: Run tests — expect pass**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~GetAiSettingsByUserIdQueryHandlerTests" --configuration Release
```
Expected: 3 tests pass.

- [ ] **Step 7: Run all unit tests**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```
Expected: all tests pass (build may fail on other handlers that reference `GeneratePromptQueryValidator` — those will be fixed in Task 6).

- [ ] **Step 8: Commit**

```bash
git add AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs AppTrack.Application/Mappings/AiSettingsMappings.cs AppTrack.Application/Features/AiSettings/Queries/ test/AppTrack.Application.UnitTests/Features/AiSettings/
git commit -m "feat: extend AiSettingsDto with BuiltInPrompts, populate in GetAiSettingsByUserIdQueryHandler"
```

---

### Task 5: `GetPromptNamesQueryHandler`

**Files:**
- Modify: `AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/GetPromptNamesQueryHandler.cs`
- Modify: `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GetPromptNamesQueryHandlerTests.cs`

- [ ] **Step 1: Write the failing tests**

Replace the entire test file:

```csharp
// AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GetPromptNamesQueryHandlerTests.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;
using AppTrack.Domain;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.ApplicationText.Queries;

public class GetPromptNamesQueryHandlerTests
{
    private const string UserId = "user-1";
    private readonly Mock<IAiSettingsRepository> _mockAiSettingsRepo = new();
    private readonly Mock<IBuiltInPromptRepository> _mockBuiltInPromptRepo = new();

    public GetPromptNamesQueryHandlerTests()
    {
        // Default: no default prompts (keeps existing tests unaffected)
        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>());
    }

    private GetPromptNamesQueryHandler CreateHandler() =>
        new(_mockAiSettingsRepo.Object, _mockBuiltInPromptRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnNamesInInsertionOrder_WhenAiSettingsHaveMultiplePrompts()
    {
        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(Prompt.Create("Cover Letter", "template A"));
        aiSettings.Prompts.Add(Prompt.Create("LinkedIn Message", "template B"));
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(aiSettings);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBe(["Cover Letter", "LinkedIn Message"]);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAiSettingsHaveNoPromptsAndNoDefaults()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId });

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAiSettingsNotFound()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync((DomainAiSettings?)null);

        await Should.ThrowAsync<BadRequestException>(() =>
            CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnBuiltInPromptNames_WhenUserHasNoPrompts()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId });

        var defaults = new List<BuiltInPrompt>
        {
            BuiltInPrompt.Create("Anschreiben", "template", "de"),
            BuiltInPrompt.Create("Vorstellung", "template", "de"),
        };
        _mockBuiltInPromptRepo.Setup(r => r.GetAsync()).ReturnsAsync(defaults);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBe(["Anschreiben", "Vorstellung"]);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserNamesThenBuiltInNames_WithUserNamesFirst()
    {
        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(Prompt.Create("My Custom Prompt", "template"));
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(aiSettings);

        var defaults = new List<BuiltInPrompt>
        {
            BuiltInPrompt.Create("Anschreiben", "template", "de"),
        };
        _mockBuiltInPromptRepo.Setup(r => r.GetAsync()).ReturnsAsync(defaults);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.First().ShouldBe("My Custom Prompt");
        result.Names.Last().ShouldBe("Anschreiben");
    }

    [Fact]
    public async Task Handle_ShouldDeduplicatePromptNames_CaseInsensitively()
    {
        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(Prompt.Create("anschreiben", "user template"));
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(aiSettings);

        var defaults = new List<BuiltInPrompt>
        {
            BuiltInPrompt.Create("Anschreiben", "default template", "de"),
            BuiltInPrompt.Create("Vorstellung", "template", "de"),
        };
        _mockBuiltInPromptRepo.Setup(r => r.GetAsync()).ReturnsAsync(defaults);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        // "Anschreiben" from defaults is suppressed; "anschreiben" from user + "Vorstellung" remain
        result.Names.Count.ShouldBe(2);
        result.Names.ShouldContain("anschreiben");
        result.Names.ShouldContain("Vorstellung");
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~GetPromptNamesQueryHandlerTests" --configuration Release
```
Expected: compile error — handler constructor doesn't match.

- [ ] **Step 3: Update `GetPromptNamesQueryHandler`**

Full replacement of `AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/GetPromptNamesQueryHandler.cs`:

```csharp
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;

namespace AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;

public class GetPromptNamesQueryHandler : IRequestHandler<GetPromptNamesQuery, GetPromptNamesDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IBuiltInPromptRepository _builtInPromptRepository;

    public GetPromptNamesQueryHandler(IAiSettingsRepository aiSettingsRepository, IBuiltInPromptRepository builtInPromptRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;
        _builtInPromptRepository = builtInPromptRepository;
    }

    public async Task<GetPromptNamesDto> Handle(GetPromptNamesQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetPromptNamesQueryValidator(_aiSettingsRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid request.", validationResult);

        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId);
        var defaults = await _builtInPromptRepository.GetAsync();

        var userNames = aiSettings!.Prompts.Select(p => p.Name).ToList();
        var defaultNames = defaults
            .Select(d => d.Name)
            .Where(dn => !userNames.Any(un => string.Equals(un, dn, StringComparison.OrdinalIgnoreCase)));

        var names = userNames.Concat(defaultNames).ToList();
        return new GetPromptNamesDto { Names = names };
    }
}
```

- [ ] **Step 4: Run tests — expect pass**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~GetPromptNamesQueryHandlerTests" --configuration Release
```
Expected: 6 tests pass.

- [ ] **Step 5: Commit**

```bash
git add AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/ test/AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GetPromptNamesQueryHandlerTests.cs
git commit -m "feat: extend GetPromptNamesQueryHandler — merge user and default prompt names"
```

---

### Task 6: `GeneratePromptQueryValidator` + `GeneratePromptQueryHandler`

**Files:**
- Modify: `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryValidator.cs`
- Modify: `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryHandler.cs`
- Modify: `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryValidatorTests.cs`
- Modify: `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryHandlerTests.cs`

- [ ] **Step 1: Write the failing validator test**

Open `GeneratePromptQueryValidatorTests.cs`. The test constructor currently creates the validator with 2 arguments. After our change it takes 3. Add the new mock and a new test case.

The existing constructor setup and all tests should be preserved. Replace the entire file:

```csharp
// AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryValidatorTests.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;
using AppTrack.Domain;
using FluentValidation.TestHelper;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;
using DomainJobApplication = AppTrack.Domain.JobApplication;
using DomainPrompt = AppTrack.Domain.Prompt;

namespace AppTrack.Application.UnitTests.Features.ApplicationText.Queries;

public class GeneratePromptQueryValidatorTests
{
    private const string UserId = "user-1";
    private const string PromptName = "Default";
    private const int ExistingJobApplicationId = 42;

    private readonly Mock<IJobApplicationRepository> _jobAppRepo;
    private readonly Mock<IAiSettingsRepository> _aiSettingsRepo;
    private readonly Mock<IBuiltInPromptRepository> _builtInPromptRepo;
    private readonly GeneratePromptQueryValidator _validator;

    public GeneratePromptQueryValidatorTests()
    {
        _jobAppRepo = new Mock<IJobApplicationRepository>();
        _aiSettingsRepo = new Mock<IAiSettingsRepository>();
        _builtInPromptRepo = new Mock<IBuiltInPromptRepository>();

        var jobApplication = new DomainJobApplication { Id = ExistingJobApplicationId, UserId = UserId };
        _jobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(jobApplication);
        _jobAppRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingJobApplicationId)))
            .ReturnsAsync((DomainJobApplication?)null);

        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(DomainPrompt.Create(PromptName, "Write a cover letter for {position}"));

        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(aiSettings);
        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(It.Is<string>(id => id != UserId)))
            .ReturnsAsync((DomainAiSettings?)null);

        // Default: no default prompts
        _builtInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>());

        _validator = new GeneratePromptQueryValidator(_jobAppRepo.Object, _aiSettingsRepo.Object, _builtInPromptRepo.Object);
    }

    private static GeneratePromptQuery BuildValidQuery(
        string userId = UserId,
        int jobApplicationId = ExistingJobApplicationId,
        string promptName = PromptName) => new()
    {
        UserId = userId,
        JobApplicationId = jobApplicationId,
        PromptName = promptName
    };

    [Fact]
    public async Task Validate_ShouldPass_WhenQueryIsValid()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationIdIsZero()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery(jobApplicationId: 0));
        result.ShouldHaveValidationErrorFor(x => x.JobApplicationId);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationDoesNotExist()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery(jobApplicationId: 9999));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Job application doesn't exist");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationBelongsToAnotherUser()
    {
        const string otherUserId = "user-other";
        _jobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(new DomainJobApplication { Id = ExistingJobApplicationId, UserId = otherUserId });

        var result = await _validator.TestValidateAsync(BuildValidQuery());
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Job application doesn't belong to this user.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenAiSettingsNotFound()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery(userId: "unknown-user"));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "AI settings not found for this user.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenPromptNameIsEmpty()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery(promptName: ""));
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.PromptName);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenNamedPromptNotFoundInUserOrDefaults()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery(promptName: "NonExistentPrompt"));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Prompt not found in AI settings.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenNamedPromptTemplateIsEmpty()
    {
        const string emptyTemplateUser = "user-empty-template";
        var aiSettingsWithEmptyTemplate = new DomainAiSettings { Id = 3, UserId = emptyTemplateUser };
        aiSettingsWithEmptyTemplate.Prompts.Add(DomainPrompt.Create(PromptName, " "));

        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(emptyTemplateUser))
            .ReturnsAsync(aiSettingsWithEmptyTemplate);

        _jobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(new DomainJobApplication { Id = ExistingJobApplicationId, UserId = emptyTemplateUser });

        var result = await _validator.TestValidateAsync(BuildValidQuery(userId: emptyTemplateUser));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Prompt template is empty.");
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenPromptNameExistsInBuiltInPromptsOnly()
    {
        const string defaultOnlyPromptName = "Anschreiben";
        // User has no prompt with this name
        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId }); // no prompts

        _builtInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>
            {
                BuiltInPrompt.Create(defaultOnlyPromptName, "Schreibe ein Anschreiben für {Position}.", "de"),
            });

        _jobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(new DomainJobApplication { Id = ExistingJobApplicationId, UserId = UserId });

        var localValidator = new GeneratePromptQueryValidator(_jobAppRepo.Object, _aiSettingsRepo.Object, _builtInPromptRepo.Object);
        var result = await localValidator.TestValidateAsync(BuildValidQuery(promptName: defaultOnlyPromptName));
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenBuiltInPromptTemplateIsEmpty()
    {
        const string defaultOnlyPromptName = "EmptyDefault";
        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId }); // no user prompts

        _builtInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>
            {
                BuiltInPrompt.Create(defaultOnlyPromptName, " ", "de"), // empty template
            });

        _jobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(new DomainJobApplication { Id = ExistingJobApplicationId, UserId = UserId });

        var localValidator = new GeneratePromptQueryValidator(
            _jobAppRepo.Object, _aiSettingsRepo.Object, _builtInPromptRepo.Object);
        var result = await localValidator.TestValidateAsync(BuildValidQuery(promptName: defaultOnlyPromptName));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Prompt template is empty.");
    }
}
```

- [ ] **Step 2: Write the failing handler test**

Add one test to `GeneratePromptQueryHandlerTests.cs`. The handler constructor also needs the new `IBuiltInPromptRepository` parameter. Replace the entire file:

```csharp
// AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryHandlerTests.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;
using AppTrack.Domain;
using AppTrack.Domain.Contracts;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.ApplicationText.Queries;

public class GeneratePromptQueryHandlerTests
{
    private const string UserId = "user-1";
    private const int JobApplicationId = 5;
    private const string PromptName = "Default";

    private readonly Mock<IAiSettingsRepository> _mockAiSettingsRepo;
    private readonly Mock<IJobApplicationRepository> _mockJobApplicationRepo;
    private readonly Mock<IPromptBuilder> _mockPromptBuilder;
    private readonly Mock<IBuiltInPromptRepository> _mockBuiltInPromptRepo;

    public GeneratePromptQueryHandlerTests()
    {
        _mockAiSettingsRepo = new Mock<IAiSettingsRepository>();
        _mockJobApplicationRepo = new Mock<IJobApplicationRepository>();
        _mockPromptBuilder = new Mock<IPromptBuilder>();
        _mockBuiltInPromptRepo = new Mock<IBuiltInPromptRepository>();

        var existingJobApplication = new JobApplication
        {
            Id = JobApplicationId,
            UserId = UserId,
            Name = "Test Company",
            Position = "Developer",
            URL = "https://company.com",
            JobDescription = "Job desc",
            Location = "Remote",
            ContactPerson = "Jane",
            Status = JobApplicationStatus.New,
            StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var existingAiSettings = new DomainAiSettings
        {
            Id = 1,
            UserId = UserId,
            Prompts = new List<Prompt> { Prompt.Create(PromptName, "Hello {Name}") },
            PromptParameter = new List<PromptParameter>()
        };

        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(JobApplicationId))
            .ReturnsAsync(existingJobApplication);
        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != JobApplicationId)))
            .ReturnsAsync((JobApplication?)null);

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(existingAiSettings);

        // Default: no default prompts
        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>());

        _mockPromptBuilder
            .Setup(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), It.IsAny<string>()))
            .Returns(("Hello Test Company", new List<string>()));
    }

    private GeneratePromptQueryHandler CreateHandler() =>
        new(_mockAiSettingsRepo.Object, _mockJobApplicationRepo.Object, _mockPromptBuilder.Object, _mockBuiltInPromptRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnGeneratedPromptDto_WhenQueryIsValid()
    {
        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptName = PromptName };

        var result = await CreateHandler().Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<GeneratedPromptDto>();
        result.Prompt.ShouldBe("Hello Test Company");
        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), "Hello {Name}"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldBuildPromptFromNamedTemplate()
    {
        const string secondPromptName = "LinkedIn";
        const string secondTemplate = "LinkedIn template for {Name}";

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings
            {
                Id = 1,
                UserId = UserId,
                Prompts = new List<Prompt>
                {
                    Prompt.Create(PromptName, "Hello {Name}"),
                    Prompt.Create(secondPromptName, secondTemplate)
                },
                PromptParameter = new List<PromptParameter>()
            });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptName = secondPromptName };
        await CreateHandler().Handle(query, CancellationToken.None);

        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), secondTemplate), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationDoesNotExist()
    {
        var query = new GeneratePromptQuery { JobApplicationId = 9999, UserId = UserId, PromptName = PromptName };
        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationBelongsToAnotherUser()
    {
        const string otherUserId = "user-other";
        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(JobApplicationId))
            .ReturnsAsync(new JobApplication { Id = JobApplicationId, UserId = otherUserId });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptName = PromptName };
        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAiSettingsDoNotExistForUser()
    {
        const string noSettingsUser = "user-no-settings";

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(noSettingsUser))
            .ReturnsAsync((DomainAiSettings?)null);

        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(JobApplicationId))
            .ReturnsAsync(new JobApplication { Id = JobApplicationId, UserId = noSettingsUser });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = noSettingsUser, PromptName = PromptName };
        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldBuildPromptFromDefaultTemplate_WhenPromptNameNotInUserSettings()
    {
        const string builtInPromptName = "Anschreiben";
        const string builtInTemplate = "Schreibe ein Anschreiben für {Position}.";

        // User has no prompt with this name
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings
            {
                Id = 1,
                UserId = UserId,
                Prompts = new List<Prompt>(),
                PromptParameter = new List<PromptParameter>()
            });

        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>
            {
                BuiltInPrompt.Create(builtInPromptName, builtInTemplate, "de"),
            });

        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(JobApplicationId))
            .ReturnsAsync(new JobApplication
            {
                Id = JobApplicationId,
                UserId = UserId,
                Name = "Acme",
                Position = "Engineer",
                URL = "https://acme.com",
                JobDescription = "desc",
                Location = "Remote",
                ContactPerson = "Bob",
                Status = JobApplicationStatus.New,
                StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptName = builtInPromptName };
        await CreateHandler().Handle(query, CancellationToken.None);

        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), builtInTemplate), Times.Once);
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~GeneratePromptQuery" --configuration Release
```
Expected: compile errors — validator/handler constructors don't match.

- [ ] **Step 4: Update `GeneratePromptQueryValidator`**

Full replacement of `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryValidator.cs`:

```csharp
using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;

public class GeneratePromptQueryValidator : AbstractValidator<GeneratePromptQuery>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IBuiltInPromptRepository _builtInPromptRepository;

    public GeneratePromptQueryValidator(
        IJobApplicationRepository jobApplicationRepository,
        IAiSettingsRepository aiSettingsRepository,
        IBuiltInPromptRepository builtInPromptRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _aiSettingsRepository = aiSettingsRepository;
        _builtInPromptRepository = builtInPromptRepository;

        RuleFor(x => x.JobApplicationId)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.PromptName)
            .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(x => x)
            .CustomAsync(async (query, context, token) =>
            {
                var jobApplication = await _jobApplicationRepository.GetByIdAsync(query.JobApplicationId);
                if (jobApplication == null)
                {
                    context.AddFailure("Job application doesn't exist");
                    return;
                }

                if (jobApplication.UserId != query.UserId)
                    context.AddFailure("Job application doesn't belong to this user.");
            });

        RuleFor(x => x)
            .CustomAsync(ValidateAiSettings);
    }

    private async Task ValidateAiSettings(GeneratePromptQuery query, ValidationContext<GeneratePromptQuery> context, CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(query.UserId);

        if (aiSettings == null)
        {
            context.AddFailure("AI settings not found for this user.");
            return;
        }

        var userPrompt = aiSettings.Prompts.FirstOrDefault(
            p => string.Equals(p.Name, query.PromptName, StringComparison.OrdinalIgnoreCase));

        if (userPrompt != null)
        {
            if (string.IsNullOrWhiteSpace(userPrompt.PromptTemplate))
                context.AddFailure("Prompt template is empty.");
            return;
        }

        var defaults = await _builtInPromptRepository.GetAsync();
        var builtInPrompt = defaults.FirstOrDefault(
            p => string.Equals(p.Name, query.PromptName, StringComparison.OrdinalIgnoreCase));

        if (builtInPrompt == null)
        {
            context.AddFailure("Prompt not found in AI settings.");
            return;
        }

        if (string.IsNullOrWhiteSpace(builtInPrompt.PromptTemplate))
            context.AddFailure("Prompt template is empty.");
    }
}
```

- [ ] **Step 5: Update `GeneratePromptQueryHandler`**

Full replacement of `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryHandler.cs`:

```csharp
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Domain.Contracts;
using AppTrack.Domain.Extensions;

namespace AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;

public class GeneratePromptQueryHandler : IRequestHandler<GeneratePromptQuery, GeneratedPromptDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IBuiltInPromptRepository _builtInPromptRepository;

    public GeneratePromptQueryHandler(
        IAiSettingsRepository aiSettingsRepository,
        IJobApplicationRepository jobApplicationRepository,
        IPromptBuilder promptBuilder,
        IBuiltInPromptRepository builtInPromptRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _promptBuilder = promptBuilder;
        _builtInPromptRepository = builtInPromptRepository;
    }

    public async Task<GeneratedPromptDto> Handle(GeneratePromptQuery request, CancellationToken cancellationToken)
    {
        var validator = new GeneratePromptQueryValidator(_jobApplicationRepository, _aiSettingsRepository, _builtInPromptRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid generate prompt request.", validationResult);

        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId);
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId);

        var applicantParameter = aiSettings!.PromptParameter.ToList();
        var jobApplicationParameter = jobApplication!.ToPromptParameters().ToList();
        var promptParameter = jobApplicationParameter.Union(applicantParameter).ToList();

        var userPrompt = aiSettings.Prompts.FirstOrDefault(
            p => string.Equals(p.Name, request.PromptName, StringComparison.OrdinalIgnoreCase));

        string promptTemplate;
        if (userPrompt != null)
        {
            promptTemplate = userPrompt.PromptTemplate;
        }
        else
        {
            var defaults = await _builtInPromptRepository.GetAsync();
            promptTemplate = defaults
                .First(p => string.Equals(p.Name, request.PromptName, StringComparison.OrdinalIgnoreCase))
                .PromptTemplate;
        }

        var (prompt, unusedKeys) = _promptBuilder.BuildPrompt(promptParameter, promptTemplate);
        return new GeneratedPromptDto() { Prompt = prompt, UnusedKeys = unusedKeys };
    }
}
```

- [ ] **Step 6: Run tests — expect pass**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```
Expected: all tests pass (including the new handler and validator tests).

- [ ] **Step 7: Commit**

```bash
git add AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/ test/AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryValidatorTests.cs test/AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryHandlerTests.cs
git commit -m "feat: extend GeneratePromptQuery — fall back to default prompts when user prompt not found"
```

---

## Chunk 3: Frontend

### Task 7: NSwag regeneration + `AiSettingsModel` + frontend mappings

**Files:**
- Modify: `AppTrack.BlazorUi/ApiService/Base/ServiceClient.cs` *(auto-generated — regenerate, do not hand-edit)*
- Modify: `Models/AiSettingsModel.cs`
- Modify: `ApiService/Mappings/AiSettingsMappings.cs`

- [ ] **Step 1: Start the API to expose the updated swagger doc**

```bash
dotnet run --project AppTrack.Api
```

Wait for "Now listening on: http://localhost:5250" in the output. Keep it running.

- [ ] **Step 2: Regenerate the NSwag client**

In a separate terminal, run the NSwag tool from the `AppTrack.BlazorUi` directory (the `nswag.json` config file lives there):

```bash
cd AppTrack.BlazorUi
dotnet tool run nswag run nswag.json
```

After completion, stop the API (`Ctrl+C`).

- [ ] **Step 3: Verify the generated `ServiceClient.cs` has `BuiltInPrompts`**

Open `AppTrack.BlazorUi/ApiService/Base/ServiceClient.cs` and confirm the generated `AiSettingsDto` class now has a `BuiltInPrompts` property (type `ICollection<PromptDto>?` or similar). Also confirm `PromptDto` is unchanged (Id, Name, PromptTemplate).

- [ ] **Step 4: Build to verify no compile errors from the regenerated client**

```bash
dotnet build AppTrack.sln --configuration Release
```
Expected: 0 errors, 0 warnings.

- [ ] **Step 5: Add `BuiltInPrompts` to `AiSettingsModel`**

File: `Models/AiSettingsModel.cs`

Add after `public ObservableCollection<PromptModel> Prompts { get; set; } = new ObservableCollection<PromptModel>();`:

```csharp
public List<PromptModel> BuiltInPrompts { get; set; } = [];
```

Do NOT add this to the `IAiSettingsValidatable` interface or to the explicit interface implementations — it is display-only and not validated.

- [ ] **Step 6: Update frontend mapping to populate `BuiltInPrompts`**

File: `ApiService/Mappings/AiSettingsMappings.cs`

In the `ToModel(this AiSettingsDto dto)` method, add the `BuiltInPrompts` mapping. Full replacement of that method:

```csharp
internal static AiSettingsModel ToModel(this AiSettingsDto dto) => new()
{
    Id = dto.Id,
    SelectedChatModelId = dto.SelectedChatModelId,
    PromptParameter = new ObservableCollection<PromptParameterModel>(
        (dto.PromptParameter ?? []).Select(p => p.ToModel())),
    Prompts = new ObservableCollection<PromptModel>(
        (dto.Prompts ?? []).Select(p => p.ToModel())),
    BuiltInPrompts = (dto.BuiltInPrompts ?? []).Select(p => p.ToModel()).ToList(),
};
```

> **Important — NSwag nullability:** After regeneration, check whether the generated `AiSettingsDto.BuiltInPrompts` is nullable (`ICollection<PromptDto>?`) or non-nullable (`ICollection<PromptDto>`). If it is **non-nullable**, remove the `?? []` null-coalescing operator: `BuiltInPrompts = dto.BuiltInPrompts.Select(p => p.ToModel()).ToList()`. Using `?? []` on a non-nullable reference generates CS8073 which fails the build under `TreatWarningsAsErrors = true`. Check the generated property in `ServiceClient.cs` before writing this line.

*(The existing `PromptDto.ToModel()` extension method in the same file is reused for default prompts.)*

- [ ] **Step 7: Build to verify no errors**

```bash
dotnet build AppTrack.sln --configuration Release
```
Expected: 0 errors, 0 warnings.

- [ ] **Step 8: Commit**

```bash
git add AppTrack.BlazorUi/ApiService/Base/ServiceClient.cs Models/AiSettingsModel.cs ApiService/Mappings/AiSettingsMappings.cs
git commit -m "feat: regenerate NSwag client, add BuiltInPrompts to AiSettingsModel and mapping"
```

---

### Task 8: Blazor UI — Default Prompts section in `AiSettings.razor`

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Pages/AiSettings.razor`

No changes needed to `AiSettings.razor.cs` — `_model.BuiltInPrompts` is populated automatically through the mapping updated in Task 7.

- [ ] **Step 1: Add the Default Prompts section above the existing Prompts section**

In `AppTrack.BlazorUi/Components/Pages/AiSettings.razor`, replace **only** the `<!-- Prompts -->` `<MudItem>` block (currently lines 44–97, from `<!-- Prompts -->` through the closing `</MudItem>`). **Do not modify lines 98 onward** — the `<!-- Prompt Parameters -->` section, the Save button row, and the closing `</MudGrid>`, `</MudContainer>` tags must remain exactly as-is.

Replace that block with the following two `<MudItem>` blocks — first the new Default Prompts section, then the existing My Prompts section (renamed from "Prompts" to "My Prompts"):

```razor
<!-- Default Prompts (read-only) -->
<MudItem xs="12">
    <MudStack Row="true" AlignItems="AlignItems.Center" Class="mb-2">
        <MudIcon Icon="@Icons.Material.Filled.Lock" Size="Size.Small" Color="Color.Secondary" Class="mr-1" />
        <MudText Typo="Typo.subtitle1">Default Prompts</MudText>
    </MudStack>
    @if (_model.BuiltInPrompts.Count == 0)
    {
        <MudText Typo="Typo.body2" Color="Color.Secondary" Class="ml-1">
            No default prompts available.
        </MudText>
    }
    else
    {
        <MudPaper Outlined="true" Class="pa-0" Style="max-height: 260px; overflow-y: auto;">
            @foreach (var prompt in _model.BuiltInPrompts)
            {
                <MudStack Row="true"
                          AlignItems="AlignItems.Center"
                          Class="pa-3"
                          Style="border-bottom: 1px solid var(--mud-palette-divider);">
                    <MudStack Spacing="0" Style="flex: 1; min-width: 0;">
                        <MudText Typo="Typo.subtitle2" Style="word-break: break-word;">@prompt.Name</MudText>
                        <MudText Typo="Typo.body2"
                                 Color="Color.Secondary"
                                 Style="word-break: break-word; white-space: pre-wrap;">@prompt.PromptTemplate</MudText>
                    </MudStack>
                </MudStack>
            }
        </MudPaper>
    }
</MudItem>
<!-- My Prompts -->
<MudItem xs="12">
    <MudStack Row="true" AlignItems="AlignItems.Center" Class="mb-2">
        <MudText Typo="Typo.subtitle1">My Prompts</MudText>
        <MudSpacer />
        <MudButton Variant="Variant.Outlined"
                   Color="Color.Primary"
                   StartIcon="@Icons.Material.Filled.Add"
                   Size="Size.Small"
                   OnClick="AddPromptAsync">
            Add
        </MudButton>
    </MudStack>
    @if (_model.Prompts.Count == 0)
    {
        <MudText Typo="Typo.body2" Color="Color.Secondary" Class="ml-1">
            No prompts defined.
        </MudText>
    }
    else
    {
        <MudPaper Outlined="true" Class="pa-0" Style="max-height: 260px; overflow-y: auto;">
            @foreach (var prompt in _model.Prompts)
            {
                var captured = prompt;
                <MudStack Row="true"
                          AlignItems="AlignItems.Center"
                          Class="pa-3"
                          Style="border-bottom: 1px solid var(--mud-palette-divider);">
                    <MudStack Spacing="0" Style="flex: 1; min-width: 0;">
                        <MudText Typo="Typo.subtitle2" Style="word-break: break-word;">@captured.Name</MudText>
                        <MudText Typo="Typo.body2"
                                 Color="Color.Secondary"
                                 Style="word-break: break-word; white-space: pre-wrap;">@captured.PromptTemplate</MudText>
                    </MudStack>
                    <MudStack Row="true" Spacing="0">
                        <MudTooltip Text="Edit">
                            <MudIconButton Icon="@Icons.Material.Filled.Edit"
                                           Size="Size.Small"
                                           Color="Color.Primary"
                                           OnClick="@(() => EditPromptAsync(captured))" />
                        </MudTooltip>
                        <MudTooltip Text="Delete">
                            <MudIconButton Icon="@Icons.Material.Filled.Delete"
                                           Size="Size.Small"
                                           Color="Color.Error"
                                           OnClick="@(() => DeletePrompt(captured))" />
                        </MudTooltip>
                    </MudStack>
                </MudStack>
            }
        </MudPaper>
    }
</MudItem>
```

The existing header was `<MudText Typo="Typo.subtitle1">Prompts</MudText>` — it becomes `My Prompts` as shown above.

- [ ] **Step 2: Build to verify no errors**

```bash
dotnet build AppTrack.sln --configuration Release
```
Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Run all unit tests**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```
Expected: all tests pass.

- [ ] **Step 4: Commit**

```bash
git add AppTrack.BlazorUi/Components/Pages/AiSettings.razor
git commit -m "feat: add Default Prompts read-only section to AI Settings page"
```
