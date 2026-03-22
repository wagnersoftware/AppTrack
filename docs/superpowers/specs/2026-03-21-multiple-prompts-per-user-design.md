# Design: Multiple Prompts per User in AI Settings

**Date:** 2026-03-21
**Status:** Approved
**Scope:** Full stack — Domain, Application, API, Frontend Models, Blazor UI

---

## Overview

Currently each user has exactly one `AiSettings` instance containing a single `PromptTemplate` string. This feature replaces that with a named collection of `Prompt` entities per user, allowing users to manage multiple reusable prompts.

**`PromptParameter` (global key-value pairs) is NOT affected** — it stays on `AiSettings` exactly as today. Only `PromptTemplate` (the single string field) is removed and replaced by the new `Prompts` collection.

Selection of an active prompt is **out of scope** for this iteration and will be implemented as a follow-up step.

The architecture is designed to support global template prompts (available to new users) in a future iteration.

---

## Data Model & Database

### New Entity: `Prompt` (`AppTrack.Domain`)

| Field           | Type       | Constraints                           |
|-----------------|------------|---------------------------------------|
| Id              | int        | PK, Identity                          |
| Name            | string     | required, max 100 chars               |
| PromptTemplate  | string     | required                              |
| AiSettingsId    | int        | FK → AiSettings, CASCADE DELETE       |
| AiSettings      | AiSettings | navigation back-reference (required)  |

Factory: `static Create(string? name, string? promptTemplate)` with private constructor — consistent with `PromptParameter.Create(...)`.

### Changes to `AiSettings`
- Remove field: `PromptTemplate`
- Add navigation: `ICollection<Prompt> Prompts`
- `PromptParameter` navigation remains unchanged

### EF Core Configuration

**`PromptConfiguration : IEntityTypeConfiguration<Prompt>`** — column-level constraints only (consistent with `PromptParameterConfiguration`):
- `Name`: required, max 100
- `PromptTemplate`: required
- Unique index on `(AiSettingsId, Name)`

**`AiSettingsConfiguration`** — relationship configuration belongs here (consistent with existing `PromptParameter` relationship):
```csharp
builder.HasMany(s => s.Prompts)
       .WithOne(p => p.AiSettings)
       .HasForeignKey(p => p.AiSettingsId)
       .OnDelete(DeleteBehavior.Cascade);
```

No `DbSet<Prompt>` on `AppTrackDatabaseContext` — discovered via `ApplyConfigurationsFromAssembly`, consistent with `PromptParameter`.

### Migration
- New table `Prompts`
- Drop column `PromptTemplate` from `AiSettings`

---

## Application Layer (`AppTrack.Application`)

### DTOs
- New `PromptDto` with `Id`, `Name`, `PromptTemplate` — implements `IPromptValidatable`
  (`AppTrack.Application` already references `AppTrack.Shared.Validation`, as confirmed by `PromptParameterDto : IPromptParameterValidatable`)
- `AiSettingsDto`: remove `PromptTemplate`, add `List<PromptDto> Prompts`

### Commands
- `UpdateAiSettingsCommand`: remove `PromptTemplate`, add `List<PromptDto> Prompts`
- New explicit interface implementation:
  ```csharp
  IEnumerable<IPromptValidatable> IAiSettingsValidatable.Prompts => Prompts;
  ```
  Existing `IEnumerable<IPromptParameterValidatable> IAiSettingsValidatable.PromptParameter => PromptParameter;` unchanged.
- Handler: clears + recreates `Prompts` on save (same pattern as `PromptParameter`)

### Validators (Application layer)
- New `PromptDtoValidator : PromptBaseValidator<PromptDto>`
  - File: `AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/PromptDtoValidator.cs`
- `UpdateAiSettingsCommandValidator`: no additional rules needed — inherits Prompts rules from `AiSettingsBaseValidator<T>` via the base class, same as `AiSettingsModelValidator`

### Repository (`IAiSettingsRepository`)
Update both methods to eagerly load `Prompts`. Method names unchanged (still technically correct):
- `GetByIdWithPromptParameterAsync`: add `.Include(s => s.Prompts)`
- `GetByUserIdIncludePromptParameterAsync`: add `.Include(s => s.Prompts)`

### Interim handler (`GeneratePromptQueryHandler`)

`GeneratePromptQueryHandler` reads `aiSettings.PromptTemplate` directly. Replace with:
```csharp
var promptTemplate = aiSettings.Prompts.FirstOrDefault()?.PromptTemplate ?? string.Empty;
```
If no prompts exist, generation proceeds with an empty template. This is replaced by explicit prompt selection in the next iteration.

(`GenerateApplicationTextCommandHandler` does NOT access `PromptTemplate` — it uses `request.Prompt` directly and requires no change.)

### Auto-created `AiSettings`
`GetAiSettingsByUserIdQueryHandler` auto-creates a new `AiSettings` if none exists for the user. After the migration, auto-created records will have `Prompts = []` (empty collection). No seed data or default prompt is needed — an empty collection is valid.

### Mappings (`AppTrack.Application/Mappings/AiSettingsMappings.cs`)
- `ApplyTo` extension: add clear + recreate `Prompts` using `Prompt.Create(...)` (analogous to existing `PromptParameter` block)
- `ToDto` on `AiSettings`: add `Prompts = entity.Prompts.Select(p => p.ToDto()).ToList()`
- New `ToDto` extension on `Prompt`

### NSwag Client Regeneration
Removing `PromptTemplate` from `AiSettingsDto` changes the API contract. After implementing the backend changes, regenerate `AppTrack.Frontend.ApiService/Base/ServiceClient.cs` before updating the frontend mappings. All references to `dto.PromptTemplate` / `model.PromptTemplate` in `ApiService/Mappings/AiSettingsMappings.cs` must be removed as part of this step.

---

## Shared Validation (`AppTrack.Shared.Validation`)

### New `IPromptValidatable`
```csharp
public interface IPromptValidatable
{
    string Name { get; }
    string PromptTemplate { get; }
}
```

### New `PromptBaseValidator<T> where T : IPromptValidatable`
- `Name`: required, max 100 characters
- `PromptTemplate`: required

### `IAiSettingsValidatable` — additive change only
Add new property. **`PromptParameter` member stays unchanged**:
```csharp
IEnumerable<IPromptValidatable> Prompts { get; }
```

### `AiSettingsBaseValidator<T>` — additive change only
Keep all existing `PromptParameter` rules. Add:
- `RuleForEach(x => x.Prompts)` via sealed inner `PromptItemValidator : PromptBaseValidator<IPromptValidatable>` (analogous to `PromptParameterItemValidator`)
- Uniqueness rule: `Name` must be unique within the collection (case-insensitive)

---

## API Layer (`AppTrack.Api`)
No new endpoints. `PUT /api/ai-settings/{id}` extended with `Prompts` list.

---

## Frontend Models (`AppTrack.Frontend.Models`)

### New `PromptModel`
- Inherits `ModelBase`
- `partial class`, `[ObservableProperty]` for `name` (→ `Name`) and `promptTemplate` (→ `PromptTemplate`) — same pattern as `PromptParameterModel`
- `TempId` (Guid, initialized to `Guid.NewGuid()`)
- `IEnumerable<PromptModel>? SiblingPrompts` — set via dialog `[Parameter]`, assigned in `OnParametersSet`, enables sibling uniqueness check
  - Intentional naming deviation from `PromptParameterModel.ParentCollection`: `SiblingPrompts` is clearer and more descriptive; both properties serve the same purpose
- `Clone()` — copies `CreationDate` and `ModifiedDate` from the source instance (same as `PromptParameterModel.Clone()` copies its timestamps; do NOT use `DateTime.MinValue`)
- Implements `IPromptValidatable`

### `AiSettingsModel` — additive change
- Remove `PromptTemplate` property
- Add `ObservableCollection<PromptModel> Prompts`
- New explicit interface implementation:
  ```csharp
  IEnumerable<IPromptValidatable> IAiSettingsValidatable.Prompts => Prompts;
  ```
  Existing `IEnumerable<IPromptParameterValidatable> IAiSettingsValidatable.PromptParameter => PromptParameter;` unchanged.

### `AiSettingsModelValidator`
No additional rules. Inherits updated `AiSettingsBaseValidator<AiSettingsModel>` automatically — same applies to `UpdateAiSettingsCommandValidator` on the backend.

### New `PromptModelValidator`
- Inherits `PromptBaseValidator<PromptModel>`
- Adds sibling uniqueness check on `Name` (case-insensitive, ignores own `TempId`) using `SiblingPrompts`

### Mappings (`AppTrack.Frontend.ApiService/Mappings/AiSettingsMappings.cs`)
After NSwag client regeneration:
- Add `PromptDto` → `PromptModel` mapping
- Add `PromptModel` → `PromptDto` mapping in `ToUpdateCommand()`
- Remove `PromptTemplate` references

### DI Registration (`AppTrack.BlazorUi/Program.cs`)
```csharp
services.AddTransient<IValidator<PromptModel>, PromptModelValidator>();
```
`IModelValidator<PromptModel>` resolved automatically via existing open-generic `AddTransient(typeof(IModelValidator<>), typeof(ModelValidator<>))` registration.

---

## Blazor UI (`AppTrack.BlazorUi`)

### `AiSettingsDialog.razor` + `.razor.cs`
- Remove `PromptTemplate` textarea from `.razor`
- Remove `OnPromptTemplateChanged` and all `nameof(AiSettingsModel.PromptTemplate)` references from `.razor.cs` (`TreatWarningsAsErrors` will fail the build otherwise)
- Add "Prompts" section: scrollable list, Add/Edit/Delete buttons — mirrors existing `PromptParameter` section exactly

**Opening `PromptDialog` (Add/Edit)** — pass siblings via `DialogParameters<PromptDialog>`, consistent with how `SiblingParameters` is passed to `PromptParameterDialog`:
```csharp
var parameters = new DialogParameters<PromptDialog>
{
    { x => x.SiblingPrompts, _model.Prompts },
    { x => x.ExistingPrompt, existingPrompt } // null = Add mode
};
```

### New `PromptDialog.razor` + `PromptDialog.razor.cs`
- `[Parameter] public IEnumerable<PromptModel>? SiblingPrompts { get; set; }`
- `[Parameter] public PromptModel? ExistingPrompt { get; set; }`
- `OnParametersSet` — full model initialization (analogous to `PromptParameterDialog.OnParametersSet`):
  - If `ExistingPrompt is not null`: `_model = ExistingPrompt.Clone()` (edit mode — work on a copy)
  - Otherwise: `_model = new PromptModel()` (add mode)
  - Always: `_model.SiblingPrompts = SiblingPrompts`
- `Name` field: text input, required
- `PromptTemplate` field: multiline textarea (8 lines), required
- Injected `IModelValidator<PromptModel>` for validation

---

## Out of Scope (Next Step)
- Selecting/activating a specific prompt per user
- Global template prompts for new users

---

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| Inline update (Ansatz A) | Consistent with existing `PromptParameter` pattern |
| `PromptTemplate` removed from `AiSettings` | No longer meaningful once multiple prompts exist |
| `PromptParameter` stays on `AiSettings` unchanged | Parameters are global per user, not per prompt |
| `IAiSettingsValidatable` and `AiSettingsBaseValidator` extended additively | Both `PromptParameter` rules and new `Prompts` rules coexist |
| Name uniqueness at DB + validation level | Unique index + FluentValidation, same as `PromptParameter` key uniqueness |
| `Prompt.Create(...)` factory method | Consistent with `PromptParameter.Create(...)` domain pattern |
| `PromptModel` inherits `ModelBase` | Provides `Id` for round-tripping, consistent frontend model pattern |
| Relationship config in `AiSettingsConfiguration` | Consistent with `PromptParameter` relationship (not in `PromptParameterConfiguration`) |
| No `DbSet<Prompt>` on context | Consistent with `PromptParameter` — discovered via `ApplyConfigurationsFromAssembly` |
| Repository method names unchanged | Still correct — both include `PromptParameter`; adding `Prompts` is additive |
| NSwag client must be regenerated | API DTO contract changes when `PromptTemplate` removed from `AiSettingsDto` |
| Only `GeneratePromptQueryHandler` needs `FirstOrDefault()` fallback | `GenerateApplicationTextCommandHandler` uses `request.Prompt` — not affected |
| `SiblingPrompts` naming (not `ParentCollection`) | Clearer, more descriptive name for the same pattern |
| `Clone()` copies timestamps from source | Consistent with `PromptParameterModel.Clone()` — never use `DateTime.MinValue` |
