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

### 1. `DefaultPrompt.Create()` — Domain (`AppTrack.Domain`)

Add two guards after the existing `ArgumentNullException.ThrowIfNull` calls:

- `ArgumentException` if name does not start with `"Default_"`
- `ArgumentException` if name contains a space

Add a short comment: seeder code runs outside the FluentValidation pipeline, so guards are the only domain-level enforcement here.

### 2. `PromptBaseValidator` — Shared (`AppTrack.Shared.Validation`)

Add two new rules to the existing `Name` rule chain:

- Name must not start with `"Default_"` (case-insensitive) → `"A prompt name must not start with 'Default_'."`
- Name must not contain spaces → `"A prompt name must not contain spaces."`

These rules apply to all validators that inherit from `PromptBaseValidator`:
`PromptDtoValidator` (backend) and `PromptModelValidator` (frontend).

### 3. Seed Data + EF Core Migration — Persistence (`AppTrack.Persistance`)

Rename the four seeded default prompts to English names with the `Default_` prefix:

| Old name | New name |
|---|---|
| `Anschreiben` | `Default_Cover_Letter` |
| `LinkedIn Nachricht` | `Default_LinkedIn_Message` |
| `Vorstellung` | `Default_Introduction` |
| `Nachfassen` | `Default_Follow_Up` |

Create a new EF Core migration that updates the four existing rows.

### 4. Init Test — Persistence Integration Tests (`AppTrack.Persistance.IntegrationTests`)

Add a new test class `DefaultPromptSeedTests` using the InMemory database.

Two tests:
- All seeded `DefaultPrompt` names start with `"Default_"`
- All seeded `DefaultPrompt` names contain no spaces

These tests act as a safety net for future additions to the seed data.

### 5. Update Existing Tests

- `DefaultPromptFactoryTests`: update all test names to use the `Default_` prefix (e.g., `"Default_Cover_Letter"`)
- `PromptDtoValidatorTests`: add test cases for the two new rules (no `Default_` prefix, no spaces)

## What Does Not Change

- `GeneratePromptQueryHandler`: lookup by `PromptName` is unchanged — it searches by full name including prefix
- `AiSettingsBaseValidator`: `HaveUniqueNames` remains as-is; the prefix makes cross-namespace conflicts structurally impossible
- `UpdateAiSettingsCommandValidator`: no async default-prompt lookup needed

## Testing Strategy

| Layer | Test type | What is tested |
|---|---|---|
| Domain | Unit (`DefaultPromptFactoryTests`) | `Create()` throws on missing prefix and on spaces |
| Shared validation | Unit (`PromptDtoValidatorTests`) | New rules block `Default_` prefix and spaces in user prompts |
| Persistence | Integration (`DefaultPromptSeedTests`) | All seed entries follow the naming convention |
