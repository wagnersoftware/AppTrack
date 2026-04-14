# Design: Default Prompt Prefix Enforcement

**Date:** 2026-04-04
**Branch:** feature/default-prompt-templates

## Goal

Prevent users from creating or renaming prompts whose names conflict with default prompt names.
The mechanism is a reserved prefix `Default_` that structurally separates the two namespaces.

## Rules

| Subject | Rule |
|---|---|
| Default prompts | Name must start with `Default_` (case-sensitive) |
| Default prompts | Name must not contain spaces |
| User prompts | Name must not start with `Default_` (case-insensitive) |
| User prompts | Name must not contain spaces |

No spaces are allowed in any prompt name. Underscores are used as word separators.

## Changes

### 1. `BuiltInPrompt.Create()` — Domain (`AppTrack.Domain`)

Add two guards immediately after the existing `ArgumentNullException.ThrowIfNull` calls:

```csharp
// Seeder code runs outside the FluentValidation pipeline, so guards are the
// only domain-level enforcement for these invariants.
if (!name.StartsWith("Default_", StringComparison.Ordinal))
    throw new ArgumentException("Default prompt names must start with 'Default_'.", nameof(name));

if (name.Contains(' '))
    throw new ArgumentException("Default prompt names must not contain spaces.", nameof(name));
```

> **Important:** Changes 1 and 3 must be committed atomically. Applying the guards before renaming the seed entries will cause an immediate build/runtime failure because the existing names (`"Anschreiben"` etc.) violate the new guards.



### 2. `PromptBaseValidator` — Shared (`AppTrack.Shared.Validation`)

Add two new rules to the existing `Name` rule chain:

- Name must not start with `"Default_"` (case-insensitive) → error message: `"A prompt name must not start with 'Default_'."`
- Name must not contain spaces → error message: `"A prompt name must not contain spaces."`

These rules propagate automatically to all inheriting validators:
- Backend: `PromptDtoValidator` (via `PromptBaseValidator<PromptDto>`)
- Backend: `AiSettingsBaseValidator` (via its private `PromptItemValidator`)
- Frontend: `PromptModelValidator` (via `PromptBaseValidator<PromptModel>`)

### 3. Seed Data + EF Core Migration — Persistence (`AppTrack.Persistance`)

Rename the four seeded default prompts to English names with the `Default_` prefix:

| Old name | New name |
|---|---|
| `Anschreiben` | `Default_Cover_Letter` |
| `LinkedIn Nachricht` | `Default_LinkedIn_Message` |
| `Vorstellung` | `Default_Introduction` |
| `Nachfassen` | `Default_Follow_Up` |

Create a new EF Core migration that updates the four existing rows via `UpdateData`.

> **Must be applied atomically with Change 1** — see note in Change 1.

### 4. Init Test — Persistence Integration Tests (`AppTrack.Persistance.IntegrationTests`)

Add a new test class `BuiltInPromptSeedTests` using the InMemory database.

Two tests:
- All seeded `BuiltInPrompt` names start with `"Default_"`
- All seeded `BuiltInPrompt` names contain no spaces

These tests act as a safety net for future additions to the seed data.

### 5. Update Existing Tests

All existing test fixtures that use prompt names with spaces must be renamed to use underscores. The following files are affected:

**`PromptBaseValidatorTests.cs`**
- Rename fixture `"My Prompt"` → `"My_Prompt"` (used in `ValidPrompt_ShouldPass` and `EmptyPromptTemplate_ShouldFail`)
- Add: `NameWithSpace_ShouldFail` — verifies the no-spaces rule
- Add: `NameStartingWithDefaultPrefix_ShouldFail` — verifies the no-`Default_`-prefix rule

**`AiSettingsBaseValidatorTests.cs`**
- Rename `"Prompt A"` → `"Prompt_A"` and `"Prompt B"` → `"Prompt_B"` in `ValidPrompts_ShouldPass`

**`PromptDtoValidatorTests.cs`**
- Rename `BuildValidDto()` fixture name `"Cover Letter"` → `"Cover_Letter"`
- Add: `Validate_ShouldHaveError_WhenNameContainsSpace`
- Add: `Validate_ShouldHaveError_WhenNameStartsWithDefaultPrefix`

**`BuiltInPromptFactoryTests.cs`**
- Update valid-prompt test: both the `Create(...)` argument and the `result.Name.ShouldBe(...)` assertion from `"Anschreiben"` → `"Default_Cover_Letter"`
- Add: `Create_ShouldThrowArgumentException_WhenNameDoesNotStartWithDefaultPrefix`
- Add: `Create_ShouldThrowArgumentException_WhenNameContainsSpace`

## What Does Not Change

- `GeneratePromptQueryHandler`: lookup by `PromptName` is unchanged — it searches by full name including prefix
- `AiSettingsBaseValidator`: `HaveUniqueNames` remains as-is; the prefix makes cross-namespace conflicts structurally impossible
- `UpdateAiSettingsCommandValidator`: no async default-prompt lookup needed

## Out of Scope

Existing user prompts in the database that happen to contain spaces are not migrated or renamed. The new validation rules will only prevent saving such prompts going forward via the normal command pipeline. This is a conscious decision — retroactive data cleanup is not part of this change.

## Testing Strategy

| Layer | Test type | File | What is tested |
|---|---|---|---|
| Domain | Unit | `BuiltInPromptFactoryTests` | `Create()` throws on missing prefix and on spaces (2 new negative tests) |
| Shared validation | Unit | `PromptBaseValidatorTests` | New rules block spaces and `Default_` prefix in user prompts (2 new tests) |
| Shared validation | Unit | `AiSettingsBaseValidatorTests` | Existing valid-prompt fixture renamed to avoid spaces |
| Backend | Unit | `PromptDtoValidatorTests` | Fixture renamed + 2 new test cases for the new rules |
| Persistence | Integration | `BuiltInPromptSeedTests` | All seed entries follow the naming convention (safety net for future additions) |
| Frontend | — | — | No unit tests exist for `PromptModelValidator`; rules propagate automatically via `PromptBaseValidator` |
