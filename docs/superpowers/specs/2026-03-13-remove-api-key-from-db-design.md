# Design: Remove API Key from Database & Blazor UI

**Date:** 2026-03-13
**Status:** Approved

## Summary

Remove the per-user OpenAI API key from the database and the Blazor UI input form. The API key will be sourced centrally from the application's configuration (User Secrets in development, Azure Key Vault in production).

## Background

Currently, each user stores their own OpenAI API key in the `AiSettings` database table. The Blazor UI provides a masked password field to enter and update this key. The key is fetched from the DB at text-generation time and passed to the `OpenAiApplicationTextGenerator` via `SetApiKey()`.

The goal is to use a single shared API key managed by the application operator, removing the burden of key management from end users and eliminating plaintext key storage in the database.

## Approach

Extend `OpenAiOptions` with an `ApiKey` property. The `OpenAiApplicationTextGenerator` reads the key from `IOptions<OpenAiOptions>` in its constructor. The `SetApiKey()` method and interface member are removed. All layers that previously carried or validated the API key are cleaned up.

## Changes by Layer

### Infrastructure (`AppTrack.Infrastructure`)

- `OpenAiOptions`: Add `string ApiKey` property (required, validated via `[Required]`)
- `OpenAiApplicationTextGenerator`:
  - Remove `SetApiKey(string)` method
  - Read `_apiKey` from `openAiOptions.Value.ApiKey` in constructor, replacing the previous `_apiKey` instance field assignment
- `IApplicationTextGenerator` (`AppTrack.Application/Contracts`): Remove `SetApiKey(string)` method

**Note:** The `OpenAiOptions` binding and `.ValidateDataAnnotations().ValidateOnStart()` registration already exist in `AppTrack.Api/Program.cs`. No changes to `InfrastructureServicesRegistration.cs` are needed.

### Application (`AppTrack.Application`)

- `GenerateApplicationTextCommandHandler`: Remove `_applicationTextGenerator.SetApiKey(aiSettings!.ApiKey)` call
- `GenerateApplicationTextCommandValidator`: Remove the `string.IsNullOrWhiteSpace(aiSettings.ApiKey)` guard in `ValidateAiSettings` (the `ApiKey` property no longer exists on the domain entity)
- `AiSettingsDto`: Remove `ApiKey` property
- `UpdateAiSettingsCommand`: Remove `ApiKey` property
- `AiSettingsMappings`:
  - `ApplyTo()`: Remove `entity.ApiKey = command.ApiKey`
  - `ToDto()`: Remove `ApiKey = entity.ApiKey`

### Domain (`AppTrack.Domain`)

- `AiSettings`: Remove `ApiKey` property

### Persistence (`AppTrack.Persistance`)

- `AiSettingsConfiguration`: Remove `HasMaxLength(200)` configuration for `ApiKey`
- New EF Core migration: Drop `ApiKey` column from `AiSettings` table

### Shared Validation (`AppTrack.Shared.Validation`)

- `IAiSettingsValidatable`: Remove `string ApiKey` property
- `AiSettingsBaseValidator<T>`: Remove `RuleFor(x => x.ApiKey)` rule

### Frontend Models (`AppTrack.Frontend.Models`)

- `AiSettingsModel`: Remove `ApiKey` property
- `AiSettingsModelValidator`: No change (inherits from base, rule is removed there)

### Frontend API Service (`AppTrack.Frontend.ApiService`)

The NSwag-generated client (`AppTrack.Frontend.ApiService`) is generated from the API's OpenAPI spec. The `AiSettingsPUTAsync` method accepts the command type directly — no separate wrapper type is generated. After the build, the NSwag client must be regenerated to reflect the removed `ApiKey` field.

Additionally, the hand-written mapping file must be updated:

- `ApiService/Mappings/AiSettingsMappings.cs`:
  - `ToModel()`: Remove `ApiKey = dto.ApiKey ?? string.Empty`
  - `ToUpdateCommand()`: Remove `ApiKey = model.ApiKey`

### Blazor UI (`AppTrack.BlazorUi`)

- `AiSettingsDialog.razor`: Remove the API key password input field and its Show/Hide toggle button
- `AiSettingsDialog.razor.cs`: Remove the following members:
  - `_apiKeyAttributes` dictionary (browser `autocomplete` hint)
  - `_apiKeyInputType` field
  - `_apiKeyInputIcon` field
  - `ToggleApiKeyVisibility()` method
  - `OnApiKeyChanged()` method

### Configuration

| Environment | Mechanism | Key path |
|-------------|-----------|----------|
| Development | `dotnet user-secrets` | `OpenAiSettings:ApiKey` |
| Production | Azure Key Vault | `OpenAiSettings--ApiKey` |

`appsettings.json` does **not** contain the actual key value. A placeholder (e.g. `"ApiKey": ""`) may be kept to document the expected configuration key, but the actual value must be injected via secrets or Key Vault. Since `.ValidateOnStart()` is enabled, the API will fail to start in any environment where `ApiKey` is not configured.

**Integration test environment:** The `AppTrack.Api.IntegrationTests` `WebApplicationFactory` must supply a non-empty `ApiKey` value. Use `builder.UseSetting("OpenAiSettings:ApiKey", "test-api-key")` in the factory configuration, or add the value to the test `appsettings.json`.

## Data Migration

An EF Core migration drops the `ApiKey` column from the `AiSettings` table. Existing API key values in the database will be lost. Operators must ensure the key is configured in the target environment before deploying.

## What Does NOT Change

- Prompt template, model selection, and prompt parameters remain in the database and UI
- All other `AiSettings` CQRS flow (get, update, create on first access) is unchanged
- No changes to authentication, authorization, or other features

## Testing Considerations

### Unit Tests (`AppTrack.Application.UnitTests`)

- `UpdateAiSettingsCommandHandler` / `UpdateAiSettingsCommandValidator` tests: Remove any `ApiKey` property assignments from test command construction and any assertions on `result.ApiKey`
- `GenerateApplicationTextCommandHandler` tests: Remove `SetApiKey` call verification from the mock `IApplicationTextGenerator` setup

### API Integration Tests (`AppTrack.Api.IntegrationTests`)

- `UpdateAiSettingsTests.cs`:
  - Remove `ApiKey = "..."` from all `UpdateAiSettingsCommand` object initializers (multiple occurrences)
  - Remove `result.ApiKey.ShouldBe(validRequest.ApiKey)` assertion
  - Delete the `UpdateAiSettings_ShouldReturn400_WhenApiKeyExceedsMaxChars` test entirely (tests behavior that is being removed)
- `AiSettingsSeedsHelper.cs`: Remove `ApiKey = "1234abc"` from the `AiSettings` domain object initializer in `CreateAiSettingsForUserAsync`
- `WebApplicationFactory`: Add `builder.UseSetting("OpenAiSettings:ApiKey", "test-api-key")` (or equivalent) to satisfy the `[Required]` validation on startup

### Infrastructure Unit Tests

- `OpenAiApplicationTextGeneratorTests` (if present): Remove `SetApiKey` tests; add test verifying key is read from `OpenAiOptions` in constructor
