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

    public static DefaultPrompt Create(string name, string promptTemplate, string language)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(promptTemplate);
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

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
- `HasData()` seeds the initial German prompts (exact names and templates to be defined in the implementation plan)

**File:** `AppTrack.Persistance/AppTrackDbContext.cs`
- Add `DbSet<DefaultPrompt> DefaultPrompts`

**EF Core migration** added for the new table and seed data.

### 3. Persistence — `IDefaultPromptRepository` + `DefaultPromptRepository`

**File:** `AppTrack.Application/Contracts/Persistance/IDefaultPromptRepository.cs`

```csharp
public interface IDefaultPromptRepository : IGenericRepository<DefaultPrompt>
{
    Task<List<DefaultPrompt>> GetAllAsync();
    Task<List<DefaultPrompt>> GetByLanguageAsync(string language);
}
```

**File:** `AppTrack.Persistance/Repositories/DefaultPromptRepository.cs`
- `GetAllAsync()`: returns all default prompts, `AsNoTracking`
- `GetByLanguageAsync(string language)`: filters by `Language`, `AsNoTracking`

**File:** `AppTrack.Persistance/PersistenceServiceRegistration.cs`
- Register `IDefaultPromptRepository` → `DefaultPromptRepository`

### 4. Application — `AiSettingsDto`

**File:** `AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs`

Add to the existing DTO:
```csharp
public List<PromptDto> DefaultPrompts { get; set; } = [];
```

`PromptDto` is reused — no new DTO type needed. `DefaultPrompts` is populated from `DefaultPrompt` entities (Name + PromptTemplate fields only; `AiSettingsId` is irrelevant and left as `0`).

### 5. Application — `GetAiSettingsByUserIdQueryHandler`

**File:** `AppTrack.Application/Features/AiSettings/Queries/GetAiSettingsByUserIdQuery/GetAiSettingsByUserIdQueryHandler.cs`

- Inject `IDefaultPromptRepository`
- After loading/creating user `AiSettings`, call `GetAllAsync()` (no language filter until user profile exists)
- Map results to `List<PromptDto>` and assign to `AiSettingsDto.DefaultPrompts`

### 6. Application — `PromptNamesQueryHandler`

**File:** `AppTrack.Application/Features/ApplicationText/Query/PromptNamesQuery/PromptNamesQueryHandler.cs`

- Inject `IDefaultPromptRepository`
- Call `GetAllAsync()`, extract names
- Return user prompt names first, then default prompt names (merged, no duplicates — user prompt name takes precedence if same name exists in both)

### 7. Application — `GeneratePromptQueryValidator`

**File:** `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryValidator.cs`

- Inject `IDefaultPromptRepository`
- Existing rule validates that the named prompt exists in `AiSettings.Prompts`
- Extend: if not found in user prompts, also check default prompts
- Validation fails only if the name is absent from both

### 8. Application — `GeneratePromptQueryHandler`

**File:** `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryHandler.cs`

- After loading `AiSettings`, look up the prompt template:
  1. Check `AiSettings.Prompts` first (user prompt takes precedence)
  2. If not found, fetch from `IDefaultPromptRepository.GetAllAsync()` and look up by name
- Use the resolved template to generate the text (unchanged logic after template is found)

### 9. NSwag — Regenerate API Client

The `AiSettingsDto` gains `DefaultPrompts`. After backend changes, regenerate `AppTrack.BlazorUi/ApiService/Base/ServiceClient.cs` via NSwag so the Blazor client receives the new field.

### 10. Blazor UI — `AiSettings.razor` / `AiSettings.razor.cs`

**File:** `AppTrack.BlazorUi/Components/Pages/AiSettings.razor`

The Prompts section is split into two sub-sections:

**Default Prompts (read-only):**
- Header: "Default Prompts" with a lock icon (`Icons.Material.Filled.Lock`)
- List of `_aiSettings.DefaultPrompts`, each row shows Name + PromptTemplate
- No Add / Edit / Delete buttons

**My Prompts (editable):**
- Existing behaviour unchanged — Add / Edit / Delete buttons remain

**File:** `AppTrack.BlazorUi/Components/Pages/AiSettings.razor.cs`
- No logic changes beyond the DTO now providing `DefaultPrompts` — display only

### 11. Blazor UI — `GenerateTextDialog`

**File:** `AppTrack.BlazorUi/Components/Dialogs/GenerateTextDialog.razor.cs`

The `PromptNames` parameter already receives a `List<string>` from `Home.razor.cs`. The caller (`Home.razor.cs`) now gets prompt names from `PromptNamesQueryHandler` which already merges user + default names. No changes to the dialog itself.

**File:** `AppTrack.BlazorUi/Components/Pages/Home.razor.cs`
- No changes needed (already calls `ApplicationTextService.GetPromptNames()` which will now return merged names)

## Out of Scope

- Language selection UI
- User profile / language preference storage
- Filtering default prompts by user's language (deferred until user profile exists)
- WPF frontend changes
- Admin UI to manage default prompts
- Allowing users to create personal copies of default prompts
