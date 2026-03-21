# Design: Multiple Prompts per User in AI Settings

**Date:** 2026-03-21
**Status:** Approved
**Scope:** Full stack — Domain, Application, API, Frontend Models, Blazor UI

---

## Overview

Currently each user has exactly one `AiSettings` instance containing a single `PromptTemplate` string. This feature replaces that with a named, ordered collection of `Prompt` entities per user, allowing users to manage multiple reusable prompts.

Selection of an active prompt is **out of scope** for this iteration and will be implemented as a follow-up step.

The architecture is designed to support global template prompts (available to new users) in a future iteration — the `Prompt` entity is intentionally clean so that a `IsTemplate` flag or separate template table can be added without schema changes.

---

## Data Model & Database

### New Entity: `Prompt` (`AppTrack.Domain`)

| Field           | Type   | Constraints                          |
|-----------------|--------|--------------------------------------|
| Id              | int    | PK, Identity                         |
| Name            | string | required, max 100 chars              |
| PromptTemplate  | string | required                             |
| AiSettingsId    | int    | FK → AiSettings, CASCADE DELETE      |

### Changes to `AiSettings`

- Remove field: `PromptTemplate`
- Add navigation: `ICollection<Prompt> Prompts`

### EF Core Configuration

- New `PromptConfiguration : IEntityTypeConfiguration<Prompt>`
- Unique index on `(AiSettingsId, Name)`
- Cascade delete already handled by FK

### Migration

- New table `Prompts` with the fields above
- Drop column `PromptTemplate` from `AiSettings`

---

## Application Layer (`AppTrack.Application`)

### DTOs

- New `PromptDto` with `Id`, `Name`, `PromptTemplate`
- `AiSettingsDto`: remove `PromptTemplate`, add `List<PromptDto> Prompts`

### Commands

- `UpdateAiSettingsCommand`: remove `PromptTemplate`, add `List<PromptDto> Prompts`
- Handler: clears and recreates the `Prompts` collection on every save (same pattern as `PromptParameter`)

### Mappings (`AiSettingsMappings.cs`)

- `ApplyTo` extension: clear + recreate `Prompts` from command DTOs
- `ToDto` extension: map `Prompt` entities to `PromptDto`

---

## Shared Validation (`AppTrack.Shared.Validation`)

### New Interface: `IPromptValidatable`

```csharp
public interface IPromptValidatable
{
    string Name { get; }
    string PromptTemplate { get; }
}
```

### New Base Validator: `PromptBaseValidator<T>`

- `Name`: required, max 100 characters
- `PromptTemplate`: required

### Changes to `IAiSettingsValidatable`

- Add: `IEnumerable<IPromptValidatable> Prompts`

### Changes to `AiSettingsBaseValidator<T>`

- Add `RuleForEach` for `Prompts` using `PromptBaseValidator`
- Add uniqueness rule: `Name` must be unique within the collection (case-insensitive), analogous to the existing `PromptParameter` key uniqueness rule

---

## API Layer (`AppTrack.Api`)

No new endpoints. Everything flows through the existing:

```
PUT /api/ai-settings/{id}
```

The request body (`UpdateAiSettingsCommand`) is extended with the `Prompts` list.

---

## Frontend Models (`AppTrack.Frontend.Models`)

### New Model: `PromptModel`

- `Name` (string, observable)
- `PromptTemplate` (string, observable)
- `TempId` (Guid, for local identity before save)
- `Clone()` method (for dialog edit-without-mutate pattern)
- Implements `IPromptValidatable`

### Changes to `AiSettingsModel`

- Remove: `PromptTemplate`
- Add: `ObservableCollection<PromptModel> Prompts`
- Implements updated `IAiSettingsValidatable`

### New Validator: `PromptModelValidator`

- Inherits `PromptBaseValidator<PromptModel>`
- Adds sibling-collection uniqueness check on `Name` (case-insensitive, ignores own `TempId`)

### Mappings (`ApiService/Mappings/AiSettingsMappings.cs`)

- Map `PromptDto` ↔ `PromptModel`
- Map `PromptModel` → `PromptDto` in `ToUpdateCommand()`

---

## Blazor UI (`AppTrack.BlazorUi`)

### Changes to `AiSettingsDialog`

- Remove: `PromptTemplate` textarea from the main form
- Add: "Prompts" section with scrollable list + Add / Edit / Delete buttons
- Pattern mirrors the existing `PromptParameter` section exactly

### New Component: `PromptDialog.razor`

A MudBlazor dialog for creating and editing a single prompt:

- `Name` field: text input, required
- `PromptTemplate` field: multiline textarea (8 lines), required
- Supports both Add and Edit mode (receives optional `ExistingPrompt` parameter)
- Validates against sibling collection for name uniqueness

---

## Out of Scope (Next Step)

- Selecting/activating a specific prompt per user
- Global template prompts for new users

---

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| Inline update (Ansatz A) | Consistent with existing `PromptParameter` pattern — one PUT saves everything |
| `PromptTemplate` removed from `AiSettings` | No longer meaningful once multiple prompts exist |
| `PromptParameter` stays on `AiSettings` | Parameters are global per user, not per prompt |
| Name uniqueness at DB + validation level | Enforced by unique index and FluentValidation, same as `PromptParameter` key uniqueness |
| No new API endpoints | Keeps the API surface minimal for this iteration |
