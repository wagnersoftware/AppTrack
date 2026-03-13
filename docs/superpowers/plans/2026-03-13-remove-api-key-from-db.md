# Remove API Key from Database Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove the per-user OpenAI API key from the database and Blazor UI, sourcing it instead from application configuration (User Secrets / Azure Key Vault).

**Architecture:** `OpenAiOptions` gains an `ApiKey` property read at startup. `OpenAiApplicationTextGenerator` reads the key from `IOptions<OpenAiOptions>` in its constructor; the `SetApiKey()` method is removed. All layers (domain, application, shared validation, frontend models, Blazor UI, tests) that previously carried or validated the API key are cleaned up, and an EF Core migration drops the column.

**Tech Stack:** .NET 10, ASP.NET Core, EF Core, FluentValidation, MudBlazor, xUnit + Shouldly, Testcontainers

---

## Chunk 1: Backend Core (Infrastructure + Application + Domain + Persistence)

### Task 1: Remove `SetApiKey` from `IApplicationTextGenerator` and `OpenAiApplicationTextGenerator`

**Files:**
- Modify: `AppTrack.Application/Contracts/ApplicationTextGenerator/IApplicationTextGenerator.cs`
- Modify: `AppTrack.Infrastructure/ApplicationTextGeneration/Settings/OpenAiOptions.cs`
- Modify: `AppTrack.Infrastructure/ApplicationTextGeneration/OpenAiApplicationTextGenerator.cs`

- [ ] **Step 1: Add `ApiKey` to `OpenAiOptions`**

  Replace the contents of `AppTrack.Infrastructure/ApplicationTextGeneration/Settings/OpenAiOptions.cs`:

  ```csharp
  using System.ComponentModel.DataAnnotations;

  namespace AppTrack.Infrastructure.ApplicationTextGeneration;

  public class OpenAiOptions
  {
      [Required]
      [Url]
      public string ApiUrl { get; set; } = string.Empty;

      [Required]
      public int TimeoutInSeconds { get; set; }

      [Required]
      public string ApiKey { get; set; } = string.Empty;
  }
  ```

- [ ] **Step 2: Remove `SetApiKey` from the interface**

  Replace the contents of `AppTrack.Application/Contracts/ApplicationTextGenerator/IApplicationTextGenerator.cs`:

  ```csharp
  namespace AppTrack.Application.Contracts.ApplicationTextGenerator;

  public interface IApplicationTextGenerator
  {
      Task<string> GenerateApplicationTextAsync(string prompt, string modelName, CancellationToken cancellationToken = default);
  }
  ```

- [ ] **Step 3: Refactor `OpenAiApplicationTextGenerator` to read key from options**

  Replace the contents of `AppTrack.Infrastructure/ApplicationTextGeneration/OpenAiApplicationTextGenerator.cs`:

  ```csharp
  using AppTrack.Application.Contracts.ApplicationTextGenerator;
  using AppTrack.Infrastructure.ApplicationTextGeneration.OpAiModels;
  using Microsoft.Extensions.Options;
  using System.Net.Http.Headers;
  using System.Net.Http.Json;

  namespace AppTrack.Infrastructure.ApplicationTextGeneration;

  public class OpenAiApplicationTextGenerator : IApplicationTextGenerator
  {
      private readonly HttpClient _httpClient;
      private readonly string _apiKey;
      private readonly string _openAiUrl;

      public OpenAiApplicationTextGenerator(HttpClient httpClient, IOptions<OpenAiOptions> openAiOptions)
      {
          _httpClient = httpClient;
          _httpClient.Timeout = TimeSpan.FromSeconds(openAiOptions.Value.TimeoutInSeconds);
          _openAiUrl = openAiOptions.Value.ApiUrl ?? throw new InvalidOperationException("OpenAI API URL is not configured.");
          _apiKey = openAiOptions.Value.ApiKey ?? throw new InvalidOperationException("OpenAI API key is not configured.");
      }

      public async Task<string> GenerateApplicationTextAsync(string prompt, string modelName, CancellationToken cancellationToken = default)
      {
          var request = new HttpRequestMessage(HttpMethod.Post, _openAiUrl);
          request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

          request.Content = JsonContent.Create(new
          {
              model = modelName,
              messages = new[]
              {
                  new { role = "system", content = "You are an assistant that writes professional job applications." },
                  new { role = "user", content = prompt }
              },
              max_tokens = 400
          });

          var response = await _httpClient.SendAsync(request, cancellationToken);
          response.EnsureSuccessStatusCode();

          var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken);

          return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
      }
  }
  ```

- [ ] **Step 4: Add `ApiKey` placeholder to `appsettings.json`**

  In `AppTrack.Api/appsettings.json`, add a non-empty sentinel to the `OpenAiSettings` section. **Important:** `[Required]` rejects empty strings (`AllowEmptyStrings` defaults to `false`), and `.ValidateOnStart()` is already wired in `Program.cs`. An empty `""` would cause startup failure even before User Secrets are applied. Use a non-empty placeholder that must be overridden:

  ```json
  "OpenAiSettings": {
    "ApiUrl": "https://api.openai.com/v1/chat/completions",
    "TimeoutInSeconds": 90,
    "ApiKey": "replace-with-user-secret"
  }
  ```

  Override locally via User Secrets (takes precedence over `appsettings.json`):
  ```bash
  dotnet user-secrets set "OpenAiSettings:ApiKey" "sk-your-actual-key-here" --project AppTrack.Api
  ```

  In Azure Key Vault, create a secret named `OpenAiSettings--ApiKey` (double dash = section separator).

- [ ] **Step 5: Verify build**

  Run: `dotnet build AppTrack.sln --configuration Release`

  Expected: Build errors in `GenerateApplicationTextCommandHandler.cs` (calls `SetApiKey`) and `GenerateApplicationTextCommandValidator.cs` (accesses `aiSettings.ApiKey`) — these are fixed in Task 2 and Task 4.

---

### Task 2: Remove `SetApiKey` call from `GenerateApplicationTextCommandHandler` and `ApiKey` check from validator

**Files:**
- Modify: `AppTrack.Application/Features/AiSettings/Commands/GenerateApplicationText/GenerateApplicationTextCommandHandler.cs`
- Modify: `AppTrack.Application/Features/AiSettings/Commands/GenerateApplicationText/GenerateApplicationTextCommandValidator.cs`

- [ ] **Step 1: Remove `SetApiKey` call in handler**

  In `GenerateApplicationTextCommandHandler.cs`, remove line 47:
  ```
  _applicationTextGenerator.SetApiKey(aiSettings!.ApiKey);
  ```
  The surrounding block should become:
  ```csharp
  var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId);
  var chatModel = await _chatModelRepository.GetByIdAsync(aiSettings!.SelectedChatModelId);
  var chatModelName = chatModel!.ApiModelName;

  var generatedApplicationText = await _applicationTextGenerator.GenerateApplicationTextAsync(request.Prompt, chatModelName, cancellationToken);
  ```

- [ ] **Step 2: Remove `ApiKey` guard from `GenerateApplicationTextCommandValidator`**

  In `GenerateApplicationTextCommandValidator.cs`, remove lines 57–58:
  ```csharp
  if (string.IsNullOrWhiteSpace(aiSettings.ApiKey))
      context.AddFailure("ApiKey in AI settings is missing.");
  ```

- [ ] **Step 3: Verify build** — these two files should now compile without errors. Run: `dotnet build AppTrack.sln --configuration Release`

  Expected: Remaining build errors are in `AiSettings.cs` (domain) and files referencing `ApiKey` on `AiSettings` — fixed in Task 3+.

---

### Task 3: Remove `ApiKey` from Domain, Persistence, Application DTO/Command/Mappings, and fix integration test compilation

Removing `ApiKey` from `AiSettings`, `AiSettingsDto`, and `UpdateAiSettingsCommand` in this task will cause the integration test project to fail to compile (it references these properties). The test fixes are therefore included here alongside the source changes.

**Files:**
- Modify: `AppTrack.Domain/AiSettings.cs`
- Modify: `AppTrack.Persistance/Configurations/AiSettingsConfiguration.cs`
- Modify: `AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs`
- Modify: `AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/UpdateAiSettingsCommand.cs`
- Modify: `AppTrack.Application/Mappings/AiSettingsMappings.cs`
- Modify: `AppTrack.Api.IntegrationTests/SeedData/AiSettings/AiSettingsSeedsHelper.cs`
- Modify: `AppTrack.Api.IntegrationTests/AiSettingsControllerTests/UpdateAiSettingsTests.cs`

- [ ] **Step 1: Remove `ApiKey` from `AiSettings` domain entity**

  Replace `AppTrack.Domain/AiSettings.cs`:

  ```csharp
  using AppTrack.Domain.Common;

  namespace AppTrack.Domain;

  public class AiSettings : BaseEntity
  {
      public int SelectedChatModelId { get; set; }

      public string PromptTemplate { get; set; } = string.Empty;

      public string UserId { get; set; } = string.Empty;

      public ICollection<PromptParameter> PromptParameter { get; set; } = new List<PromptParameter>();
  }
  ```

- [ ] **Step 2: Remove `ApiKey` config from `AiSettingsConfiguration`**

  Replace the `Configure` method in `AppTrack.Persistance/Configurations/AiSettingsConfiguration.cs`:

  ```csharp
  public void Configure(EntityTypeBuilder<AiSettings> builder)
  {
      builder.HasMany(s => s.PromptParameter)
          .WithOne(p => p.AISettings)
          .HasForeignKey(p => p.AISettingsId)
          .OnDelete(DeleteBehavior.Cascade);
  }
  ```

- [ ] **Step 3: Remove `ApiKey` from `AiSettingsDto`**

  Replace `AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs`:

  ```csharp
  namespace AppTrack.Application.Features.AiSettings.Dto;

  public class AiSettingsDto
  {
      public int SelectedChatModelId { get; set; }
      public int Id { get; set; }
      public string PromptTemplate { get; set; } = string.Empty;
      public string UserId { get; set; } = string.Empty;
      public List<PromptParameterDto> PromptParameter { get; set; } = new List<PromptParameterDto>();
  }
  ```

- [ ] **Step 4: Remove `ApiKey` from `UpdateAiSettingsCommand`**

  Replace `AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/UpdateAiSettingsCommand.cs`:

  ```csharp
  using AppTrack.Application.Contracts.Mediator;
  using AppTrack.Application.Features.AiSettings.Dto;
  using AppTrack.Shared.Validation.Interfaces;

  namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

  public class UpdateAiSettingsCommand : IRequest<AiSettingsDto>, IAiSettingsValidatable, IUserScopedRequest
  {
      public int SelectedChatModelId { get; set; }
      public int Id { get; set; }
      public string PromptTemplate { get; set; } = string.Empty;
      public string UserId { get; set; } = string.Empty;
      public List<PromptParameterDto> PromptParameter { get; set; } = new List<PromptParameterDto>();

      IEnumerable<IPromptParameterValidatable> IAiSettingsValidatable.PromptParameter => PromptParameter;
  }
  ```

- [ ] **Step 5: Remove `ApiKey` from `AiSettingsMappings`**

  Replace `AppTrack.Application/Mappings/AiSettingsMappings.cs`:

  ```csharp
  using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
  using AppTrack.Application.Features.AiSettings.Dto;
  using AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;
  using AppTrack.Domain;

  namespace AppTrack.Application.Mappings;

  internal static class AiSettingsMappings
  {
      internal static AiSettings ToDomain(this GetAiSettingsByUserIdQuery query) => new()
      {
          UserId = query.UserId,
      };

      internal static void ApplyTo(this UpdateAiSettingsCommand command, AiSettings entity)
      {
          entity.SelectedChatModelId = command.SelectedChatModelId;
          entity.PromptTemplate = command.PromptTemplate;
          entity.UserId = command.UserId;
          entity.PromptParameter.Clear();
          foreach (var dto in command.PromptParameter)
          {
              entity.PromptParameter.Add(PromptParameter.Create(dto.Key, dto.Value));
          }
      }

      internal static AiSettingsDto ToDto(this AiSettings entity) => new()
      {
          Id = entity.Id,
          SelectedChatModelId = entity.SelectedChatModelId,
          PromptTemplate = entity.PromptTemplate,
          UserId = entity.UserId,
          PromptParameter = entity.PromptParameter.Select(p => p.ToDto()).ToList(),
      };

      internal static PromptParameterDto ToDto(this PromptParameter entity) => new()
      {
          Id = entity.Id,
          Key = entity.Key,
          Value = entity.Value,
      };
  }
  ```

- [ ] **Step 6: Fix `AiSettingsSeedsHelper` — remove `ApiKey` from seed data**

  In `AppTrack.Api.IntegrationTests/SeedData/AiSettings/AiSettingsSeedsHelper.cs`, remove the `ApiKey = "1234abc",` line from the `new Domain.AiSettings { ... }` initializer in `CreateAiSettingsForUserAsync`.

- [ ] **Step 7: Fix `UpdateAiSettingsTests` — remove `ApiKey` assignments**

  In `AppTrack.Api.IntegrationTests/AiSettingsControllerTests/UpdateAiSettingsTests.cs`, remove `ApiKey = "..."` from all `UpdateAiSettingsCommand` object initializers in every test **that will remain after Step 8** (i.e., all except `UpdateAiSettings_ShouldReturn400_WhenApiKeyExceedsMaxChars`):
  - `UpdateAiSettings_ShouldReturn400_WhenIdIsZero`
  - `UpdateAiSettings_ShouldReturn400_WhenAiSettingsDoesNotExist`
  - `UpdateAiSettings_ShouldReturn400_WhenAiSettingsUserMismatch`
  - `UpdateAiSettings_ShouldReturn200_WhenRequestIsValid`
  - `UpdateAiSettings_ShouldReturn400_WhenDuplicateKeysExist`
  - `UpdateAiSettings_ShouldReturn400_WhenValueIsEmpty`
  - `UpdateAiSettings_ShouldReturn400_WhenPromptParameterKeyIsEmpty`

  Also in `UpdateAiSettings_ShouldReturn200_WhenRequestIsValid`, remove the assertion:
  ```csharp
  result.ApiKey.ShouldBe(validRequest.ApiKey);
  ```

  You can leave the `ApiKey` assignment inside `UpdateAiSettings_ShouldReturn400_WhenApiKeyExceedsMaxChars` as-is — the entire method is deleted in Step 8.

- [ ] **Step 8: Delete `UpdateAiSettings_ShouldReturn400_WhenApiKeyExceedsMaxChars` test**

  Delete the entire test method (lines 223–256 in the current file). This test validates behavior (`MaximumLength(200)` on `ApiKey`) that no longer exists.

- [ ] **Step 9: Verify build**

  Run: `dotnet build AppTrack.sln --configuration Release`

  Expected: All backend and integration test projects compile. Remaining errors are in `AppTrack.Shared.Validation` and frontend projects — fixed in Task 4 and Task 6.

---

### Task 4: Remove `ApiKey` from Shared Validation

**Files:**
- Modify: `AppTrack.Shared.Validation/Interfaces/IAiSettingsValidatable.cs`
- Modify: `AppTrack.Shared.Validation/Validators/AiSettingsBaseValidator.cs`

- [ ] **Step 1: Remove `ApiKey` property from `IAiSettingsValidatable`**

  Replace `AppTrack.Shared.Validation/Interfaces/IAiSettingsValidatable.cs`:

  ```csharp
  namespace AppTrack.Shared.Validation.Interfaces;

  public interface IAiSettingsValidatable
  {
      IEnumerable<IPromptParameterValidatable> PromptParameter { get; }
  }
  ```

- [ ] **Step 2: Remove `ApiKey` rule from `AiSettingsBaseValidator`**

  Replace `AppTrack.Shared.Validation/Validators/AiSettingsBaseValidator.cs`:

  ```csharp
  using AppTrack.Shared.Validation.Interfaces;
  using FluentValidation;

  namespace AppTrack.Shared.Validation.Validators;

  public abstract class AiSettingsBaseValidator<T> : AbstractValidator<T>
      where T : IAiSettingsValidatable
  {
      protected AiSettingsBaseValidator()
      {
          RuleForEach(x => x.PromptParameter)
              .SetValidator(new PromptParameterItemValidator());

          RuleFor(x => x.PromptParameter)
              .Must(HaveUniqueKeys)
              .WithMessage("Each prompt parameter key must be unique.");
      }

      private static bool HaveUniqueKeys(IEnumerable<IPromptParameterValidatable> parameters)
      {
          var list = parameters?.ToList();
          if (list is null || list.Count == 0)
              return true;

          return list.Select(p => p.Key)
                     .GroupBy(k => k, StringComparer.OrdinalIgnoreCase)
                     .All(g => g.Count() == 1);
      }

      private sealed class PromptParameterItemValidator : PromptParameterBaseValidator<IPromptParameterValidatable>
      {
          public PromptParameterItemValidator() : base() { }
      }
  }
  ```

- [ ] **Step 3: Verify build**

  Run: `dotnet build AppTrack.sln --configuration Release`

  Expected: Build succeeds for backend projects. Remaining errors are in frontend projects (`Models/AiSettingsModel.cs`, `ApiService/Mappings/AiSettingsMappings.cs`).

---

### Task 5: Create EF Core migration to drop `ApiKey` column

**Files:**
- New: `AppTrack.Persistance/Migrations/<timestamp>_RemoveApiKeyFromAiSettings.cs` (auto-generated)

- [ ] **Step 1: Add the migration**

  Run from the solution root:

  ```bash
  dotnet ef migrations add RemoveApiKeyFromAiSettings \
    --project AppTrack.Persistance \
    --startup-project AppTrack.Api \
    --output-dir Migrations
  ```

- [ ] **Step 2: Verify the generated migration**

  Open the generated migration file. The `Up` method must contain:
  ```csharp
  migrationBuilder.DropColumn(
      name: "ApiKey",
      table: "AiSettings");
  ```
  The `Down` method must re-add the column (EF generates this automatically).

  If the migration contains unexpected changes, do **not** proceed — investigate the EF model snapshot.

- [ ] **Step 3: Build to verify migration compiles**

  Run: `dotnet build AppTrack.sln --configuration Release`

- [ ] **Step 4: Commit Chunk 1**

  ```bash
  git add AppTrack.Api/appsettings.json \
          AppTrack.Infrastructure/ApplicationTextGeneration/Settings/OpenAiOptions.cs \
          AppTrack.Infrastructure/ApplicationTextGeneration/OpenAiApplicationTextGenerator.cs \
          AppTrack.Application/Contracts/ApplicationTextGenerator/IApplicationTextGenerator.cs \
          AppTrack.Application/Features/AiSettings/Commands/GenerateApplicationText/GenerateApplicationTextCommandHandler.cs \
          AppTrack.Application/Features/AiSettings/Commands/GenerateApplicationText/GenerateApplicationTextCommandValidator.cs \
          AppTrack.Domain/AiSettings.cs \
          AppTrack.Persistance/Configurations/AiSettingsConfiguration.cs \
          AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs \
          AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/UpdateAiSettingsCommand.cs \
          AppTrack.Application/Mappings/AiSettingsMappings.cs \
          AppTrack.Shared.Validation/Interfaces/IAiSettingsValidatable.cs \
          AppTrack.Shared.Validation/Validators/AiSettingsBaseValidator.cs \
          AppTrack.Persistance/Migrations/ \
          AppTrack.Api.IntegrationTests/SeedData/AiSettings/AiSettingsSeedsHelper.cs \
          AppTrack.Api.IntegrationTests/AiSettingsControllerTests/UpdateAiSettingsTests.cs
  git commit -m "refactor: remove ApiKey from domain, application, shared validation, infra, and integration tests"
  ```

---

## Chunk 2: Frontend Models, API Service & Blazor UI

### Task 6: Remove `ApiKey` from frontend model and API service mappings

**Files:**
- Modify: `Models/AiSettingsModel.cs`
- Modify: `ApiService/Mappings/AiSettingsMappings.cs`

- [ ] **Step 1: Remove `ApiKey` from `AiSettingsModel`**

  Replace `Models/AiSettingsModel.cs`:

  ```csharp
  using AppTrack.Frontend.Models.Base;
  using AppTrack.Shared.Validation.Interfaces;
  using System.Collections.ObjectModel;

  namespace AppTrack.Frontend.Models;

  public partial class AiSettingsModel : ModelBase, IAiSettingsValidatable
  {
      public int SelectedChatModelId { get; set; }
      public string UserId { get; set; } = string.Empty;
      public string PromptTemplate { get; set; } = string.Empty;
      public ObservableCollection<PromptParameterModel> PromptParameter { get; set; } = new ObservableCollection<PromptParameterModel>();

      IEnumerable<IPromptParameterValidatable> IAiSettingsValidatable.PromptParameter => PromptParameter;
  }
  ```

- [ ] **Step 2: Remove `ApiKey` from `ApiService/Mappings/AiSettingsMappings.cs`**

  Replace `ApiService/Mappings/AiSettingsMappings.cs`:

  ```csharp
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
          PromptTemplate = dto.PromptTemplate ?? string.Empty,
          UserId = dto.UserId ?? string.Empty,
          PromptParameter = new ObservableCollection<PromptParameterModel>(
              (dto.PromptParameter ?? []).Select(p => p.ToModel())),
      };

      internal static PromptParameterModel ToModel(this PromptParameterDto dto) => new()
      {
          Id = dto.Id,
          Key = dto.Key ?? string.Empty,
          Value = dto.Value ?? string.Empty,
      };

      internal static UpdateAiSettingsCommand ToUpdateCommand(this AiSettingsModel model) => new()
      {
          Id = model.Id,
          SelectedChatModelId = model.SelectedChatModelId,
          PromptTemplate = model.PromptTemplate,
          UserId = model.UserId,
          PromptParameter = model.PromptParameter.Select(p => p.ToDto()).ToList(),
      };

      internal static PromptParameterDto ToDto(this PromptParameterModel model) => new()
      {
          Id = model.Id,
          Key = model.Key,
          Value = model.Value,
      };
  }
  ```

- [ ] **Step 3: Verify build**

  Run: `dotnet build AppTrack.sln --configuration Release`

  Expected: `AppTrack.Frontend.Models` and `AppTrack.Frontend.ApiService` compile. Remaining build errors (if any) are in `AppTrack.BlazorUi`.

---

### Task 7: Remove API key input from Blazor dialog

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor`
- Modify: `AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor.cs`

- [ ] **Step 1: Remove API key `<MudItem>` block from `AiSettingsDialog.razor`**

  Remove lines 19–34 (the `<!-- API Key -->` comment and the entire `<MudItem xs="12">` block containing the `MudTextField` for `ApiKey`):

  ```razor
  <!-- API Key -->
  <MudItem xs="12">
      <MudTextField T="string"
                    Label="API Key"
                    InputType="_apiKeyInputType"
                    Value="_model.ApiKey"
                    ValueChanged="@((string v) => OnApiKeyChanged(v))"
                    Error="@ModelValidator.Errors.ContainsKey(nameof(AiSettingsModel.ApiKey))"
                    ErrorText="@GetFirstError(nameof(AiSettingsModel.ApiKey))"
                    Variant="Variant.Outlined"
                    Adornment="Adornment.End"
                    AdornmentIcon="@_apiKeyInputIcon"
                    OnAdornmentClick="ToggleApiKeyVisibility"
                    UserAttributes="_apiKeyAttributes"
                    Class="mb-1" />
  </MudItem>
  ```

- [ ] **Step 2: Remove API key members from `AiSettingsDialog.razor.cs`**

  Remove the following from `AiSettingsDialog.razor.cs`:

  - Lines 36–41: `_apiKeyAttributes` static field (including the comment above it)
    ```csharp
    // Suppresses browser "Save password?" prompts while keeping the field masked.
    // "new-password" signals to the browser that this is a credential creation field,
    // not a login form, which prevents most browsers from offering to save it.
    private static readonly Dictionary<string, object> _apiKeyAttributes = new()
    {
        { "autocomplete", "new-password" },
    };
    ```
  - Line 43: `_apiKeyInputType` field
    ```csharp
    private InputType _apiKeyInputType = InputType.Password;
    ```
  - Line 44: `_apiKeyInputIcon` field
    ```csharp
    private string _apiKeyInputIcon = Icons.Material.Filled.VisibilityOff;
    ```
  - Lines 76–88: `ToggleApiKeyVisibility()` method
    ```csharp
    private void ToggleApiKeyVisibility()
    {
        if (_apiKeyInputType == InputType.Password)
        {
            _apiKeyInputType = InputType.Text;
            _apiKeyInputIcon = Icons.Material.Filled.Visibility;
        }
        else
        {
            _apiKeyInputType = InputType.Password;
            _apiKeyInputIcon = Icons.Material.Filled.VisibilityOff;
        }
    }
    ```
  - Lines 90–94: `OnApiKeyChanged()` method
    ```csharp
    private void OnApiKeyChanged(string value)
    {
        _model.ApiKey = value;
        ModelValidator.ResetErrors(nameof(AiSettingsModel.ApiKey));
    }
    ```

- [ ] **Step 3: Verify build**

  Run: `dotnet build AppTrack.sln --configuration Release`

  Expected: 0 errors, 0 warnings.

- [ ] **Step 4: Commit Chunk 2**

  ```bash
  git add Models/AiSettingsModel.cs \
          ApiService/Mappings/AiSettingsMappings.cs \
          AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor \
          AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor.cs
  git commit -m "refactor: remove ApiKey from frontend models and Blazor AI settings dialog"
  ```

---

## Chunk 3: Tests & Configuration

### Task 8: Fix `FakeAuthWebApplicationFactory` for integration test startup

The compilation fixes for the integration test files were done in Task 3. This task adds the `UseSetting` so the WebApplicationFactory can successfully start the API (which runs `ValidateOnStart` for `OpenAiOptions.ApiKey`).

**Files:**
- Modify: `AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeAuthWebApplicationFactory.cs`

- [ ] **Step 1: Supply `ApiKey` config for integration test startup**

  In `FakeAuthWebApplicationFactory.cs`, add `builder.UseSetting(...)` as the first line inside `ConfigureWebHost`, **before** `builder.ConfigureTestServices`:

  ```csharp
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
      builder.UseSetting("OpenAiSettings:ApiKey", "test-api-key-integration");

      builder.ConfigureTestServices(services =>
      {
          // Replace SQL with Testcontainer DB
          services.RemoveAll(typeof(DbContextOptions<AppTrackDatabaseContext>));

          services.AddDbContext<AppTrackDatabaseContext>(options =>
              options.UseSqlServer(_dbContainer.GetConnectionString()));

          // Add Fake Authentication
          services.AddAuthentication(options =>
          {
              options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
              options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
          })
          .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
              TestAuthHandler.AuthenticationScheme, _ => { });
      });
  }
  ```

- [ ] **Step 2: Verify build**

  Run: `dotnet build AppTrack.sln --configuration Release`

  Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Run unit tests**

  Run: `dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release`

  Expected: All tests pass.

---

### Task 9: Verify local API startup and commit Chunk 3

- [ ] **Step 1: Verify API starts locally**

  Ensure the User Secret is set (done in Task 1), then run:
  ```bash
  dotnet run --project AppTrack.Api
  ```
  Verify startup succeeds — no `ValidateOnStart` exception about missing `ApiKey`.

- [ ] **Step 2: Commit Chunk 3**

  ```bash
  git add AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeAuthWebApplicationFactory.cs
  git commit -m "fix: supply OpenAI ApiKey in integration test WebApplicationFactory"
  ```

---

### Task 10: Regenerate NSwag client and run all tests

**Files:**
- Modify: `AppTrack.Frontend.ApiService/` — NSwag-generated files (auto-generated, do not edit manually)

- [ ] **Step 1: Build the API project to ensure latest OpenAPI spec is up to date**

  Run: `dotnet build AppTrack.Api/AppTrack.Api.csproj --configuration Release`

- [ ] **Step 2: Regenerate the NSwag client**

  Find and run the NSwag generation command (check `AppTrack.Frontend.ApiService` project for a `.nswag` config file or a `nswag.json`):

  ```bash
  # If using nswag CLI:
  nswag run nswag.json
  # Or if it's configured as a build step, a clean rebuild triggers regeneration:
  dotnet build AppTrack.Frontend.ApiService/AppTrack.Frontend.ApiService.csproj --configuration Release
  ```

  Verify the generated `AiSettingsDto` and `UpdateAiSettingsCommand` types no longer contain `ApiKey`.

- [ ] **Step 3: Final full build**

  Run: `dotnet build AppTrack.sln --configuration Release`

  Expected: 0 errors, 0 warnings.

- [ ] **Step 4: Run all unit tests**

  Run: `dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release`

  Expected: All tests pass.

- [ ] **Step 5: Final commit**

  ```bash
  git add AppTrack.Frontend.ApiService/
  git commit -m "chore: regenerate NSwag client after ApiKey removal"
  ```

---

## Summary of Files Changed

| File | Change |
|------|--------|
| `AppTrack.Infrastructure/ApplicationTextGeneration/Settings/OpenAiOptions.cs` | Add `ApiKey` property |
| `AppTrack.Infrastructure/ApplicationTextGeneration/OpenAiApplicationTextGenerator.cs` | Read key from options, remove `SetApiKey` |
| `AppTrack.Application/Contracts/ApplicationTextGenerator/IApplicationTextGenerator.cs` | Remove `SetApiKey` |
| `AppTrack.Application/Features/AiSettings/Commands/GenerateApplicationText/GenerateApplicationTextCommandHandler.cs` | Remove `SetApiKey` call |
| `AppTrack.Application/Features/AiSettings/Commands/GenerateApplicationText/GenerateApplicationTextCommandValidator.cs` | Remove `ApiKey` null guard |
| `AppTrack.Domain/AiSettings.cs` | Remove `ApiKey` property |
| `AppTrack.Persistance/Configurations/AiSettingsConfiguration.cs` | Remove `ApiKey` max-length config |
| `AppTrack.Persistance/Migrations/<new>_RemoveApiKeyFromAiSettings.cs` | Drop `ApiKey` column |
| `AppTrack.Application/Features/AiSettings/Dto/AiSettingsDto.cs` | Remove `ApiKey` |
| `AppTrack.Application/Features/AiSettings/Commands/UpdateAiSettings/UpdateAiSettingsCommand.cs` | Remove `ApiKey` |
| `AppTrack.Application/Mappings/AiSettingsMappings.cs` | Remove `ApiKey` from mappings |
| `AppTrack.Shared.Validation/Interfaces/IAiSettingsValidatable.cs` | Remove `ApiKey` property |
| `AppTrack.Shared.Validation/Validators/AiSettingsBaseValidator.cs` | Remove `ApiKey` rule |
| `Models/AiSettingsModel.cs` | Remove `ApiKey` property |
| `ApiService/Mappings/AiSettingsMappings.cs` | Remove `ApiKey` from both mapping methods |
| `AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor` | Remove API key input block |
| `AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor.cs` | Remove 5 API key members |
| `AppTrack.Api/appsettings.json` | Add empty `ApiKey` placeholder (Task 1) |
| `AppTrack.Api.IntegrationTests/SeedData/AiSettings/AiSettingsSeedsHelper.cs` | Remove `ApiKey` from seed data (Task 3) |
| `AppTrack.Api.IntegrationTests/AiSettingsControllerTests/UpdateAiSettingsTests.cs` | Remove `ApiKey` assignments, delete max-chars test (Task 3) |
| `AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeAuthWebApplicationFactory.cs` | Add `UseSetting` for `ApiKey` (Task 8) |
| `AppTrack.Frontend.ApiService/` | Regenerate NSwag client |
