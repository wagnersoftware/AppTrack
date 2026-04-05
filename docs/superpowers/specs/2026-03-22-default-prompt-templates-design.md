# Default Prompt Templates — Design Spec

**Date:** 2026-03-22
**Scope:** Backend (Domain, Persistence, Application) + Blazor frontend (`AppTrack.BlazorUi`). No WPF changes.

## Overview

Introduce global, read-only `DefaultPrompt` templates that are available to every user without being copied into their personal `AiSettings`. Default prompts are stored in a dedicated DB table, seeded via EF Core `HasData()`, and returned alongside user prompts in existing queries. The data model is prepared for future multi-language support via a `Language` column; for now only German (`"de"`) prompts are seeded. Language filtering will be applied once a user profile with language preference is implemented.

## Changes

### 1. New Domain Entity — `DefaultPrompt`

**File:** `AppTrack.Domain/DefaultPrompt.cs`

```csharp
public class DefaultPrompt : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string PromptTemplate { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty; // ISO 639-1, e.g. "de", "en"

    private DefaultPrompt() { }

    public static DefaultPrompt Create(string? name, string? promptTemplate, string? language)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(promptTemplate);
        ArgumentNullException.ThrowIfNull(language);

        return new DefaultPrompt
        {
            Name = name,
            PromptTemplate = promptTemplate,
            Language = language
        };
    }
}
```

No relationship to `AiSettings` — this is a standalone global entity.

### 2. Persistence — `DefaultPromptConfiguration`

**File:** `AppTrack.Persistance/Configurations/DefaultPromptConfiguration.cs`

- Unique constraint on `(Name, Language)` — no duplicate prompt name per language
- `Name`: max length 100
- `HasData()` seeds the initial German prompts with **explicit, hard-coded `Id` values** (e.g., `Id = 1`, `Id = 2`, …). EF Core `HasData` requires all primary key values to be set explicitly in the migration snapshot — the `SaveChanges`-based `CreationDate`/`ModifiedDate` auto-assignment is bypassed, so those columns will be `null` for seed rows. This is acceptable because `BaseEntity.CreationDate` and `ModifiedDate` are both `DateTime?`.
- The specific German prompt names and templates (e.g., cover letter, introduction) are defined in the implementation plan.

**File:** `AppTrack.Persistance/AppTrackDbContext.cs`
- Add `DbSet<DefaultPrompt> DefaultPrompts`

**EF Core migration** added for the new table and seed data.

### 3. Persistence — `IDefaultPromptRepository` + `DefaultPromptRepository`

**File:** `AppTrack.Application/Contracts/Persistance/IDefaultPromptRepository.cs`

```csharp
public interface IDefaultPromptRepository : IGenericRepository<DefaultPrompt>
{
    Task<IReadOnlyList<DefaultPrompt>> GetByLanguageAsync(string language);
}
```

`GetAllAsync()` is **not** added — the inherited `GetAsync()` from `IGenericRepository<T>` is used everywhere default prompts need to be fetched without a language filter.

**File:** `AppTrack.Persistance/Repositories/DefaultPromptRepository.cs`
- `GetByLanguageAsync(string language)`: filters by `Language`, `AsNoTracking`

**File:** `AppTrack.Persistance/PersistanceServiceRegistration.cs` *(note: matches project's own spelling)*
- Register `IDefaultPromptRepository` → `DefaultPromptRepository`

### 4. Application — `AiSettingsDto`

**File:** `AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs`

Add to the existing DTO:
```csharp
public List<PromptDto> DefaultPrompts { get; set; } = [];
```

`PromptDto` (`Id`, `Name`, `PromptTemplate`) is reused — no new DTO type needed. The mapping sets `Id = entity.Id`, `Name = entity.Name`, `PromptTemplate = entity.PromptTemplate`.

### 5. Application — `GetAiSettingsByUserIdQueryHandler`

**File:** `AppTrack.Application/Features/AiSettings/Queries/GetAiSettingsByUserIdQuery/GetAiSettingsByUserIdQueryHandler.cs`

- Inject `IDefaultPromptRepository`
- After loading/creating user `AiSettings`, call `GetAsync()` (no language filter until user profile exists)
- Map results to `List<PromptDto>` and assign to `AiSettingsDto.DefaultPrompts`

### 6. Application — `PromptNamesQueryHandler`

**File:** `AppTrack.Application/Features/ApplicationText/Query/PromptNamesQuery/PromptNamesQueryHandler.cs`

- Inject `IDefaultPromptRepository`
- Call `GetAsync()`, extract names
- Build final list: user prompt names first, then any default prompt names not already present, using `StringComparer.OrdinalIgnoreCase` for deduplication (user prompt takes precedence on case-insensitive name collision)

### 7. Application — `GeneratePromptQueryValidator`

**File:** `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryValidator.cs`

- Inject `IDefaultPromptRepository`
- Existing rule validates that the named prompt exists in `AiSettings.Prompts`
- Extend: if not found in user prompts, also check default prompts via `GetAsync()`
- Validation fails only if the name is absent from both

> **Note:** Both the validator and the handler independently call `GetAsync()` for default prompts (and `GetByUserIdIncludePromptParameterAsync` for user settings). This results in two repository calls each, four total — consistent with the existing pattern for `AiSettings` already present in the codebase. Caching or result-passing is out of scope.

### 8. Application — `GeneratePromptQueryHandler`

**File:** `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryHandler.cs`

- After loading `AiSettings`, look up the prompt template:
  1. Check `AiSettings.Prompts` first (user prompt takes precedence)
  2. If not found, call `_defaultPromptRepository.GetAsync()` and look up by name (`OrdinalIgnoreCase`)
- Use the resolved template to generate the text (unchanged logic after template is found)

### 9. NSwag — Regenerate API Client

The `AiSettingsDto` gains `DefaultPrompts`. After backend changes, regenerate `AppTrack.BlazorUi/ApiService/Base/ServiceClient.cs` via NSwag so the Blazor client receives the new field.

### 10. Frontend Models — `AiSettingsModel`

**File:** `AppTrack.Frontend.Models/Models/AiSettingsModel.cs`

Add a new property for default prompts (display-only, not part of `IAiSettingsValidatable`):
```csharp
public List<PromptModel> DefaultPrompts { get; set; } = [];
```

This property is populated in `AiSettings.razor.cs` from the NSwag-generated `AiSettingsDto.DefaultPrompts` after the response arrives, using the existing `PromptModel` mapping.

### 11. Blazor UI — `AiSettings.razor` / `AiSettings.razor.cs`

**File:** `AppTrack.BlazorUi/Components/Pages/AiSettings.razor`

The Prompts section is split into two sub-sections:

**Default Prompts (read-only):**
- Header: "Default Prompts" with a lock icon (`Icons.Material.Filled.Lock`)
- List of `_model.DefaultPrompts`, each row shows Name + PromptTemplate
- No Add / Edit / Delete buttons

**My Prompts (editable):**
- Existing behaviour unchanged — Add / Edit / Delete buttons remain

**File:** `AppTrack.BlazorUi/Components/Pages/AiSettings.razor.cs`
- After mapping the `AiSettingsDto` response to `_model`, also populate `_model.DefaultPrompts` from `settingsDto.DefaultPrompts`

### 12. Blazor UI — `GenerateTextDialog` / `Home.razor.cs`

No changes needed. `Home.razor.cs` already calls `ApplicationTextService.GetPromptNames()` which maps to `PromptNamesQueryHandler` — that handler will now return the merged list automatically.

## Unit Tests

Per CLAUDE.md, every modified handler and validator requires updated or new test coverage in `AppTrack.Application.UnitTests`:

| File | New test cases |
|------|----------------|
| `DefaultPromptFactoryTests.cs` *(new)* | `Create` passes with valid args; `Create` throws `ArgumentNullException` for null name / template / language |
| `GetAiSettingsByUserIdQueryHandlerTests.cs` | `DefaultPrompts` collection is populated in returned DTO |
| `GetPromptNamesQueryHandlerTests.cs` | Merged list returns user names first; default names appended; duplicates (case-insensitive) deduplicated |
| `GeneratePromptQueryValidatorTests.cs` | Prompt name absent from user prompts but present in defaults → passes validation |
| `GeneratePromptQueryHandlerTests.cs` | Template resolved from default prompts when not found in user prompts |

## Out of Scope

- Language selection UI
- User profile / language preference storage
- Filtering default prompts by user's language (deferred until user profile exists)
- WPF frontend changes
- Admin UI to manage default prompts
- Allowing users to create personal copies of default prompts
