# Default Prompt Prefix Enforcement — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Enforce a `Default_` prefix on all default prompts and prevent user prompts from using this prefix or spaces in their names.

**Architecture:** Add two rules to the shared `PromptBaseValidator` (no spaces, no `Default_` prefix) so they propagate to all inheriting validators automatically. Add guards in `BuiltInPrompt.Create()` for domain-level enforcement. Rename the four seeded built-in prompts to English `Default_`-prefixed names via EF Core data migration.

**Tech Stack:** .NET 10, FluentValidation 12, EF Core 10, xUnit, Shouldly, EF Core InMemory (integration tests)

**Spec:** `docs/superpowers/specs/2026-04-04-default-prompt-prefix-design.md`

---

## Chunk 1: Shared validator rules

### Task 1: Add rules to `PromptBaseValidator` (TDD)

**Files:**
- Modify: `AppTrack.Shared.Validation/Validators/PromptBaseValidator.cs`
- Modify: `AppTrack.Application.UnitTests/Validators/PromptBaseValidatorTests.cs`
- Modify: `AppTrack.Application.UnitTests/Validators/AiSettingsBaseValidatorTests.cs`
- Modify: `AppTrack.Application.UnitTests/Features/AiSettings/Commands/PromptDtoValidatorTests.cs`

- [ ] **Step 1: Write the two new failing tests in `PromptBaseValidatorTests.cs`**

Add these two test methods at the end of the `PromptBaseValidatorTests` class:

```csharp
[Fact]
public async Task NameWithSpace_ShouldFail()
{
    var result = await _validator.ValidateAsync(new TestPrompt { Name = "My Prompt", PromptTemplate = "Hello" });
    result.IsValid.ShouldBeFalse();
    result.Errors.ShouldContain(e => e.PropertyName == "Name" && e.ErrorMessage == "A prompt name must not contain spaces.");
}

[Fact]
public async Task NameStartingWithDefaultPrefix_ShouldFail()
{
    var result = await _validator.ValidateAsync(new TestPrompt { Name = "Default_Something", PromptTemplate = "Hello" });
    result.IsValid.ShouldBeFalse();
    result.Errors.ShouldContain(e => e.PropertyName == "Name" && e.ErrorMessage == "A prompt name must not start with 'Default_'.");
}
```

- [ ] **Step 2: Run the new tests to verify they fail**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~PromptBaseValidatorTests.NameWithSpace_ShouldFail|FullyQualifiedName~PromptBaseValidatorTests.NameStartingWithDefaultPrefix_ShouldFail" --configuration Release
```

Expected: both FAIL — the rules don't exist yet.

- [ ] **Step 3: Add the two new rules to `PromptBaseValidator.cs`**

Current file (`AppTrack.Shared.Validation/Validators/PromptBaseValidator.cs`):

```csharp
RuleFor(x => x.Name)
    .NotEmpty().WithMessage("Name is required.")
    .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
```

Replace the `RuleFor(x => x.Name)` block with:

```csharp
RuleFor(x => x.Name)
    .NotEmpty().WithMessage("Name is required.")
    .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
    .Must(n => !n.Contains(' ')).WithMessage("A prompt name must not contain spaces.")
    .Must(n => !n.StartsWith("Default_", StringComparison.OrdinalIgnoreCase))
        .WithMessage("A prompt name must not start with 'Default_'.");
```

- [ ] **Step 4: Run the new tests to verify they pass**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~PromptBaseValidatorTests.NameWithSpace_ShouldFail|FullyQualifiedName~PromptBaseValidatorTests.NameStartingWithDefaultPrefix_ShouldFail" --configuration Release
```

Expected: both PASS.

- [ ] **Step 5: Fix now-broken fixtures in `PromptBaseValidatorTests.cs`**

The existing tests `ValidPrompt_ShouldPass` and `EmptyPromptTemplate_ShouldFail` both use `"My Prompt"` (has a space) and will now fail. The two new tests added in Step 1 also use `"My Prompt"` — but intentionally as an invalid input, so do NOT rename those.

In `PromptBaseValidatorTests.cs`, rename `"My Prompt"` → `"My_Prompt"` **only** in `ValidPrompt_ShouldPass` and `EmptyPromptTemplate_ShouldFail`.

- [ ] **Step 6: Fix now-broken fixture in `AiSettingsBaseValidatorTests.cs`**

The `ValidPrompts_ShouldPass` test uses `"Prompt A"` and `"Prompt B"` (both have spaces).

In `AiSettingsBaseValidatorTests.cs`, in the `ValidPrompts_ShouldPass` method, replace:
```csharp
new TestPromptItem { Name = "Prompt A", PromptTemplate = "Hello" },
new TestPromptItem { Name = "Prompt B", PromptTemplate = "World" },
```
with:
```csharp
new TestPromptItem { Name = "Prompt_A", PromptTemplate = "Hello" },
new TestPromptItem { Name = "Prompt_B", PromptTemplate = "World" },
```

- [ ] **Step 7: Fix now-broken fixture in `PromptDtoValidatorTests.cs`**

`BuildValidDto()` returns `Name = "Cover Letter"` (has a space). This breaks every test in the file.

In `PromptDtoValidatorTests.cs`, in `BuildValidDto()`, replace:
```csharp
Name = "Cover Letter",
```
with:
```csharp
Name = "Cover_Letter",
```

Also add the two new test methods at the end of the class:

```csharp
[Fact]
public void Validate_ShouldHaveError_WhenNameContainsSpace()
{
    var dto = BuildValidDto();
    dto.Name = "Cover Letter";
    _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name)
        .WithErrorMessage("A prompt name must not contain spaces.");
}

[Fact]
public void Validate_ShouldHaveError_WhenNameStartsWithDefaultPrefix()
{
    var dto = BuildValidDto();
    dto.Name = "Default_Cover_Letter";
    _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name)
        .WithErrorMessage("A prompt name must not start with 'Default_'.");
}
```

- [ ] **Step 8: Run all unit tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

Expected: all tests PASS, 0 failures.

- [ ] **Step 9: Commit**

```bash
git add AppTrack.Shared.Validation/Validators/PromptBaseValidator.cs
git add AppTrack.Application.UnitTests/Validators/PromptBaseValidatorTests.cs
git add AppTrack.Application.UnitTests/Validators/AiSettingsBaseValidatorTests.cs
git add "AppTrack.Application.UnitTests/Features/AiSettings/Commands/PromptDtoValidatorTests.cs"
git commit -m "feat: block spaces and Default_ prefix in user prompt names"
```

---

## Chunk 2: Domain guards + seed rename + migration (atomic)

> These three changes **must land in the same commit**. Adding guards to `BuiltInPrompt.Create()` before renaming the seed entries will break the build because the existing seed names (`"Anschreiben"` etc.) violate the guards. The strategy is: write the failing tests first, then implement all three changes together.

### Task 2: Domain guards, seed rename, and EF migration (atomic)

**Files:**
- Modify: `AppTrack.Domain/BuiltInPrompt.cs`
- Modify: `AppTrack.Application.UnitTests/Domain/BuiltInPromptFactoryTests.cs`
- Modify: `AppTrack.Persistance/Configurations/BuiltInPromptConfiguration.cs`
- Create: new EF Core migration (auto-generated name, e.g. `..._RenameDefaultPrompts`)

- [ ] **Step 1: Write the two new failing guard tests in `BuiltInPromptFactoryTests.cs`**

Add at the end of the `BuiltInPromptFactoryTests` class:

```csharp
[Fact]
public void Create_ShouldThrowArgumentException_WhenNameDoesNotStartWithDefaultPrefix()
{
    Should.Throw<ArgumentException>(() => BuiltInPrompt.Create("Cover_Letter", "template", "de"));
}

[Fact]
public void Create_ShouldThrowArgumentException_WhenNameContainsSpace()
{
    Should.Throw<ArgumentException>(() => BuiltInPrompt.Create("Default_Cover Letter", "template", "de"));
}
```

- [ ] **Step 2: Run the new tests to verify they fail**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~BuiltInPromptFactoryTests.Create_ShouldThrowArgumentException_WhenNameDoesNotStartWithDefaultPrefix|FullyQualifiedName~BuiltInPromptFactoryTests.Create_ShouldThrowArgumentException_WhenNameContainsSpace" --configuration Release
```

Expected: both FAIL — no guards exist yet.

- [ ] **Step 3: Add guards to `BuiltInPrompt.Create()` in `AppTrack.Domain/BuiltInPrompt.cs`**

The current method body:

```csharp
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
```

Replace with:

```csharp
public static BuiltInPrompt Create(string? name, string? promptTemplate, string? language)
{
    ArgumentNullException.ThrowIfNull(name);
    ArgumentNullException.ThrowIfNull(promptTemplate);
    ArgumentNullException.ThrowIfNull(language);

    // Seeder code runs outside the FluentValidation pipeline, so guards are the
    // only domain-level enforcement for these invariants.
    if (!name.StartsWith("Default_", StringComparison.Ordinal))
        throw new ArgumentException("Default prompt names must start with 'Default_'.", nameof(name));

    if (name.Contains(' '))
        throw new ArgumentException("Default prompt names must not contain spaces.", nameof(name));

    return new BuiltInPrompt
    {
        Name = name,
        PromptTemplate = promptTemplate,
        Language = language
    };
}
```

- [ ] **Step 4: Rename seed entries in `BuiltInPromptConfiguration.cs`**

Replace the four `Seed(...)` calls inside `Configure`:

```csharp
builder.HasData(
    Seed(1, "Default_Cover_Letter",
        "Write a professional cover letter for the {Position} position at {Company}. Job description: {JobDescription}"),
    Seed(2, "Default_LinkedIn_Message",
        "Write a short LinkedIn message to {ContactPerson} regarding the {Position} position at {Company}."),
    Seed(3, "Default_Introduction",
        "Introduce me in a few sentences as an applicant for the {Position} position at {Company}."),
    Seed(4, "Default_Follow_Up",
        "Write a short follow-up email to {ContactPerson} regarding my application for the {Position} position at {Company}.")
);
```

Note: prompt templates are also translated to English to be consistent with the English names. The `language` field in the `Seed()` helper remains `"de"` — the language field tracks which locale these prompts are intended for, not the language of the template text. No change is needed here.

- [ ] **Step 5: Update the three existing passing tests in `BuiltInPromptFactoryTests.cs`**

**`Create_ShouldReturnBuiltInPrompt_WhenAllArgumentsAreValid`** — update both the `Create(...)` argument and the `.ShouldBe(...)` assertion:

```csharp
// Before:
var result = BuiltInPrompt.Create("Anschreiben", "Template text", "de");
result.Name.ShouldBe("Anschreiben");

// After:
var result = BuiltInPrompt.Create("Default_Cover_Letter", "Template text", "de");
result.Name.ShouldBe("Default_Cover_Letter");
```

**`Create_ShouldThrowArgumentNullException_WhenPromptTemplateIsNull`** and **`Create_ShouldThrowArgumentNullException_WhenLanguageIsNull`** — both pass `"Name"` as the first argument, which will now throw `ArgumentException` (prefix guard) before ever reaching the null checks, breaking the tests. Update both to use a valid prefixed name:

```csharp
// Before (both tests):
Should.Throw<ArgumentNullException>(() => BuiltInPrompt.Create("Name", null, "de"));
Should.Throw<ArgumentNullException>(() => BuiltInPrompt.Create("Name", "template", null));

// After (both tests):
Should.Throw<ArgumentNullException>(() => BuiltInPrompt.Create("Default_Name", null, "de"));
Should.Throw<ArgumentNullException>(() => BuiltInPrompt.Create("Default_Name", "template", null));
```

- [ ] **Step 6: Generate the EF Core data migration**

Run from the solution root:

```bash
dotnet ef migrations add RenameDefaultPrompts --project AppTrack.Persistance/AppTrack.Persistance.csproj --startup-project AppTrack.Api/AppTrack.Api.csproj
```

This generates a migration that produces `UpdateData` calls for the four renamed rows. Verify the generated migration file contains `UpdateData` calls (not `DeleteData`/`InsertData`) for the `DefaultPrompts` table.

- [ ] **Step 7: Run all unit tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

Expected: all tests PASS.

- [ ] **Step 8: Commit atomically**

```bash
git add AppTrack.Domain/BuiltInPrompt.cs
git add AppTrack.Application.UnitTests/Domain/BuiltInPromptFactoryTests.cs
git add AppTrack.Persistance/Configurations/BuiltInPromptConfiguration.cs
git add AppTrack.Persistance/Migrations/
git commit -m "feat: add Default_ prefix guards to BuiltInPrompt.Create, rename seed data, add migration"
```

---

## Chunk 3: Persistence init tests

### Task 3: Add `BuiltInPromptSeedTests`

**Files:**
- Create: `AppTrack.Persistance.IntegrationTests/BuiltInPromptSeedTests.cs`

- [ ] **Step 1: Create the test class**

```csharp
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AppTrack.Persistance.IntegrationTests;

public class BuiltInPromptSeedTests
{
    private readonly AppTrackDatabaseContext _context;

    public BuiltInPromptSeedTests()
    {
        var options = new DbContextOptionsBuilder<AppTrackDatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppTrackDatabaseContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task AllBuiltInPrompts_ShouldStartWithDefaultPrefix()
    {
        var prompts = await _context.BuiltInPrompts.ToListAsync();

        prompts.ShouldNotBeEmpty();
        prompts.ShouldAllBe(p => p.Name.StartsWith("Default_", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AllBuiltInPrompts_ShouldNotContainSpaces()
    {
        var prompts = await _context.BuiltInPrompts.ToListAsync();

        prompts.ShouldNotBeEmpty();
        prompts.ShouldAllBe(p => !p.Name.Contains(' '));
    }
}
```

> **Note:** `UseInMemoryDatabase` + `EnsureCreated()` applies the `HasData` seed entries. The tests verify that the seed data in `BuiltInPromptConfiguration.HasData()` follows the naming convention. This is the safety net for future additions.

- [ ] **Step 2: Run the persistence integration tests**

```bash
dotnet test AppTrack.Persistance.IntegrationTests/AppTrack.Persistance.IntegrationTests.csproj --configuration Release
```

Expected: all tests PASS including the two new ones.

- [ ] **Step 3: Run the full solution build to confirm no regressions**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: 0 errors, 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Persistance.IntegrationTests/BuiltInPromptSeedTests.cs
git commit -m "test: add BuiltInPromptSeedTests to enforce Default_ prefix on seed data"
```
