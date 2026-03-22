# Send Prompt Selection — Design Spec

**Date:** 2026-03-22
**Scope:** Blazor frontend only (AppTrack.BlazorUi). WPF is explicitly excluded.

## Overview

Rename the "Generate Text" action to "Send Prompt" in the Blazor UI and add a prompt selection dropdown to the generation dialog. Users can choose from their saved prompts before sending to the AI.

## Changes

### 1. Backend — New `GetPromptNamesQuery`

New files in `AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/`:
- `GetPromptNamesQuery.cs` — implements `IRequest<GetPromptNamesDto>` and `IUserScopedRequest`
- `GetPromptNamesQueryHandler.cs`
- `GetPromptNamesQueryValidator.cs`

`GetPromptNamesDto` lives in `AppTrack.Application/Features/ApplicationText/Dto/GetPromptNamesDto.cs` and contains `List<string> Names`.

**Handler:** calls `_aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId)` (the only existing repository method), maps `aiSettings.Prompts.Select(p => p.Name).ToList()` to `GetPromptNamesDto.Names`. Order is insertion order (as stored in the database — no sorting applied). Handler assumes non-null `aiSettings` because the validator rejects null before the handler runs.

**Validator:** single `CustomAsync` rule — AI settings must exist for the user (calls `GetByUserIdIncludePromptParameterAsync`, fails if result is null).

**New API endpoint** added to `AiController` (`[Route("api/ai")]`):
```csharp
// GET /api/ai/prompt-names
[HttpGet("prompt-names")]
[ProducesResponseType(typeof(GetPromptNamesDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<ActionResult<GetPromptNamesDto>> GetPromptNames()
```

**Unit tests** in `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GetPromptNamesQueryHandlerTests.cs`:
- Handler returns correct list of names (in insertion order) when AiSettings exist with multiple prompts
- Handler returns empty list when AiSettings exist but `Prompts` collection is empty
- Validator fails when AiSettings do not exist for the user

### 2. Backend — Extend `GeneratePromptQuery`

- Add required `string PromptName` property to `GeneratePromptQuery`
- **Validator changes** — replace the existing `ValidateAiSettings` `CustomAsync` rule body with a single `CustomAsync` method that short-circuits on null:
  1. If `aiSettings == null` → add failure "AI settings not found for this user." and return
  2. If no prompt with `PromptName` exists in `aiSettings.Prompts` (case-sensitive) → add failure "Prompt not found in AI settings."
  3. If matched prompt's `PromptTemplate` is blank → add failure "Prompt template is empty."
  - Also add `RuleFor(x => x.PromptName).NotEmpty().WithMessage("{PropertyName} is required")` as a synchronous rule before the async rule
  - The old check `!aiSettings.Prompts.Any() || string.IsNullOrWhiteSpace(aiSettings.Prompts.First().PromptTemplate)` is **removed**
- **Handler:** replace `aiSettings!.Prompts.FirstOrDefault()?.PromptTemplate ?? string.Empty` with `aiSettings!.Prompts.First(p => p.Name == request.PromptName).PromptTemplate` (validator guarantees the named prompt exists)

**Updated unit tests** in `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/`:
- `GeneratePromptQueryValidatorTests`: add cases — `PromptName` empty fails; named prompt not found fails; matched prompt template blank fails
- `GeneratePromptQueryHandlerTests`: update `Handle_ShouldThrowBadRequestException_WhenNoPromptsConfigured` → rename/replace with `Handle_ShouldBuildPromptFromNamedTemplate` (passes a valid `PromptName` and asserts the correct template is used); the old test is no longer valid since the validator (not handler) now owns the "prompt not found" case

### 3. Frontend Service (`AppTrack.Frontend.ApiService`)

Files: `ApiService/Contracts/IApplicationTextService.cs` and `ApiService/Services/ApplicationTextService.cs`.

- `IApplicationTextService`: add `Task<Response<List<string>>> GetPromptNames()`
- `IApplicationTextService`: update `GeneratePrompt` signature to `GeneratePrompt(int jobApplicationId, string promptName)`
- `ApplicationTextService.GetPromptNames()`: calls `_client.GetPromptNamesAsync()` (NSwag-generated method, returns the NSwag-generated client type with `Names` as `ICollection<string>`), maps `dto.Names.ToList()`, wrapped in `TryExecuteAsync`
- `ApplicationTextService.GeneratePrompt(...)`: set `PromptName = promptName` on the `GeneratePromptQuery` body object
- **NSwag client regeneration** after backend changes covers: the new `GetPromptNamesAsync` method + the updated `GeneratePromptQuery` type (which now includes `PromptName`)

### 4. Blazor — `Home.razor` / `Home.razor.cs`

- `Home.razor`: rename tooltip `"Generate Text"` → `"Send Prompt"`; no other markup changes
- `Home.razor.cs`: add injection `[Inject] private IApplicationTextService ApplicationTextService { get; set; } = null!;`
- `Home.razor.cs` — `GenerateTextAsync`:
  1. Call `GetPromptNames()` before opening dialog
  2. If response fails → `ErrorHandlingService.HandleResponse(response)` shows snackbar, do not open dialog
  3. If response succeeds but list is empty → `ErrorHandlingService.ShowError("No prompt configured")`, do not open dialog
  4. If names available → pass names as `DialogParameters<GenerateTextDialog>` to `GenerateTextDialog` and open dialog

### 5. Blazor — `GenerateTextDialog`

`IApplicationTextService` remains injected in `GenerateTextDialog` — the dialog still calls `GeneratePrompt` directly (both on init and on selection change).

**New parameter:**
```csharp
[Parameter] public List<string> PromptNames { get; set; } = [];
```
`Home.razor.cs` guarantees the list is non-empty before the dialog opens, so `PromptNames.First()` in `OnInitializedAsync` is safe.

**New state fields:**
```csharp
private string _selectedPromptName = string.Empty;
private bool _isReloadingPrompt;
```

**Initialization (`OnInitializedAsync`):**
- `_phase` starts as `Phase.LoadingPrompt` (existing field initializer, unchanged)
- Set `_selectedPromptName = PromptNames.First()`
- Call `GeneratePrompt(JobApplication.Id, _selectedPromptName)`
- On error → cancel dialog; on success → set `_prompt`, `_unusedKeys`, `_phase = Phase.PromptReady` (same as today)

**On selection change (`OnPromptNameChangedAsync(string newName)`):**
1. Set `_selectedPromptName = newName`, `_isReloadingPrompt = true` (phase stays `PromptReady` — spinner is NOT shown; existing controls are visually disabled instead)
2. `StateHasChanged()`
3. Call `GeneratePrompt(JobApplication.Id, newName)` (no cancellation token — `MudSelect` disabled during reload prevents concurrent calls)
4. On error: `_isReloadingPrompt = false`, return (error shown via `ErrorHandlingService`)
5. On success: update `_prompt` and `_unusedKeys`, set `_isReloadingPrompt = false`

**`Phase.PromptReady` UI (`GenerateTextDialog.razor`):**
- Add `MudSelect<string>` above the text area, bound to `_selectedPromptName`, `ValueChanged="OnPromptNameChangedAsync"`, `Disabled="@_isReloadingPrompt"`
- Text area (`MudTextField`) gets `Disabled="@_isReloadingPrompt"`
- "Send Prompt" button gets `Disabled="@_isReloadingPrompt"`
- No change to dialog title (empty string, not visible)

## Data Flow

```
Home.razor — "Send Prompt" button clicked
  → GetPromptNames()
      error → snackbar via ErrorHandlingService, stop
      empty list → ErrorHandlingService.ShowError("No prompt configured"), stop
      names → open GenerateTextDialog(PromptNames = names)
          → Phase.LoadingPrompt: spinner shown
          → OnInitializedAsync: GeneratePrompt(id, names[0])
          → Phase.PromptReady: MudSelect pre-selected + editable text area
          → user changes selection: _isReloadingPrompt=true (controls disabled),
                                    GeneratePrompt(id, newName), text updates
          → user clicks "Send Prompt": GenerateApplicationText(prompt, id)
          → Phase.TextReady: read-only result, copy + close
```

## Testing

**New tests** in `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/`:
- `GetPromptNamesQueryHandlerTests.cs`: returns names in insertion order, returns empty list when Prompts is empty
- `GetPromptNamesQueryValidatorTests.cs`: fails when AiSettings not found

**Updated tests** in `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/`:
- `GeneratePromptQueryValidatorTests.cs`: add — `PromptName` empty fails; named prompt not found fails; matched prompt template blank fails
- `GeneratePromptQueryHandlerTests.cs`: update `Handle_ShouldThrowBadRequestException_WhenNoPromptsConfigured` → replace with test asserting handler uses the correct named template

## Out of Scope

- WPF frontend — no changes
- Prompt creation/editing — existing AiSettings dialog unchanged
- Ordering or categorization of prompts in the dropdown
