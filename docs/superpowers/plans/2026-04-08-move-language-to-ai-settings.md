# Move Language Setting to AI Settings — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move `ApplicationLanguage Language` from `FreelancerProfile` to `AiSettings`, wire it to `DefaultPromptRepository.GetByLanguageAsync`, and default to English.

**Architecture:** The field controls AI text-generation language, so it belongs in `AiSettings` alongside model selection. The `GetAiSettingsByUserIdQueryHandler` and `GeneratePromptQueryHandler`/`GeneratePromptQueryValidator` switch from `GetAsync()` to `GetByLanguageAsync(code)` where `code` is `"de"` or `"en"`.

**Tech Stack:** .NET 10, EF Core 10, FluentValidation 12, MudBlazor, Blazor WASM, xUnit + Shouldly + Moq

---

## Chunk 1: Domain + Application layer

### Task 1: Update domain entities

**Files:**
- Modify: `AppTrack.Domain/FreelancerProfile.cs`
- Modify: `AppTrack.Domain/AiSettings.cs`

- [ ] **Step 1: Remove `Language` from `FreelancerProfile`**

In `AppTrack.Domain/FreelancerProfile.cs`, delete:
```csharp
public ApplicationLanguage? Language { get; set; }
```
Also remove the `using AppTrack.Domain.Enums;` if it is now unused (check whether `RemotePreference` also lives there — if yes, keep the using).

- [ ] **Step 2: Add `Language` to `AiSettings` with default English**

In `AppTrack.Domain/AiSettings.cs`, add after `SelectedChatModelId`:
```csharp
public ApplicationLanguage Language { get; set; } = ApplicationLanguage.English;
```
Add using: `using AppTrack.Domain.Enums;`

- [ ] **Step 3: Build domain project to confirm no errors**

```bash
dotnet build AppTrack.Domain/AppTrack.Domain.csproj --configuration Release
```
Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

### Task 2: Update Application layer DTOs, command, and mappings

**Files:**
- Modify: `AppTrack.Application/Features/FreelancerProfile/Dto/FreelancerProfileDto.cs`
- Modify: `AppTrack.Application/Features/FreelancerProfile/Commands/UpsertFreelancerProfile/UpsertFreelancerProfileCommand.cs`
- Modify: `AppTrack.Application/Mappings/FreelancerProfileMappings.cs`
- Modify: `AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs`
- Modify: `AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/UpdateAiSettingsCommand.cs`
- Modify: `AppTrack.Application/Mappings/AiSettingsMappings.cs`

- [ ] **Step 1: Remove `Language` from `FreelancerProfileDto`**

In `AppTrack.Application/Features/FreelancerProfile/Dto/FreelancerProfileDto.cs`, delete:
```csharp
public ApplicationLanguage? Language { get; set; }
```
Remove `using AppTrack.Domain.Enums;` if unused.

- [ ] **Step 2: Remove `Language` from `UpsertFreelancerProfileCommand`**

In `AppTrack.Application/Features/FreelancerProfile/Commands/UpsertFreelancerProfile/UpsertFreelancerProfileCommand.cs`, delete:
```csharp
public ApplicationLanguage? Language { get; set; }
```
Remove unused `using AppTrack.Domain.Enums;` if needed.

- [ ] **Step 3: Remove `Language` from `FreelancerProfileMappings`**

In `AppTrack.Application/Mappings/FreelancerProfileMappings.cs`, remove all three occurrences:
- In `ToNewDomain`: delete `Language = command.Language,`
- In `ApplyTo`: delete `entity.Language = command.Language;`
- In `ToDto`: delete `Language = entity.Language,`

- [ ] **Step 4: Add `Language` to `AiSettingsDto`**

In `AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs`, add:
```csharp
using AppTrack.Domain.Enums;
```
And add property after `SelectedChatModelId`:
```csharp
public ApplicationLanguage Language { get; set; } = ApplicationLanguage.English;
```

- [ ] **Step 5: Add `Language` to `UpdateAiSettingsCommand`**

In `AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/UpdateAiSettingsCommand.cs`, add:
```csharp
using AppTrack.Domain.Enums;
```
And add property after `SelectedChatModelId`:
```csharp
public ApplicationLanguage Language { get; set; }
```

- [ ] **Step 6: Add `Language` to `AiSettingsMappings`**

In `AppTrack.Application/Mappings/AiSettingsMappings.cs`:

In `ApplyTo`, add after `entity.SelectedChatModelId = command.SelectedChatModelId;`:
```csharp
entity.Language = command.Language;
```

In `ToDto`, add after `SelectedChatModelId = entity.SelectedChatModelId,`:
```csharp
Language = entity.Language,
```

- [ ] **Step 7: Build application project**

```bash
dotnet build AppTrack.Application/AppTrack.Application.csproj --configuration Release
```
Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

### Task 3: Wire `GetByLanguageAsync` in handlers and validator

**Files:**
- Modify: `AppTrack.Application/Features/AiSettings/Queries/GetAiSettingsByUserId/GetAiSettingsByUserIdQueryHandler.cs`
- Modify: `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryHandler.cs`
- Modify: `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryValidator.cs`

**Context:** `DefaultPrompt.Language` is a string ISO 639-1 code ("de" or "en"). `ApplicationLanguage.German = 0`, `ApplicationLanguage.English = 1`. Use inline conversion: `entity.Language == ApplicationLanguage.German ? "de" : "en"`.

- [ ] **Step 1: Update `GetAiSettingsByUserIdQueryHandler` to use `GetByLanguageAsync`**

Replace the line that loads default prompts:
```csharp
// BEFORE
var defaults = await _defaultPromptRepository.GetAsync();

// AFTER
var languageCode = entity.Language == ApplicationLanguage.German ? "de" : "en";
var defaults = await _defaultPromptRepository.GetByLanguageAsync(languageCode);
```
Add `using AppTrack.Domain.Enums;` if not present.

- [ ] **Step 2: Update `GeneratePromptQueryHandler` to use language-filtered defaults**

In the handler's `Handle` method, where it loads defaults for a `Default_` prompt, replace:
```csharp
// BEFORE
var defaults = await _defaultPromptRepository.GetAsync();

// AFTER
var languageCode = aiSettings!.Language == ApplicationLanguage.German ? "de" : "en";
var defaults = await _defaultPromptRepository.GetByLanguageAsync(languageCode);
```
Add `using AppTrack.Domain.Enums;` if not present. Note: `aiSettings` is already loaded on line 37 of the handler.

- [ ] **Step 3: Update `GeneratePromptQueryValidator` to use language-filtered defaults**

In `ValidateAiSettings`, replace:
```csharp
// BEFORE
var defaults = await _defaultPromptRepository.GetAsync();

// AFTER
var languageCode = aiSettings!.Language == ApplicationLanguage.German ? "de" : "en";
var defaults = await _defaultPromptRepository.GetByLanguageAsync(languageCode);
```
Note: `aiSettings` is already loaded earlier in `ValidateAiSettings`.
Add `using AppTrack.Domain.Enums;` if not present.

- [ ] **Step 4: Build full solution**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```
Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

### Task 4: Update unit tests

**Files:**
- Modify: `AppTrack.Application.UnitTests/Features/AiSettings/Queries/GetAiSettingsByUserIdQueryHandlerTests.cs`
- Modify: `AppTrack.Application.UnitTests/Features/AiSettings/Commands/UpdateAiSettingsCommandHandlerTests.cs`
- Modify: `AppTrack.Application.UnitTests/Mocks/MockFreelancerProfileRepository.cs`

- [ ] **Step 1: Update `GetAiSettingsByUserIdQueryHandlerTests` to use `GetByLanguageAsync`**

The tests currently mock `_mockDefaultPromptRepo.Setup(r => r.GetAsync())`. These must be updated to mock `GetByLanguageAsync` instead.

Replace the constructor setup:
```csharp
// BEFORE
_mockDefaultPromptRepo
    .Setup(r => r.GetAsync())
    .ReturnsAsync(new List<DefaultPrompt>());

// AFTER
_mockDefaultPromptRepo
    .Setup(r => r.GetByLanguageAsync(It.IsAny<string>()))
    .ReturnsAsync(new List<DefaultPrompt>());
```

Update `Handle_ShouldPopulateDefaultPrompts_InReturnedDto` — replace the mock setup:
```csharp
// BEFORE
_mockDefaultPromptRepo
    .Setup(r => r.GetAsync())
    .ReturnsAsync(defaults);

// AFTER
_mockDefaultPromptRepo
    .Setup(r => r.GetByLanguageAsync(It.IsAny<string>()))
    .ReturnsAsync(defaults);
```

Add new test verifying English is used by default:
```csharp
[Fact]
public async Task Handle_ShouldRequestEnglishDefaultPrompts_WhenLanguageIsEnglish()
{
    const string userId = "user-1";
    _mockRepo
        .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userId))
        .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = userId, Language = ApplicationLanguage.English });
    _mockDefaultPromptRepo
        .Setup(r => r.GetByLanguageAsync("en"))
        .ReturnsAsync(new List<DefaultPrompt>());

    await CreateHandler().Handle(new GetAiSettingsByUserIdQuery { UserId = userId }, CancellationToken.None);

    _mockDefaultPromptRepo.Verify(r => r.GetByLanguageAsync("en"), Times.Once);
}

[Fact]
public async Task Handle_ShouldRequestGermanDefaultPrompts_WhenLanguageIsGerman()
{
    const string userId = "user-1";
    _mockRepo
        .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userId))
        .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = userId, Language = ApplicationLanguage.German });
    _mockDefaultPromptRepo
        .Setup(r => r.GetByLanguageAsync("de"))
        .ReturnsAsync(new List<DefaultPrompt>());

    await CreateHandler().Handle(new GetAiSettingsByUserIdQuery { UserId = userId }, CancellationToken.None);

    _mockDefaultPromptRepo.Verify(r => r.GetByLanguageAsync("de"), Times.Once);
}
```
Add `using AppTrack.Domain.Enums;` at the top.

- [ ] **Step 2: Add Language test to `UpdateAiSettingsCommandHandlerTests`**

Add test verifying Language is persisted (add after `Handle_WithEmptyPrompts_ShouldReturnEmptyPromptsList`):
```csharp
[Fact]
public async Task Handle_ShouldReturnDto_WithLanguage_WhenLanguageIsGerman()
{
    var command = new UpdateAiSettingsCommand
    {
        Id = ExistingId,
        UserId = OwnerId,
        SelectedChatModelId = 2,
        Language = ApplicationLanguage.German,
        Prompts = [],
        PromptParameter = []
    };

    var result = await _handler.Handle(command, CancellationToken.None);

    result.Language.ShouldBe(ApplicationLanguage.German);
}
```
Add `using AppTrack.Domain.Enums;` if not present.

- [ ] **Step 3: Remove `Language` reference from `MockFreelancerProfileRepository`**

`MockFreelancerProfileRepository.cs` currently does **not** set `Language` on the existing profile entity (it was never added to the mock). No change needed here — just verify the build is clean.

- [ ] **Step 4: Run all unit tests and verify they pass**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```
Expected: `Passed! - Failed: 0`

- [ ] **Step 5: Commit**

```bash
git add AppTrack.Domain/ AppTrack.Application/ AppTrack.Application.UnitTests/
git commit -m "feat: move Language setting from FreelancerProfile to AiSettings, wire GetByLanguageAsync"
```

---

## Chunk 2: EF Core migration + frontend

### Task 5: EF Core migration

**Files:**
- Create: `AppTrack.Persistance/Migrations/<timestamp>_MoveLanguageToAiSettings.cs` (generated)
- Modify: `AppTrack.Persistance/Migrations/AppTrackDatabaseContextModelSnapshot.cs` (auto-updated)

- [ ] **Step 1: Add migration**

```bash
dotnet ef migrations add MoveLanguageToAiSettings --project AppTrack.Persistance --startup-project AppTrack.Api
```
Expected: new migration file created.

- [ ] **Step 2: Verify migration content**

Open the generated migration and confirm:
- `FreelancerProfile` table: `DropColumn("Language")` (or equivalent)
- `AiSettings` table: `AddColumn<int>("Language", defaultValue: 1)` (1 = English)

If the `defaultValue` for `AiSettings.Language` is `0` (German) instead of `1` (English), edit the migration to set `defaultValue: 1`.

- [ ] **Step 3: Build persistence project**

```bash
dotnet build AppTrack.Persistance/AppTrack.Persistance.csproj --configuration Release
```
Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

### Task 6: Update frontend models and API service mappings

**Files:**
- Modify: `Models/FreelancerProfileModel.cs`
- Modify: `Models/AiSettingsModel.cs`
- Modify: `ApiService/Mappings/FreelancerProfileMappings.cs`
- Modify: `ApiService/Mappings/AiSettingsMappings.cs`
- Modify: `ApiService/Base/ServiceClient.cs`

- [ ] **Step 1: Remove `Language` from `FreelancerProfileModel`**

In `Models/FreelancerProfileModel.cs`, delete:
```csharp
public ApplicationLanguage? Language { get; set; }
```
Remove `using AppTrack.Frontend.Models;` for `ApplicationLanguage` if unused (check — `ApplicationLanguage` is in the same namespace `AppTrack.Frontend.Models`, so no using needed; just delete the property).

- [ ] **Step 2: Add `Language` to `AiSettingsModel`**

In `Models/AiSettingsModel.cs`, add:
```csharp
public ApplicationLanguage Language { get; set; } = ApplicationLanguage.English;
```
`ApplicationLanguage` is in the same `AppTrack.Frontend.Models` namespace — no extra using needed.

- [ ] **Step 3: Remove `Language` from `ApiService/Mappings/FreelancerProfileMappings.cs`**

In `ToUpsertCommand`, delete:
```csharp
Language = model.Language.HasValue
    ? (AppTrack.Frontend.ApiService.Base.ApplicationLanguage)(int)model.Language.Value
    : default,
```

In `ToModel`, delete:
```csharp
Language = (AppTrack.Frontend.Models.ApplicationLanguage)(int)dto.Language,
```

- [ ] **Step 4: Add `Language` to `ApiService/Mappings/AiSettingsMappings.cs`**

In `ToModel`, add after `SelectedChatModelId = dto.SelectedChatModelId,`:
```csharp
Language = (AppTrack.Frontend.Models.ApplicationLanguage)(int)dto.Language,
```

In `ToUpdateCommand`, add after `SelectedChatModelId = model.SelectedChatModelId,`:
```csharp
Language = (AppTrack.Frontend.ApiService.Base.ApplicationLanguage)(int)model.Language,
```

- [ ] **Step 5: Update `ServiceClient.cs` (generated — edit manually)**

`ServiceClient.cs` has no `#nullable` context and no name collisions, so edits are straightforward. The `ApplicationLanguage` enum is already defined there (`_0 = 0`, `_1 = 1`) — no change needed to the enum itself.

**Remove from `FreelancerProfileDto`** (around line 2172):
```csharp
// DELETE this block:
[System.Text.Json.Serialization.JsonPropertyName("language")]
public ApplicationLanguage Language { get; set; }
```

**Remove from `UpsertFreelancerProfileCommand`** (around line 2500):
```csharp
// DELETE this block:
[System.Text.Json.Serialization.JsonPropertyName("language")]
public ApplicationLanguage Language { get; set; }
```

**Add to `AiSettingsDto`** (after `SelectedChatModelId` property, around line 2017):
```csharp
[System.Text.Json.Serialization.JsonPropertyName("language")]
public ApplicationLanguage Language { get; set; }
```

**Add to `UpdateAiSettingsCommand`** (after `SelectedChatModelId` property, around line 2403):
```csharp
[System.Text.Json.Serialization.JsonPropertyName("language")]
public ApplicationLanguage Language { get; set; }
```

- [ ] **Step 6: Build full solution**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```
Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

### Task 7: Update Blazor UI

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor`
- Modify: `AppTrack.BlazorUi/Components/Pages/AiSettings.razor`

- [ ] **Step 1: Remove Language dropdown from `FreelancerProfileForm.razor`**

Delete the entire `<!-- Application Language -->` section (lines 138–151 of the current file):
```razor
<!-- Application Language -->
<MudItem xs="12">
    <MudSelect T="ApplicationLanguage?"
               Label="Application Language"
               Value="Model.Language"
               ValueChanged="@((ApplicationLanguage? v) => Model.Language = v)"
               Variant="Variant.Outlined"
               Clearable="true"
               HelperText="Language for AI-generated application text"
               AdornmentIcon="@Icons.Material.Outlined.Translate">
        <MudSelectItem T="ApplicationLanguage?" Value="@((ApplicationLanguage?)ApplicationLanguage.German)">German</MudSelectItem>
        <MudSelectItem T="ApplicationLanguage?" Value="@((ApplicationLanguage?)ApplicationLanguage.English)">English</MudSelectItem>
    </MudSelect>
</MudItem>
```

- [ ] **Step 2: Add Language dropdown to `AiSettings.razor`**

Insert after the Chat Model `<MudItem>` block (after `</MudItem>` on ~line 43, before the Default Prompts section):
```razor
<!-- Application Language -->
<MudItem xs="12" sm="6">
    <MudSelect T="ApplicationLanguage"
               Label="Application Language"
               Value="_model.Language"
               ValueChanged="@((ApplicationLanguage v) => _model.Language = v)"
               Variant="Variant.Outlined"
               HelperText="Language for AI-generated text"
               AdornmentIcon="@Icons.Material.Outlined.Translate">
        <MudSelectItem T="ApplicationLanguage" Value="ApplicationLanguage.English">English</MudSelectItem>
        <MudSelectItem T="ApplicationLanguage" Value="ApplicationLanguage.German">German</MudSelectItem>
    </MudSelect>
</MudItem>
```

Add the using at the top of the file if not already present (check the existing usings in the `@using` directives or `_Imports.razor`):
```razor
@using AppTrack.Frontend.Models
```

- [ ] **Step 3: Build full solution including Blazor**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```
Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Run all unit tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```
Expected: `Passed! - Failed: 0`

- [ ] **Step 5: Commit**

```bash
git add .
git commit -m "feat: wire Language to AI settings UI, migrate DB, update frontend mappings"
```
