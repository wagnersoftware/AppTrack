# Multiple Prompts per User Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the single `PromptTemplate` string on `AiSettings` with a named `Prompt` collection, allowing each user to manage multiple named prompts via the Blazor UI.

**Architecture:** New `Prompt` domain entity (child of `AiSettings`, same pattern as `PromptParameter`). `PromptParameter` and all existing code remain unchanged — this is purely additive except for removing `PromptTemplate`. `UpdateAiSettingsCommand` carries the full `Prompts` list inline (clear + recreate on save). No new API endpoints.

**Tech Stack:** .NET 10, EF Core 10 (MS SQL Server), MediatR, FluentValidation 12, Blazor WASM, MudBlazor, CommunityToolkit.Mvvm, NSwag

**Spec:** `docs/superpowers/specs/2026-03-21-multiple-prompts-per-user-design.md`

---

## Chunk 1: Foundation — Domain Entity + Shared Validation

### Task 1: Create `Prompt` domain entity

**Files:**
- Create: `AppTrack.Domain/Prompt.cs`

- [ ] **Step 1: Write the failing test**

Add file `AppTrack.Application.UnitTests/Domain/PromptFactoryTests.cs`:

```csharp
// AppTrack.Application.UnitTests/Domain/PromptFactoryTests.cs
using AppTrack.Domain;
using Shouldly;

namespace AppTrack.Application.UnitTests.Domain;

public class PromptFactoryTests
{
    [Fact]
    public void Create_WithNullName_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() => Prompt.Create(null, "template"));
    }

    [Fact]
    public void Create_WithNullTemplate_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() => Prompt.Create("My Prompt", null));
    }

    [Fact]
    public void Create_WithValidArgs_ShouldReturnPrompt()
    {
        var prompt = Prompt.Create("My Prompt", "Hello {name}");
        prompt.Name.ShouldBe("My Prompt");
        prompt.PromptTemplate.ShouldBe("Hello {name}");
    }
}
```

- [ ] **Step 2: Run to verify it fails (red phase)**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~PromptFactoryTests" --configuration Release
```
Expected: Build error — `AppTrack.Domain.Prompt` does not exist yet.

- [ ] **Step 3: Create the entity**

```csharp
// AppTrack.Domain/Prompt.cs
using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class Prompt : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;
    public int AiSettingsId { get; set; }        // Note: "AiSettings" (not "AISettings") — deliberate improvement over PromptParameter.AISettingsId naming
    public AiSettings AiSettings { get; set; } = null!;

    private Prompt()
    {
    }

    public static Prompt Create(string? name, string? promptTemplate)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(promptTemplate);

        return new Prompt { Name = name, PromptTemplate = promptTemplate };
    }
}
```

- [ ] **Step 4: Build domain project**

```bash
dotnet build AppTrack.Domain/AppTrack.Domain.csproj --configuration Release
```
Expected: Build succeeded, 0 errors, 0 warnings.

- [ ] **Step 5: Run factory tests to verify they pass**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~PromptFactoryTests" --configuration Release
```
Expected: 3 tests pass.

> **⚠️ Run this now.** Task 2 will remove `PromptTemplate` from `AiSettings`, breaking `AppTrack.Application`. After Task 2, `AppTrack.Application.UnitTests` will no longer build until Chunk 3. This is the last opportunity to run these factory tests.

- [ ] **Step 6: Commit**

```bash
git add AppTrack.Domain/Prompt.cs AppTrack.Application.UnitTests/Domain/PromptFactoryTests.cs
git commit -m "feat: add Prompt domain entity with factory tests"
```

---

### Task 2: Update `AiSettings` entity — remove `PromptTemplate`, add `Prompts`

**Files:**
- Modify: `AppTrack.Domain/AiSettings.cs`

- [ ] **Step 1: Edit `AiSettings.cs`**

Replace the entire file with:

```csharp
// AppTrack.Domain/AiSettings.cs
using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class AiSettings : BaseEntity
{
    public int SelectedChatModelId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ICollection<PromptParameter> PromptParameter { get; set; } = new List<PromptParameter>();

    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
```

Note: `PromptTemplate` is removed. `PromptParameter` is unchanged.

- [ ] **Step 2: Build domain project**

```bash
dotnet build AppTrack.Domain/AppTrack.Domain.csproj --configuration Release
```
Expected: Build succeeded.

> **Note:** These projects will now fail to build (they still reference `PromptTemplate`): `AppTrack.Application`, `AppTrack.Application.UnitTests`, `AppTrack.Persistance`, `AppTrack.Api`. Fix them in subsequent chunks. Do NOT run the full solution build until Task 15.

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Domain/AiSettings.cs
git commit -m "feat: replace PromptTemplate with Prompts collection on AiSettings"
```

---

### Task 3: Add `IPromptValidatable` interface

**Files:**
- Create: `AppTrack.Shared.Validation/Interfaces/IPromptValidatable.cs`

- [ ] **Step 1: Create the interface**

```csharp
// AppTrack.Shared.Validation/Interfaces/IPromptValidatable.cs
namespace AppTrack.Shared.Validation.Interfaces;

public interface IPromptValidatable
{
    string Name { get; }
    string PromptTemplate { get; }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.Shared.Validation/AppTrack.Shared.Validation.csproj --configuration Release
```
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Shared.Validation/Interfaces/IPromptValidatable.cs
git commit -m "feat: add IPromptValidatable interface"
```

---

### Task 4: Add `PromptBaseValidator<T>`

**Files:**
- Create: `AppTrack.Shared.Validation/Validators/PromptBaseValidator.cs`

- [ ] **Step 1: Write the failing test**

> **Reference resolution:** The reference chain `AppTrack.Application.UnitTests → AppTrack.Application → AppTrack.Shared.Validation` means `IPromptValidatable` and `PromptBaseValidator` are transitively visible — no extra project reference is needed in the test `.csproj`. (Note: `AppTrack.Application` does not compile in Chunk 1 due to Task 2, but the reference path exists structurally and will be active once Chunk 3 restores compilation.)

Create test file `AppTrack.Application.UnitTests/Validators/PromptBaseValidatorTests.cs`:

```csharp
// AppTrack.Application.UnitTests/Validators/PromptBaseValidatorTests.cs
using AppTrack.Shared.Validation.Interfaces;
using AppTrack.Shared.Validation.Validators;
using FluentValidation;
using Shouldly;

namespace AppTrack.Application.UnitTests.Validators;

// Concrete test implementation
file class TestPromptValidator : PromptBaseValidator<TestPrompt> { }

file class TestPrompt : IPromptValidatable
{
    public string Name { get; init; } = string.Empty;
    public string PromptTemplate { get; init; } = string.Empty;
}

public class PromptBaseValidatorTests
{
    private readonly TestPromptValidator _validator = new();

    [Fact]
    public async Task ValidPrompt_ShouldPass()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Name = "My Prompt", PromptTemplate = "Hello {name}" });
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task EmptyName_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Name = "", PromptTemplate = "Hello" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task NameTooLong_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Name = new string('a', 101), PromptTemplate = "Hello" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task EmptyPromptTemplate_ShouldFail()
    {
        var result = await _validator.ValidateAsync(new TestPrompt { Name = "My Prompt", PromptTemplate = "" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PromptTemplate");
    }
}
```

- [ ] **Step 2: ⏭ Deferred — cannot run yet**

> After Task 2 removed `PromptTemplate` from `AiSettings`, `AppTrack.Application` no longer compiles (it still references the removed field). `AppTrack.Application.UnitTests` depends on `AppTrack.Application`, so any test run here fails with a cascading build error, **not** a clean "PromptBaseValidator not found" failure. The TDD red phase is acknowledged but cannot be independently verified at this point — the test file's presence establishes intent. These tests are verified in **Task 15, Step 3**.

- [ ] **Step 3: Create `PromptBaseValidator.cs`**

```csharp
// AppTrack.Shared.Validation/Validators/PromptBaseValidator.cs
using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class PromptBaseValidator<T> : AbstractValidator<T>
    where T : IPromptValidatable
{
    protected PromptBaseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.PromptTemplate)
            .NotEmpty().WithMessage("Prompt template is required.");
    }
}
```

- [ ] **Step 4: ⏭ Deferred — cannot run yet**

> Same constraint as Step 2: `AppTrack.Application` does not compile until Chunk 3. These tests are verified in **Task 15, Step 3**.

- [ ] **Step 5: Commit**

```bash
git add AppTrack.Shared.Validation/Validators/PromptBaseValidator.cs AppTrack.Application.UnitTests/Validators/PromptBaseValidatorTests.cs
git commit -m "feat: add PromptBaseValidator with tests"
```

---

### Task 5: Update `IAiSettingsValidatable` — add `Prompts`

**Files:**
- Modify: `AppTrack.Shared.Validation/Interfaces/IAiSettingsValidatable.cs`

Current content:
```csharp
public interface IAiSettingsValidatable
{
    IEnumerable<IPromptParameterValidatable> PromptParameter { get; }
}
```

- [ ] **Step 1: Add `Prompts` property**

```csharp
// AppTrack.Shared.Validation/Interfaces/IAiSettingsValidatable.cs
namespace AppTrack.Shared.Validation.Interfaces;

public interface IAiSettingsValidatable
{
    IEnumerable<IPromptParameterValidatable> PromptParameter { get; }
    IEnumerable<IPromptValidatable> Prompts { get; }
}
```

- [ ] **Step 2: Build shared validation project only**

```bash
dotnet build AppTrack.Shared.Validation/AppTrack.Shared.Validation.csproj --configuration Release
```
Expected: Build succeeded.

> **Do NOT run `dotnet test` or build the full solution at this point.** `UpdateAiSettingsCommand` (Application layer) and `AiSettingsModel` (Frontend.Models) implement `IAiSettingsValidatable` — they are now missing the `Prompts` member and will fail to compile until fixed in Chunk 3/4. Run only the shared validation project build here.

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Shared.Validation/Interfaces/IAiSettingsValidatable.cs
git commit -m "feat: add Prompts to IAiSettingsValidatable"
```

---

### Task 6: Update `AiSettingsBaseValidator<T>` — add Prompts rules

**Files:**
- Modify: `AppTrack.Shared.Validation/Validators/AiSettingsBaseValidator.cs`

- [ ] **Step 1: Write the failing test**

Add to `AppTrack.Application.UnitTests/Validators/PromptBaseValidatorTests.cs` (or create a new file `AiSettingsBaseValidatorTests.cs`):

Create `AppTrack.Application.UnitTests/Validators/AiSettingsBaseValidatorTests.cs`:

```csharp
// AppTrack.Application.UnitTests/Validators/AiSettingsBaseValidatorTests.cs
using AppTrack.Shared.Validation.Interfaces;
using AppTrack.Shared.Validation.Validators;
using Shouldly;

namespace AppTrack.Application.UnitTests.Validators;

file class TestAiSettingsValidator : AiSettingsBaseValidator<TestAiSettings> { }

file class TestAiSettings : IAiSettingsValidatable
{
    public IEnumerable<IPromptParameterValidatable> PromptParameter { get; init; } = [];
    public IEnumerable<IPromptValidatable> Prompts { get; init; } = [];
}

file class TestPromptItem : IPromptValidatable
{
    public string Name { get; init; } = string.Empty;
    public string PromptTemplate { get; init; } = string.Empty;
}

file class TestPromptParameterItem : IPromptParameterValidatable
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public class AiSettingsBaseValidatorTests
{
    private readonly TestAiSettingsValidator _validator = new();

    [Fact]
    public async Task EmptyPrompts_ShouldPass()
    {
        var result = await _validator.ValidateAsync(new TestAiSettings());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidPrompts_ShouldPass()
    {
        var settings = new TestAiSettings
        {
            Prompts =
            [
                new TestPromptItem { Name = "Prompt A", PromptTemplate = "Hello" },
                new TestPromptItem { Name = "Prompt B", PromptTemplate = "World" },
            ]
        };
        var result = await _validator.ValidateAsync(settings);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task DuplicatePromptNames_ShouldFail()
    {
        var settings = new TestAiSettings
        {
            Prompts =
            [
                new TestPromptItem { Name = "Same", PromptTemplate = "Hello" },
                new TestPromptItem { Name = "same", PromptTemplate = "World" }, // case-insensitive duplicate
            ]
        };
        var result = await _validator.ValidateAsync(settings);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Prompts");
    }

    [Fact]
    public async Task PromptWithEmptyName_ShouldFail()
    {
        var settings = new TestAiSettings
        {
            Prompts = [new TestPromptItem { Name = "", PromptTemplate = "Hello" }]
        };
        var result = await _validator.ValidateAsync(settings);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task PromptWithEmptyTemplate_ShouldFail()
    {
        // Verifies RuleForEach -> PromptItemValidator -> PromptTemplate rule is wired correctly
        var settings = new TestAiSettings
        {
            Prompts = [new TestPromptItem { Name = "Valid", PromptTemplate = "" }]
        };
        var result = await _validator.ValidateAsync(settings);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task DuplicateParameterKeys_ShouldFail()
    {
        // Regression: collection-level PromptParameter uniqueness rule still fires after additive change
        var settings = new TestAiSettings
        {
            PromptParameter =
            [
                new TestPromptParameterItem { Key = "key", Value = "a" },
                new TestPromptParameterItem { Key = "KEY", Value = "b" }, // case-insensitive duplicate
            ]
        };
        var result = await _validator.ValidateAsync(settings);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "PromptParameter");
    }

    [Fact]
    public async Task ParameterWithEmptyKey_ShouldFail()
    {
        // Regression: item-level PromptParameter validation (RuleForEach -> PromptParameterItemValidator) still fires
        var settings = new TestAiSettings
        {
            PromptParameter = [new TestPromptParameterItem { Key = "", Value = "v" }]
        };
        var result = await _validator.ValidateAsync(settings);
        result.IsValid.ShouldBeFalse();
    }
}
```

- [ ] **Step 2: ⏭ Deferred — cannot run yet**

> `AppTrack.Application` does not compile until Chunk 3 (it still references the removed `PromptTemplate` field). Running `AppTrack.Application.UnitTests` here produces a build error unrelated to this task. These tests are verified in **Task 15, Step 3**.

- [ ] **Step 3: Update `AiSettingsBaseValidator.cs`**

```csharp
// AppTrack.Shared.Validation/Validators/AiSettingsBaseValidator.cs
using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class AiSettingsBaseValidator<T> : AbstractValidator<T>
    where T : IAiSettingsValidatable
{
    protected AiSettingsBaseValidator()
    {
        // Existing PromptParameter rules — unchanged
        RuleForEach(x => x.PromptParameter)
            .SetValidator(new PromptParameterItemValidator());

        RuleFor(x => x.PromptParameter)
            .Must(HaveUniqueKeys)
            .WithMessage("Each prompt parameter key must be unique.");

        // New Prompts rules
        RuleForEach(x => x.Prompts)
            .SetValidator(new PromptItemValidator());

        RuleFor(x => x.Prompts)
            .Must(HaveUniqueNames)
            .WithMessage("Each prompt name must be unique.");
    }

    private static bool HaveUniqueKeys(IEnumerable<IPromptParameterValidatable> parameters)
    {
        var list = parameters?.ToList();
        if (list is null || list.Count == 0)
            return true;

        return list.Select(p => p.Key)
                   .GroupBy(k => k, StringComparer.OrdinalIgnoreCase)
                   .All(g => g.Count() == 1);
    }

    private static bool HaveUniqueNames(IEnumerable<IPromptValidatable> prompts)
    {
        var list = prompts?.ToList();
        if (list is null || list.Count == 0)
            return true;

        return list.Select(p => p.Name)
                   .GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
                   .All(g => g.Count() == 1);
    }

    private sealed class PromptParameterItemValidator : PromptParameterBaseValidator<IPromptParameterValidatable>
    {
        public PromptParameterItemValidator() : base() { }
    }

    private sealed class PromptItemValidator : PromptBaseValidator<IPromptValidatable>
    {
        public PromptItemValidator() : base() { }
    }
}
```

- [ ] **Step 4: ⏭ Deferred — cannot run yet**

> Same constraint as Step 2. These tests are verified in **Task 15, Step 3**.

- [ ] **Step 5: Commit**

```bash
git add AppTrack.Shared.Validation/Validators/AiSettingsBaseValidator.cs AppTrack.Application.UnitTests/Validators/AiSettingsBaseValidatorTests.cs
git commit -m "feat: add Prompts validation rules to AiSettingsBaseValidator"
```

---

## Chunk 2: Persistence Layer

### Task 7: Add `PromptConfiguration`

**Files:**
- Create: `AppTrack.Persistance/Configurations/PromptConfiguration.cs`

- [ ] **Step 1: Create the configuration**

```csharp
// AppTrack.Persistance/Configurations/PromptConfiguration.cs
using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class PromptConfiguration : IEntityTypeConfiguration<Prompt>
{
    public void Configure(EntityTypeBuilder<Prompt> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PromptTemplate)
            .IsRequired();

        builder.HasIndex(x => new { x.AiSettingsId, x.Name })
            .IsUnique();
    }
}
```

- [ ] **Step 2: Build persistence project**

```bash
dotnet build AppTrack.Persistance/AppTrack.Persistance.csproj --configuration Release
```
Expected: Build succeeded (assuming domain project already built).

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Persistance/Configurations/PromptConfiguration.cs
git commit -m "feat: add EF Core PromptConfiguration"
```

---

### Task 8: Update `AiSettingsConfiguration` — add `Prompts` relationship

**Files:**
- Modify: `AppTrack.Persistance/Configurations/AiSettingsConfiguration.cs`

Current content:
```csharp
builder.HasMany(s => s.PromptParameter)
    .WithOne(p => p.AISettings)
    .HasForeignKey(p => p.AISettingsId)
    .OnDelete(DeleteBehavior.Cascade);
```

- [ ] **Step 1: Add `Prompts` relationship**

```csharp
// AppTrack.Persistance/Configurations/AiSettingsConfiguration.cs
using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class AiSettingsConfiguration : IEntityTypeConfiguration<AiSettings>
{
    public void Configure(EntityTypeBuilder<AiSettings> builder)
    {
        builder.HasMany(s => s.PromptParameter)
            .WithOne(p => p.AISettings)
            .HasForeignKey(p => p.AISettingsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Prompts)
            .WithOne(p => p.AiSettings)
            .HasForeignKey(p => p.AiSettingsId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.Persistance/AppTrack.Persistance.csproj --configuration Release
```
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Persistance/Configurations/AiSettingsConfiguration.cs
git commit -m "feat: configure Prompts relationship in AiSettingsConfiguration"
```

---

### Task 9: Update `AiSettingsRepository` — include `Prompts` in both methods

**Files:**
- Modify: `AppTrack.Persistance/Repositories/AiSettingsRepository.cs`

- [ ] **Step 1: Add `.Include(s => s.Prompts)` to both methods**

```csharp
// AppTrack.Persistance/Repositories/AiSettingsRepository.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class AiSettingsRepository : GenericRepository<AiSettings>, IAiSettingsRepository
{
    public AiSettingsRepository(AppTrackDatabaseContext dbContext) : base(dbContext)
    {
    }

    public async Task<AiSettings?> GetByIdWithPromptParameterAsync(int id)
    {
        return await _context.AiSettings
            .Include(s => s.PromptParameter)
            .Include(s => s.Prompts)
            .SingleOrDefaultAsync(s => s.Id == id);
    }

    public async Task<AiSettings?> GetByUserIdIncludePromptParameterAsync(string userId)
    {
        return await _context.AiSettings.AsNoTracking()
            .Include(s => s.PromptParameter)
            .Include(s => s.Prompts)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.Persistance/AppTrack.Persistance.csproj --configuration Release
```
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Persistance/Repositories/AiSettingsRepository.cs
git commit -m "feat: include Prompts in AiSettingsRepository eager-load methods"
```

---

### Task 10: Create EF Core migration

> **Prerequisites:** The API project must build cleanly. Complete all Domain + Persistence tasks first, and enough of the Application layer for the API to build. If the API doesn't build yet, skip this task and come back after Task 15.

- [ ] **Step 1: Add the migration**

Run from the repo root:

```bash
dotnet ef migrations add AddPromptsTable --project AppTrack.Persistance/AppTrack.Persistance.csproj --startup-project AppTrack.Api/AppTrack.Api.csproj
```

Expected output: `Done. To undo this action, use 'ef migrations remove'`

The generated migration should:
- Create table `Prompts` with columns: `Id`, `Name` (nvarchar(100)), `PromptTemplate` (nvarchar(max)), `AiSettingsId` (int, FK), `CreationDate`, `ModifiedDate`
- Create unique index `IX_Prompts_AiSettingsId_Name`
- Drop column `PromptTemplate` from `AiSettings`

> **Verify:** Open the generated migration file and confirm those three operations are present.

- [ ] **Step 2: Commit**

```bash
git add AppTrack.Persistance/Migrations/
git commit -m "feat: add migration for Prompts table and remove PromptTemplate from AiSettings"
```

---

## Chunk 3: Application Layer

### Task 11: Add `PromptDto`, update `AiSettingsDto`

**Files:**
- Create: `AppTrack.Application/Features/AiSettings/Dto/PromptDto.cs`
- Modify: `AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs`

- [ ] **Step 1: Create `PromptDto`**

```csharp
// AppTrack.Application/Features/AiSettings/Dto/PromptDto.cs
using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Application.Features.AiSettings.Dto;

public class PromptDto : IPromptValidatable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Update `AiSettingsDto`**

Replace with:

```csharp
// AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs
namespace AppTrack.Application.Features.AiSettings.Dto;

public class AiSettingsDto
{
    public int SelectedChatModelId { get; set; }
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<PromptParameterDto> PromptParameter { get; set; } = new List<PromptParameterDto>();
    public List<PromptDto> Prompts { get; set; } = new List<PromptDto>();
}
```

(`PromptTemplate` removed, `Prompts` added.)

- [ ] **Step 3: Build application project**

```bash
dotnet build AppTrack.Application/AppTrack.Application.csproj --configuration Release
```
Expected: Some errors from `UpdateAiSettingsCommand` and mappings still referencing `PromptTemplate` — fix in next tasks.

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Application/Features/AiSettings/Dto/PromptDto.cs AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs
git commit -m "feat: add PromptDto and update AiSettingsDto"
```

---

### Task 12: Update `UpdateAiSettingsCommand`

**Files:**
- Modify: `AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/UpdateAiSettingsCommand.cs`

- [ ] **Step 1: Replace with updated command**

```csharp
// AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/UpdateAiSettingsCommand.cs
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class UpdateAiSettingsCommand : IRequest<AiSettingsDto>, IAiSettingsValidatable, IUserScopedRequest
{
    public int SelectedChatModelId { get; set; }
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<PromptParameterDto> PromptParameter { get; set; } = new List<PromptParameterDto>();
    public List<PromptDto> Prompts { get; set; } = new List<PromptDto>();

    IEnumerable<IPromptParameterValidatable> IAiSettingsValidatable.PromptParameter => PromptParameter;
    IEnumerable<IPromptValidatable> IAiSettingsValidatable.Prompts => Prompts;
}
```

(`PromptTemplate` removed, `Prompts` added with explicit interface implementation.)

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.Application/AppTrack.Application.csproj --configuration Release
```
Expected: Remaining errors from `AiSettingsMappings.cs` and `GeneratePromptQueryHandler.cs` referencing `PromptTemplate`.

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/UpdateAiSettingsCommand.cs
git commit -m "feat: update UpdateAiSettingsCommand to use Prompts collection"
```

---

### Task 13: Add `PromptDtoValidator`

**Files:**
- Create: `AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/PromptDtoValidator.cs`

- [ ] **Step 1: Create the validator**

```csharp
// AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/PromptDtoValidator.cs
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Shared.Validation.Validators;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class PromptDtoValidator : PromptBaseValidator<PromptDto>
{
    public PromptDtoValidator() : base() { }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.Application/AppTrack.Application.csproj --configuration Release
```

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/PromptDtoValidator.cs
git commit -m "feat: add PromptDtoValidator"
```

---

### Task 14: Update application mappings

**Files:**
- Modify: `AppTrack.Application/Mappings/AiSettingsMappings.cs`

- [ ] **Step 1: Replace the entire mappings file**

```csharp
// AppTrack.Application/Mappings/AiSettingsMappings.cs
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;
using AppTrack.Domain;

namespace AppTrack.Application.Mappings;

internal static class AiSettingsMappings
{
    internal static AiSettings ToDomain(this GetAiSettingsByUserIdQuery query) => new()
    {
        UserId = query.UserId,
    };

    internal static void ApplyTo(this UpdateAiSettingsCommand command, AiSettings entity)
    {
        entity.SelectedChatModelId = command.SelectedChatModelId;
        entity.UserId = command.UserId;

        entity.PromptParameter.Clear();
        foreach (var dto in command.PromptParameter)
        {
            entity.PromptParameter.Add(PromptParameter.Create(dto.Key, dto.Value));
        }

        entity.Prompts.Clear();
        foreach (var dto in command.Prompts)
        {
            entity.Prompts.Add(Prompt.Create(dto.Name, dto.PromptTemplate));
        }
    }

    internal static AiSettingsDto ToDto(this AiSettings entity) => new()
    {
        Id = entity.Id,
        SelectedChatModelId = entity.SelectedChatModelId,
        UserId = entity.UserId,
        PromptParameter = entity.PromptParameter.Select(p => p.ToDto()).ToList(),
        Prompts = entity.Prompts.Select(p => p.ToDto()).ToList(),
    };

    internal static PromptParameterDto ToDto(this PromptParameter entity) => new()
    {
        Id = entity.Id,
        Key = entity.Key,
        Value = entity.Value,
    };

    internal static PromptDto ToDto(this Prompt entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        PromptTemplate = entity.PromptTemplate,
    };
}
```

- [ ] **Step 2: Build application project**

```bash
dotnet build AppTrack.Application/AppTrack.Application.csproj --configuration Release
```
Expected: 1 remaining error from `GeneratePromptQueryHandler.cs`.

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Application/Mappings/AiSettingsMappings.cs
git commit -m "feat: update application mappings for Prompts"
```

---

### Task 15: Fix `GeneratePromptQueryHandler` — interim `PromptTemplate` fallback

**Files:**
- Modify: `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryHandler.cs`

Current line 43:
```csharp
var (prompt, unusedKeys) = _promptBuilder.BuildPrompt(promptParameter, aiSettings.PromptTemplate);
```

- [ ] **Step 1: Replace `aiSettings.PromptTemplate` with fallback**

Change only line 43:

```csharp
var promptTemplate = aiSettings!.Prompts.FirstOrDefault()?.PromptTemplate ?? string.Empty;
var (prompt, unusedKeys) = _promptBuilder.BuildPrompt(promptParameter, promptTemplate);
```

The full updated handler body (lines 24–46):

```csharp
public async Task<GeneratedPromptDto> Handle(GeneratePromptQuery request, CancellationToken cancellationToken)
{
    var validator = new GeneratePromptQueryValidator(_jobApplicationRepository, _aiSettingsRepository);
    var validationResult = await validator.ValidateAsync(request);

    if (validationResult.Errors.Any())
    {
        throw new BadRequestException($"Invalid Ai setting.", validationResult);
    }

    //get Ai settings
    var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId);

    //get job application
    var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId);

    //build prompt
    var applicantParameter = aiSettings!.PromptParameter.ToList();
    var jobApplicationParameter = jobApplication!.ToPromptParameters().ToList();
    var promptParameter = jobApplicationParameter.Union(applicantParameter).ToList();
    var promptTemplate = aiSettings.Prompts.FirstOrDefault()?.PromptTemplate ?? string.Empty;
    var (prompt, unusedKeys) = _promptBuilder.BuildPrompt(promptParameter, promptTemplate);

    return new GeneratedPromptDto() { Prompt = prompt, UnusedKeys = unusedKeys };
}
```

- [ ] **Step 2: Build the full solution**

```bash
dotnet build AppTrack.sln --configuration Release
```
Expected: Build succeeded, 0 errors, 0 warnings. This is the first time the full solution should build cleanly.

- [ ] **Step 3: Run all unit tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```
Expected: All tests pass — including `PromptBaseValidatorTests` and `AiSettingsBaseValidatorTests` written in Tasks 4 and 6. This is the first point at which those tests can run, now that `AppTrack.Application` compiles cleanly.

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryHandler.cs
git commit -m "fix: use Prompts.FirstOrDefault() in GeneratePromptQueryHandler (interim until prompt selection)"
```

---

### Task 16: Add unit test for `UpdateAiSettingsCommandHandler` with Prompts

**Files:**
- Create: `AppTrack.Application.UnitTests/Features/AiSettings/Commands/UpdateAiSettingsCommandHandlerTests.cs`
- Create: `AppTrack.Application.UnitTests/Mocks/MockAiSettingsRepository.cs`

- [ ] **Step 1: Create mock repository**

```csharp
// AppTrack.Application.UnitTests/Mocks/MockAiSettingsRepository.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using Moq;

namespace AppTrack.Application.UnitTests.Mocks;

public static class MockAiSettingsRepository
{
    public static Mock<IAiSettingsRepository> GetMock()
    {
        var mockRepo = new Mock<IAiSettingsRepository>();

        var aiSettings = new AiSettings
        {
            Id = 1,
            UserId = "user1",
            SelectedChatModelId = 1,
        };

        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(aiSettings);
        mockRepo.Setup(r => r.GetByIdWithPromptParameterAsync(1)).ReturnsAsync(aiSettings);
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<AiSettings>())).Returns(Task.CompletedTask);

        return mockRepo;
    }
}
```

- [ ] **Step 2: Create handler test**

```csharp
// AppTrack.Application.UnitTests/Features/AiSettings/Commands/UpdateAiSettingsCommandHandlerTests.cs
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.UnitTests.Mocks;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Commands;

public class UpdateAiSettingsCommandHandlerTests
{
    private readonly UpdateAiSettingsCommandHandler _handler;

    public UpdateAiSettingsCommandHandlerTests()
    {
        var mockRepo = MockAiSettingsRepository.GetMock();
        _handler = new UpdateAiSettingsCommandHandler(mockRepo.Object);
    }

    [Fact]
    public async Task Handle_WithPrompts_ShouldReturnDtoWithPrompts()
    {
        var command = new UpdateAiSettingsCommand
        {
            Id = 1,
            UserId = "user1",
            SelectedChatModelId = 1,
            Prompts =
            [
                new PromptDto { Name = "My Prompt", PromptTemplate = "Hello {name}" }
            ]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeOfType<AiSettingsDto>();
        result.Prompts.Count.ShouldBe(1);
        result.Prompts[0].Name.ShouldBe("My Prompt");
    }

    [Fact]
    public async Task Handle_WithEmptyPrompts_ShouldReturnEmptyPromptsList()
    {
        var command = new UpdateAiSettingsCommand
        {
            Id = 1,
            UserId = "user1",
            SelectedChatModelId = 1,
            Prompts = []
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Prompts.ShouldBeEmpty();
    }
}
```

- [ ] **Step 3: Run the tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~UpdateAiSettingsCommandHandlerTests" --configuration Release
```
Expected: 2 tests pass.

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Application.UnitTests/Mocks/MockAiSettingsRepository.cs AppTrack.Application.UnitTests/Features/AiSettings/Commands/UpdateAiSettingsCommandHandlerTests.cs
git commit -m "test: add UpdateAiSettingsCommandHandler tests for Prompts"
```

---

## Chunk 3 Review — Run migration and smoke test the API

> **Before moving to frontend:** Verify the migration runs and the API starts.

- [ ] **Step 1: Start the API (https profile) and verify it starts without errors**

```bash
dotnet run --project AppTrack.Api/AppTrack.Api.csproj --launch-profile https
```
Expected: API starts, migration applied automatically, Swagger available at `https://localhost:7273/swagger`.

- [ ] **Step 2: Verify `PUT /api/ai-settings/{id}` accepts `Prompts` in the body**

In Swagger UI, try sending a request with body:
```json
{
  "id": 1,
  "selectedChatModelId": 1,
  "userId": "...",
  "promptParameter": [],
  "prompts": [{ "id": 0, "name": "Test", "promptTemplate": "Hello {name}" }]
}
```
Expected: 200 OK, response includes `prompts` array.

---

## Chunk 4: Frontend Models + API Service

### Task 17: Regenerate NSwag client

**Files:**
- Modify: `ApiService/Base/ServiceClient.cs` (auto-generated — do not edit manually)
- Modify: `ApiService/Base/clientsettings.nswag` (update runtime version)

**Background:** The NSwag config at `ApiService/Base/clientsettings.nswag` fetches the Swagger JSON from the live API at `https://localhost:7273/swagger/v1/swagger.json`. The client must be regenerated after the API contract changes.

- [ ] **Step 1: Ensure the API is running** (from Task 16 smoke test, or restart)

```bash
dotnet run --project AppTrack.Api/AppTrack.Api.csproj --launch-profile https
```

- [ ] **Step 2: Update the runtime in `clientsettings.nswag`**

Change `"runtime": "Net80"` to `"runtime": "Net100"` (matches .NET 10 project target).

- [ ] **Step 3: Run NSwag code generation from the `ApiService/Base/` directory**

```bash
cd ApiService/Base
dotnet tool run nswag run clientsettings.nswag /runtime:Net100
```

If `nswag` tool is not installed locally:
```bash
dotnet tool install NSwag.ConsoleCore --global
nswag run clientsettings.nswag /runtime:Net100
```

Alternatively, in Visual Studio: right-click `ApiService/Base/clientsettings.nswag` → "Generate Files".

Expected: `ServiceClient.cs` is regenerated. The new `AiSettingsDto` class will have `Prompts` and no `PromptTemplate`. The `UpdateAiSettingsCommand` class will have `Prompts` and no `PromptTemplate`.

- [ ] **Step 4: Build the ApiService project**

```bash
dotnet build ApiService/ApiService.csproj --configuration Release
```
Expected: Build errors in `ApiService/Mappings/AiSettingsMappings.cs` referencing `dto.PromptTemplate` / `model.PromptTemplate` — fix in Task 21.

- [ ] **Step 5: Commit the regenerated client**

```bash
git add ApiService/Base/ServiceClient.cs ApiService/Base/clientsettings.nswag
git commit -m "feat: regenerate NSwag client after PromptTemplate removal"
```

---

### Task 18: Create `PromptModel`

**Files:**
- Create: `Models/PromptModel.cs`

- [ ] **Step 1: Create `PromptModel`**

```csharp
// Models/PromptModel.cs
using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppTrack.Frontend.Models;

public partial class PromptModel : ModelBase, IPromptValidatable
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string promptTemplate = string.Empty;

    public IEnumerable<PromptModel>? SiblingPrompts { get; set; }

    public Guid TempId { get; set; } = Guid.NewGuid();

    public PromptModel Clone()
    {
        return new PromptModel
        {
            Id = Id,
            Name = Name,
            PromptTemplate = PromptTemplate,
            CreationDate = CreationDate,
            ModifiedDate = ModifiedDate,
            SiblingPrompts = SiblingPrompts,
            TempId = TempId
        };
    }
}
```

- [ ] **Step 2: Build models project**

```bash
dotnet build Models/AppTrack.Frontend.Models.csproj --configuration Release
```
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add Models/PromptModel.cs
git commit -m "feat: add PromptModel"
```

---

### Task 19: Update `AiSettingsModel`

**Files:**
- Modify: `Models/AiSettingsModel.cs`

- [ ] **Step 1: Replace with updated model**

```csharp
// Models/AiSettingsModel.cs
using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;
using System.Collections.ObjectModel;

namespace AppTrack.Frontend.Models;

public partial class AiSettingsModel : ModelBase, IAiSettingsValidatable
{
    public int SelectedChatModelId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ObservableCollection<PromptParameterModel> PromptParameter { get; set; } = new ObservableCollection<PromptParameterModel>();
    public ObservableCollection<PromptModel> Prompts { get; set; } = new ObservableCollection<PromptModel>();

    IEnumerable<IPromptParameterValidatable> IAiSettingsValidatable.PromptParameter => PromptParameter;
    IEnumerable<IPromptValidatable> IAiSettingsValidatable.Prompts => Prompts;
}
```

(`PromptTemplate` removed, `Prompts` added with explicit interface implementation.)

- [ ] **Step 2: Build**

```bash
dotnet build Models/AppTrack.Frontend.Models.csproj --configuration Release
```
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add Models/AiSettingsModel.cs
git commit -m "feat: update AiSettingsModel with Prompts collection"
```

---

### Task 20: Add `PromptModelValidator`

**Files:**
- Create: `Models/Validators/PromptModelValidator.cs`

- [ ] **Step 1: Create the validator**

```csharp
// Models/Validators/PromptModelValidator.cs
using AppTrack.Shared.Validation.Validators;
using FluentValidation;

namespace AppTrack.Frontend.Models.Validators;

public class PromptModelValidator : PromptBaseValidator<PromptModel>
{
    public PromptModelValidator()
    {
        RuleFor(x => x.Name)
            .Must((model, name) => model.SiblingPrompts == null ||
                                   !model.SiblingPrompts.Any(p => p.TempId != model.TempId &&
                                                                   string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("A prompt with this name already exists.");
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build Models/AppTrack.Frontend.Models.csproj --configuration Release
```
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add Models/Validators/PromptModelValidator.cs
git commit -m "feat: add PromptModelValidator"
```

---

### Task 21: Update frontend API service mappings

**Files:**
- Modify: `ApiService/Mappings/AiSettingsMappings.cs`

- [ ] **Step 1: Replace the entire file**

```csharp
// ApiService/Mappings/AiSettingsMappings.cs
using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;
using System.Collections.ObjectModel;

namespace AppTrack.Frontend.ApiService.Mappings;

internal static class AiSettingsMappings
{
    internal static AiSettingsModel ToModel(this AiSettingsDto dto) => new()
    {
        Id = dto.Id,
        SelectedChatModelId = dto.SelectedChatModelId,
        UserId = dto.UserId ?? string.Empty,
        PromptParameter = new ObservableCollection<PromptParameterModel>(
            (dto.PromptParameter ?? []).Select(p => p.ToModel())),
        Prompts = new ObservableCollection<PromptModel>(
            (dto.Prompts ?? []).Select(p => p.ToModel())),
    };

    internal static PromptParameterModel ToModel(this PromptParameterDto dto) => new()
    {
        Id = dto.Id,
        Key = dto.Key ?? string.Empty,
        Value = dto.Value ?? string.Empty,
    };

    internal static PromptModel ToModel(this PromptDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name ?? string.Empty,
        PromptTemplate = dto.PromptTemplate ?? string.Empty,
    };

    internal static UpdateAiSettingsCommand ToUpdateCommand(this AiSettingsModel model) => new()
    {
        Id = model.Id,
        SelectedChatModelId = model.SelectedChatModelId,
        UserId = model.UserId,
        PromptParameter = model.PromptParameter.Select(p => p.ToDto()).ToList(),
        Prompts = model.Prompts.Select(p => p.ToDto()).ToList(),
    };

    internal static PromptParameterDto ToDto(this PromptParameterModel model) => new()
    {
        Id = model.Id,
        Key = model.Key,
        Value = model.Value,
    };

    internal static PromptDto ToDto(this PromptModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        PromptTemplate = model.PromptTemplate,
    };
}
```

> **Note:** The exact generated class names (`AiSettingsDto`, `UpdateAiSettingsCommand`, `PromptDto`) come from `ServiceClient.cs`. Verify these match after NSwag regeneration.

- [ ] **Step 2: Build the full frontend**

```bash
dotnet build ApiService/ApiService.csproj --configuration Release
dotnet build Models/AppTrack.Frontend.Models.csproj --configuration Release
```
Expected: Both build succeeded.

- [ ] **Step 3: Commit**

```bash
git add ApiService/Mappings/AiSettingsMappings.cs
git commit -m "feat: update frontend ApiService mappings for Prompts"
```

---

### Task 22: Register `PromptModelValidator` in DI

**Files:**
- Modify: `AppTrack.BlazorUi/Program.cs`

- [ ] **Step 1: Add validator registration**

After the existing validator registrations (around line 38), add:

```csharp
builder.Services.AddTransient<IValidator<PromptModel>, PromptModelValidator>();
```

The updated block should look like:

```csharp
builder.Services.AddTransient<IValidator<JobApplicationModel>, JobApplicationModelValidator>();
builder.Services.AddTransient<IValidator<AiSettingsModel>, AiSettingsModelValidator>();
builder.Services.AddTransient<IValidator<PromptParameterModel>, PromptParameterModelValidator>();
builder.Services.AddTransient<IValidator<PromptModel>, PromptModelValidator>();
builder.Services.AddTransient(typeof(IModelValidator<>), typeof(ModelValidator<>));
```

- [ ] **Step 2: Build Blazor project**

```bash
dotnet build AppTrack.BlazorUi/AppTrack.BlazorUi.csproj --configuration Release
```
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add AppTrack.BlazorUi/Program.cs
git commit -m "feat: register PromptModelValidator in DI"
```

---

## Chunk 5: Blazor UI

### Task 23: Update `AiSettingsDialog`

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor`
- Modify: `AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor.cs`

#### Step A: Update the code-behind (`.razor.cs`)

- [ ] **Step 1: Remove `OnPromptTemplateChanged` method**

In `AiSettingsDialog.razor.cs`, delete lines 65–69:

```csharp
// DELETE these lines:
private void OnPromptTemplateChanged(string value)
{
    _model.PromptTemplate = value;
    ModelValidator.ResetErrors(nameof(AiSettingsModel.PromptTemplate));
}
```

- [ ] **Step 2: Add Prompt CRUD methods**

Add these three methods to the class (after the `EditPromptParameterAsync` method, following the same pattern):

```csharp
private async Task AddPromptAsync()
{
    var parameters = new DialogParameters<PromptDialog>
    {
        { x => x.SiblingPrompts, _model.Prompts },
    };

    var dialog = await DialogService.ShowAsync<PromptDialog>("", parameters, _paramDialogOptions);
    var result = await dialog.Result;

    if (result is { Canceled: true }) return;
    if (result?.Data is not PromptModel newPrompt) return;

    _model.Prompts.Add(newPrompt);
    await InvokeAsync(StateHasChanged);
}

private async Task EditPromptAsync(PromptModel prompt)
{
    var parameters = new DialogParameters<PromptDialog>
    {
        { x => x.ExistingPrompt, prompt },
        { x => x.SiblingPrompts, _model.Prompts },
    };

    var dialog = await DialogService.ShowAsync<PromptDialog>("", parameters, _paramDialogOptions);
    var result = await dialog.Result;

    if (result is { Canceled: true }) return;
    if (result?.Data is not PromptModel updatedPrompt) return;

    prompt.Name = updatedPrompt.Name;
    prompt.PromptTemplate = updatedPrompt.PromptTemplate;

    await InvokeAsync(StateHasChanged);
}

private void DeletePrompt(PromptModel prompt)
{
    _model.Prompts.Remove(prompt);
}
```

- [ ] **Step 3: Build to verify `.razor.cs` compiles**

```bash
dotnet build AppTrack.BlazorUi/AppTrack.BlazorUi.csproj --configuration Release
```
Expected: Error about `PromptTemplate` still referenced in the `.razor` template — fix next.

#### Step B: Update the Razor template

- [ ] **Step 4: Replace the `.razor` template**

Replace the entire content of `AiSettingsDialog.razor` with:

```razor
<MudDialog Class="apptrack-dialog">
    <TitleContent>
        <MudText Typo="Typo.h6">
            <MudIcon Icon="@Icons.Material.Filled.SmartToy" Class="mr-2" />
            AI Settings
        </MudText>
    </TitleContent>
    <DialogContent>
        @if (_isLoading)
        {
            <MudStack AlignItems="AlignItems.Center" Class="py-8">
                <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
                <MudText Typo="Typo.body2" Color="Color.Secondary">Loading settings…</MudText>
            </MudStack>
        }
        else
        {
            <MudGrid Spacing="2">
                <!-- Chat Model -->
                <MudItem xs="12" sm="6">
                    <MudSelect T="ChatModel"
                               Label="Chat Model"
                               Value="_selectedChatModel"
                               ValueChanged="@OnChatModelChanged"
                               ToStringFunc="@(m => m?.Name ?? string.Empty)"
                               Variant="Variant.Outlined"
                               AdornmentIcon="@Icons.Material.Outlined.Psychology"
                               Class="mb-1">
                        @foreach (var model in _chatModels)
                        {
                            <MudSelectItem T="ChatModel" Value="@model">
                                <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="1">
                                    <MudText>@model.Name</MudText>
                                    @if (!string.IsNullOrWhiteSpace(model.Description))
                                    {
                                        <MudText Typo="Typo.caption" Color="Color.Secondary">— @model.Description</MudText>
                                    }
                                </MudStack>
                            </MudSelectItem>
                        }
                    </MudSelect>
                </MudItem>
                <!-- Prompts -->
                <MudItem xs="12">
                    <MudStack Row="true" AlignItems="AlignItems.Center" Class="mb-2">
                        <MudText Typo="Typo.subtitle1">Prompts</MudText>
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
                <!-- Prompt Parameters -->
                <MudItem xs="12">
                    <MudStack Row="true" AlignItems="AlignItems.Center" Class="mb-2">
                        <MudText Typo="Typo.subtitle1">Prompt Parameters</MudText>
                        <MudSpacer />
                        <MudButton Variant="Variant.Outlined"
                                   Color="Color.Primary"
                                   StartIcon="@Icons.Material.Filled.Add"
                                   Size="Size.Small"
                                   OnClick="AddPromptParameterAsync">
                            Add
                        </MudButton>
                    </MudStack>
                    @if (_model.PromptParameter.Count == 0)
                    {
                        <MudText Typo="Typo.body2" Color="Color.Secondary" Class="ml-1">
                            No prompt parameters defined.
                        </MudText>
                    }
                    else
                    {
                        <MudPaper Outlined="true" Class="pa-0" Style="max-height: 260px; overflow-y: auto;">
                            @foreach (var param in _model.PromptParameter)
                            {
                                var captured = param;
                                <MudStack Row="true"
                                          AlignItems="AlignItems.Center"
                                          Class="pa-3"
                                          Style="border-bottom: 1px solid var(--mud-palette-divider);">
                                    <MudStack Spacing="0" Style="flex: 1; min-width: 0;">
                                        <MudText Typo="Typo.subtitle2" Style="word-break: break-word;">@captured.Key</MudText>
                                        <MudText Typo="Typo.body2"
                                                 Color="Color.Secondary"
                                                 Style="word-break: break-word; white-space: pre-wrap;">@captured.Value</MudText>
                                    </MudStack>
                                    <MudStack Row="true" Spacing="0">
                                        <MudTooltip Text="Edit">
                                            <MudIconButton Icon="@Icons.Material.Filled.Edit"
                                                           Size="Size.Small"
                                                           Color="Color.Primary"
                                                           OnClick="@(() => EditPromptParameterAsync(captured))" />
                                        </MudTooltip>
                                        <MudTooltip Text="Delete">
                                            <MudIconButton Icon="@Icons.Material.Filled.Delete"
                                                           Size="Size.Small"
                                                           Color="Color.Error"
                                                           OnClick="@(() => DeletePromptParameter(captured))" />
                                        </MudTooltip>
                                    </MudStack>
                                </MudStack>
                            }
                        </MudPaper>
                    }
                </MudItem>
            </MudGrid>
        }
    </DialogContent>
    <DialogActions>
        <MudButton Variant="Variant.Text" OnClick="Cancel" Disabled="_isBusy || _isLoading">Cancel</MudButton>
        <MudSpacer />
        <MudButton Variant="Variant.Filled"
                   Color="Color.Primary"
                   StartIcon="@Icons.Material.Filled.Save"
                   OnClick="SubmitAsync"
                   Disabled="_isBusy || _isLoading">
            @if (_isBusy)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
                <span>Saving…</span>
            }
            else
            {
                <span>Save</span>
            }
        </MudButton>
    </DialogActions>
</MudDialog>
```

- [ ] **Step 5: Build**

```bash
dotnet build AppTrack.BlazorUi/AppTrack.BlazorUi.csproj --configuration Release
```
Expected: Error about `PromptDialog` not existing yet — OK, fix in Task 24.

- [ ] **Step 6: Commit**

```bash
git add AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor.cs
git commit -m "feat: update AiSettingsDialog with Prompts section"
```

---

### Task 24: Create `PromptDialog`

**Files:**
- Create: `AppTrack.BlazorUi/Components/Dialogs/PromptDialog.razor`
- Create: `AppTrack.BlazorUi/Components/Dialogs/PromptDialog.razor.cs`

- [ ] **Step 1: Create `PromptDialog.razor.cs`**

```csharp
// AppTrack.BlazorUi/Components/Dialogs/PromptDialog.razor.cs
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class PromptDialog
{
    [Inject] private IModelValidator<PromptModel> ModelValidator { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    /// <summary>
    /// When set, the dialog operates in edit mode for an existing prompt.
    /// When null, the dialog creates a new prompt.
    /// </summary>
    [Parameter] public PromptModel? ExistingPrompt { get; set; }

    /// <summary>
    /// The sibling collection used for uniqueness validation.
    /// </summary>
    [Parameter] public IEnumerable<PromptModel>? SiblingPrompts { get; set; }

    private PromptModel _model = new();
    private bool _isEdit;

    protected override void OnParametersSet()
    {
        _isEdit = ExistingPrompt is not null;

        if (_isEdit)
        {
            _model = new PromptModel
            {
                Id = ExistingPrompt!.Id,
                Name = ExistingPrompt.Name,
                PromptTemplate = ExistingPrompt.PromptTemplate,
                TempId = ExistingPrompt.TempId,
                CreationDate = ExistingPrompt.CreationDate,
                ModifiedDate = ExistingPrompt.ModifiedDate,
            };
        }
        else
        {
            _model = new PromptModel();
        }

        // Wire sibling collection for uniqueness validation
        _model.SiblingPrompts = SiblingPrompts;
    }

    private void OnNameChanged(string value)
    {
        _model.Name = value;
        ModelValidator.ResetErrors(nameof(PromptModel.Name));
    }

    private void OnPromptTemplateChanged(string value)
    {
        _model.PromptTemplate = value;
        ModelValidator.ResetErrors(nameof(PromptModel.PromptTemplate));
    }

    private string GetFirstError(string propertyName)
        => ModelValidator.Errors.GetValueOrDefault(propertyName)?.FirstOrDefault() ?? string.Empty;

    private void Submit()
    {
        if (!ModelValidator.Validate(_model)) return;
        MudDialog.Close(DialogResult.Ok(_model));
    }

    private void Cancel() => MudDialog.Cancel();
}
```

- [ ] **Step 2: Create `PromptDialog.razor`**

```razor
@* AppTrack.BlazorUi/Components/Dialogs/PromptDialog.razor *@
<MudDialog Class="apptrack-dialog">
    <TitleContent>
        <MudText Typo="Typo.h6">
            <MudIcon Icon="@Icons.Material.Filled.Description" Class="mr-2" />
            @(_isEdit ? "Edit Prompt" : "Add Prompt")
        </MudText>
    </TitleContent>
    <DialogContent>
        <MudGrid Spacing="2">
            <!-- Name -->
            <MudItem xs="12">
                <MudTextField T="string"
                              Label="Name"
                              Value="_model.Name"
                              ValueChanged="@((string v) => OnNameChanged(v))"
                              Error="@ModelValidator.Errors.ContainsKey(nameof(PromptModel.Name))"
                              ErrorText="@GetFirstError(nameof(PromptModel.Name))"
                              Variant="Variant.Outlined"
                              Adornment="Adornment.Start"
                              AdornmentIcon="@Icons.Material.Outlined.Label"
                              Class="mb-1" />
            </MudItem>
            <!-- Prompt Template -->
            <MudItem xs="12">
                <MudTextField T="string"
                              Label="Prompt Template"
                              Value="_model.PromptTemplate"
                              ValueChanged="@((string v) => OnPromptTemplateChanged(v))"
                              Error="@ModelValidator.Errors.ContainsKey(nameof(PromptModel.PromptTemplate))"
                              ErrorText="@GetFirstError(nameof(PromptModel.PromptTemplate))"
                              Variant="Variant.Outlined"
                              Lines="8"
                              Class="mb-1" />
            </MudItem>
        </MudGrid>
    </DialogContent>
    <DialogActions>
        <MudButton Variant="Variant.Text" OnClick="Cancel">Cancel</MudButton>
        <MudSpacer />
        <MudButton Variant="Variant.Filled"
                   Color="Color.Primary"
                   StartIcon="@Icons.Material.Filled.Check"
                   OnClick="Submit">
            OK
        </MudButton>
    </DialogActions>
</MudDialog>
```

- [ ] **Step 3: Build the full solution**

```bash
dotnet build AppTrack.sln --configuration Release
```
Expected: **Build succeeded, 0 errors, 0 warnings.** This is the final verification that the entire stack compiles cleanly.

- [ ] **Step 4: Run all unit tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```
Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add AppTrack.BlazorUi/Components/Dialogs/PromptDialog.razor AppTrack.BlazorUi/Components/Dialogs/PromptDialog.razor.cs
git commit -m "feat: add PromptDialog component"
```

---

## Final Verification

- [ ] **Step 1: Run the full solution (API + Blazor)**

Terminal 1:
```bash
dotnet run --project AppTrack.Api/AppTrack.Api.csproj --launch-profile https
```

Terminal 2:
```bash
dotnet run --project AppTrack.BlazorUi/AppTrack.BlazorUi.csproj --launch-profile https
```

- [ ] **Step 2: Manual smoke test**

1. Open the Blazor app and log in
2. Open AI Settings dialog
3. Verify: no `Prompt Template` text field visible
4. Verify: "Prompts" section with "Add" button is present
5. Click "Add" — `PromptDialog` should open
6. Enter a name and template → OK
7. Prompt appears in the list
8. Edit the prompt → changes saved
9. Add a second prompt with the same name → should show validation error
10. Delete a prompt
11. Save settings → API call succeeds
12. Reopen dialog → prompts are loaded from DB

- [ ] **Step 3: Final commit if any small fixes were needed during smoke test**
