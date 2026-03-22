# Remove UserId from API Contract Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove `UserId` from all inbound request DTOs (Commands/Queries) so clients can never supply or spoof it — the backend mediator always derives it from the JWT token.

**Architecture:** Add `[JsonIgnore]` to `UserId` on all 11 `IUserScopedRequest` implementations so ASP.NET Core's JSON deserializer ignores any client-supplied value. Remove `UserId` from frontend models (`AiSettingsModel`, `JobApplicationDefaultsModel`) and all frontend-to-command mappings. Regenerate the NSwag client so the generated TypeScript/C# client no longer exposes `UserId` on command/query classes. Backend validators and handlers continue to use `UserId` as before — the mediator sets it before dispatch.

**Tech Stack:** .NET 10, ASP.NET Core, System.Text.Json, FluentValidation 12, Blazor WASM, NSwag

**Spec:** `docs/superpowers/specs/` (no dedicated spec — derived from architecture review)

---

## Chunk 1: Backend — Add `[JsonIgnore]` to `UserId` on all Commands/Queries

### Task 1: Add `[JsonIgnore]` to `CreateJobApplicationCommand`

**Files:**
- Modify: `AppTrack.Application/Features/JobApplications/Commands/CreateJobApplication/CreateJobApplicationCommand.cs`

- [ ] **Step 1: Add `[JsonIgnore]`**

Add `using System.Text.Json.Serialization;` and decorate `UserId`:

```csharp
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain.Enums;
using AppTrack.Shared.Validation.Interfaces;
using System.Text.Json.Serialization;

namespace AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;

public class CreateJobApplicationCommand : IRequest<JobApplicationDto>, IJobApplicationValidatable, IUserScopedRequest
{
    public DateTime CreationDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string ApplicationText { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;

    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;

    public string JobDescription { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public string DurationInMonths { get; set; } = string.Empty;
}
```

> **Note:** Read the actual file first to ensure all existing properties are preserved exactly.

- [ ] **Step 2: Build Application project**

```bash
dotnet build AppTrack.Application/AppTrack.Application.csproj --configuration Release
```
Expected: Build succeeded, 0 errors, 0 warnings.

---

### Task 2: Add `[JsonIgnore]` to remaining 10 Commands/Queries

**Files:**
- Modify: `AppTrack.Application/Features/JobApplications/Commands/UpdateJobApplication/UpdateJobApplicationCommand.cs`
- Modify: `AppTrack.Application/Features/JobApplications/Commands/DeleteJobApplication/DeleteJobApplicationCommand.cs`
- Modify: `AppTrack.Application/Features/JobApplications/Queries/GetJobApplicationById/GetJobApplicationByIdQuery.cs`
- Modify: `AppTrack.Application/Features/JobApplications/Queries/GetAllJobApplicationsForUser/GetJobApplicationsForUserQuery.cs`
- Modify: `AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/UpdateAiSettingsCommand.cs`
- Modify: `AppTrack.Application/Features/AiSettings/Commands/GenerateApplicationText/GenerateApplicationTextCommand.cs`
- Modify: `AppTrack.Application/Features/AiSettings/Queries/GetAiSettingsByUserId/GetAiSettingsByUserIdQuery.cs`
- Modify: `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQuery.cs`
- Modify: `AppTrack.Application/Features/JobApplicationDefaults/Commands/UpdateApplicationDefaults/UpdateJobApplicationDefaultsCommand.cs`
- Modify: `AppTrack.Application/Features/JobApplicationDefaults/Queries/GetJobApplicationDefaultsByUserId/GetJobApplicationDefaultsByUserIdQuery.cs`

For each file:
1. Add `using System.Text.Json.Serialization;` at the top
2. Add `[JsonIgnore]` attribute directly above the `public string UserId { get; set; }` line

Pattern to apply to **every** file:

```csharp
// Before:
public string UserId { get; set; } = string.Empty;

// After:
[JsonIgnore]
public string UserId { get; set; } = string.Empty;
```

- [ ] **Step 1: Edit all 10 files** (apply pattern above to each)

- [ ] **Step 2: Build Application project**

```bash
dotnet build AppTrack.Application/AppTrack.Application.csproj --configuration Release
```
Expected: Build succeeded, 0 errors, 0 warnings.

- [ ] **Step 3: Run unit tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```
Expected: All tests pass. (Unit tests set `UserId` directly on C# objects — they bypass JSON deserialization and are unaffected.)

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Application/
git commit -m "feat: add [JsonIgnore] to UserId on all IUserScopedRequest implementations"
```

---

## Chunk 2: Frontend — Remove `UserId` from Models and Mappings

### Task 3: Remove `UserId` from `AiSettingsModel`

**Files:**
- Modify: `Models/AiSettingsModel.cs`

- [ ] **Step 1: Remove `UserId` property**

```csharp
// Models/AiSettingsModel.cs
using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;
using System.Collections.ObjectModel;

namespace AppTrack.Frontend.Models;

public partial class AiSettingsModel : ModelBase, IAiSettingsValidatable
{
    public int SelectedChatModelId { get; set; }
    public ObservableCollection<PromptParameterModel> PromptParameter { get; set; } = new ObservableCollection<PromptParameterModel>();
    public ObservableCollection<PromptModel> Prompts { get; set; } = new ObservableCollection<PromptModel>();

    IEnumerable<IPromptParameterValidatable> IAiSettingsValidatable.PromptParameter => PromptParameter;
    IEnumerable<IPromptValidatable> IAiSettingsValidatable.Prompts => Prompts;
}
```

- [ ] **Step 2: Build models project**

```bash
dotnet build Models/AppTrack.Frontend.Models.csproj --configuration Release
```
Expected: Build succeeded (or errors in `ApiService/Mappings/AiSettingsMappings.cs` referencing `model.UserId` — fix in Task 5).

---

### Task 4: Remove `UserId` from `JobApplicationDefaultsModel`

**Files:**
- Modify: `Models/JobApplicationDefaultsModel.cs`

- [ ] **Step 1: Remove `UserId` property**

Read the file first, then remove the line `public string UserId { get; set; } = string.Empty;`.

- [ ] **Step 2: Build models project**

```bash
dotnet build Models/AppTrack.Frontend.Models.csproj --configuration Release
```
Expected: Build succeeded (or errors in mappings — fix in Task 5).

---

### Task 5: Update `AiSettingsMappings.cs`

**Files:**
- Modify: `ApiService/Mappings/AiSettingsMappings.cs`

- [ ] **Step 1: Replace file**

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

- [ ] **Step 2: Build ApiService**

```bash
dotnet build ApiService/ApiService.csproj --configuration Release
```
Expected: Build succeeded.

---

### Task 6: Update `JobApplicationDefaultsMappings.cs`

**Files:**
- Modify: `ApiService/Mappings/JobApplicationDefaultsMappings.cs`

- [ ] **Step 1: Replace file**

```csharp
// ApiService/Mappings/JobApplicationDefaultsMappings.cs
using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Mappings;

internal static class JobApplicationDefaultsMappings
{
    internal static JobApplicationDefaultsModel ToModel(this JobApplicationDefaultsDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name ?? string.Empty,
        Position = dto.Position ?? string.Empty,
        Location = dto.Location ?? string.Empty,
    };

    internal static UpdateJobApplicationDefaultsCommand ToUpdateCommand(this JobApplicationDefaultsModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Position = model.Position,
        Location = model.Location,
    };
}
```

- [ ] **Step 2: Build ApiService**

```bash
dotnet build ApiService/ApiService.csproj --configuration Release
```
Expected: Build succeeded.

---

### Task 7: Update `JobApplicationMappings.cs`

**Files:**
- Modify: `ApiService/Mappings/JobApplicationMappings.cs`

Remove `UserId = string.Empty` from `ToCreateCommand` and `ToUpdateCommand`. Read the file first to preserve all other properties exactly, then remove only the `UserId` lines (and the `// set by caller` comment).

- [ ] **Step 1: Remove `UserId` lines from both mapping methods**

- [ ] **Step 2: Build ApiService**

```bash
dotnet build ApiService/ApiService.csproj --configuration Release
```
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add Models/ ApiService/
git commit -m "feat: remove UserId from frontend models and command mappings"
```

---

## Chunk 3: NSwag Regeneration + Final Verification

### Task 8: Regenerate NSwag Client

**Files:**
- Modify: `ApiService/Base/ServiceClient.cs` (auto-generated — do not edit manually)
- Modify: `ApiService/Base/clientsettings.nswag` (embedded OpenAPI snapshot must be updated first)

**Background:** With `[JsonIgnore]` on `UserId`, Swashbuckle excludes it from the OpenAPI spec. `clientsettings.nswag` uses `fromDocument` mode — it contains an embedded OpenAPI JSON snapshot (not a live URL). The workflow is: run the API → fetch the updated Swagger JSON → replace the embedded snapshot → run NSwag code generation.

- [ ] **Step 1: Start the API**

In a separate terminal:
```bash
dotnet run --project AppTrack.Api/AppTrack.Api.csproj --launch-profile https
```
Wait until the API is running at `https://localhost:7273`. Check: `https://localhost:7273/health` should return `Healthy`.

- [ ] **Step 2: Update the embedded OpenAPI snapshot in `clientsettings.nswag`**

Fetch the new Swagger JSON and replace the `json` field in `clientsettings.nswag`.

**Option A (Visual Studio):** Right-click `ApiService/Base/clientsettings.nswag` → "Generate Files". This fetches the live spec, updates the snapshot, and regenerates `ServiceClient.cs` in one step. Skip Steps 3–4 if using this option.

**Option B (command line):**
```powershell
# Fetch new swagger JSON (run in PowerShell from solution root)
$swagger = Invoke-RestMethod -Uri "https://localhost:7273/swagger/v1/swagger.json"
$json = $swagger | ConvertTo-Json -Depth 50 -Compress
$nswag = Get-Content "ApiService/Base/clientsettings.nswag" -Raw | ConvertFrom-Json
$nswag.documentGenerator.fromDocument.json = $json
$nswag | ConvertTo-Json -Depth 50 | Set-Content "ApiService/Base/clientsettings.nswag"
```

- [ ] **Step 3: Run NSwag code generation**

```bash
cd ApiService/Base
dotnet tool run nswag run clientsettings.nswag /runtime:Net100
```

If the `nswag` tool is not installed locally:
```bash
nswag run clientsettings.nswag /runtime:Net100
```

Expected: `ServiceClient.cs` is regenerated. The generated command/query classes will no longer contain a `UserId` property.

- [ ] **Step 3: Build full solution**

```bash
cd ../..
dotnet build AppTrack.sln --configuration Release
```
Expected: **Build succeeded, 0 errors, 0 warnings.**

- [ ] **Step 4: Run all unit tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```
Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add ApiService/Base/ServiceClient.cs
git commit -m "feat: regenerate NSwag client — UserId removed from command/query contract"
```

---

## Final Verification

- [ ] **Step 1: Smoke test via Swagger UI**

Navigate to `https://localhost:7273/swagger`. Open any command endpoint (e.g., `POST /api/job-applications`). Verify that `userId` is **not** listed as a field in the request schema.

- [ ] **Step 2: Update `CLAUDE.md`**

Add the following note under the **Authentication** section in `CLAUDE.md`:

> `UserId` must never appear in frontend validators or frontend-to-command mappings. It is always set by the backend mediator from JWT claims (`IUserScopedRequest` + `Mediator.cs`). In the API contract, `UserId` is decorated with `[JsonIgnore]` on all `IUserScopedRequest` implementations to prevent client spoofing.

- [ ] **Step 3: Commit CLAUDE.md**

```bash
git add CLAUDE.md
git commit -m "docs: document UserId JWT-only policy in CLAUDE.md"
```
