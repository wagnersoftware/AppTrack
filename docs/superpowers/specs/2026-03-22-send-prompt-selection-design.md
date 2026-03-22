# Send Prompt Selection — Design Spec

**Date:** 2026-03-22
**Scope:** Blazor frontend only (AppTrack.BlazorUi). WPF is explicitly excluded.

## Overview

Rename the "Generate Text" action to "Send Prompt" in the Blazor UI and add a prompt selection dropdown to the generation dialog. Users can choose from their saved prompts before sending to the AI.

## Changes

### 1. Backend — New `GetPromptNamesQuery`

New query in `AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/`:

- `GetPromptNamesQuery` implements `IRequest<GetPromptNamesDto>` and `IUserScopedRequest`
- `GetPromptNamesDto` contains `List<string> Names`
- Handler loads `AiSettings` via `IAiSettingsRepository`, returns prompt names for the current user
- Validator: checks that AI settings exist for the user
- New API endpoint: `GET /api/applicationtext/prompt-names`
- Unit tests: handler + validator

### 2. Backend — Extend `GeneratePromptQuery`

- Add required `string PromptName` property to `GeneratePromptQuery`
- Validator: `PromptName` must not be empty (`NotEmpty()`)
- Handler: look up prompt by `PromptName` instead of `FirstOrDefault()`; if no prompt with that name exists, throw `BadRequestException`
- Update unit tests for the changed validator and handler

### 3. Frontend Service (`AppTrack.Frontend.ApiService`)

- `IApplicationTextService`: add `Task<Response<List<string>>> GetPromptNames()`
- `IApplicationTextService`: update `GeneratePrompt` signature to `GeneratePrompt(int jobApplicationId, string promptName)`
- `ApplicationTextService`: implement both accordingly
- Regenerate NSwag client after backend changes

### 4. Blazor — `Home.razor` / `Home.razor.cs`

- `Home.razor`: rename tooltip `"Generate Text"` → `"Send Prompt"`
- `Home.razor.cs` — `GenerateTextAsync`:
  1. Call `GetPromptNames()` before opening dialog
  2. If response fails or list is empty → show snackbar "Kein Prompt konfiguriert", do not open dialog
  3. If names available → pass names as `DialogParameters` to `GenerateTextDialog`

### 5. Blazor — `GenerateTextDialog`

**New parameter:**
- `[Parameter] public List<string> PromptNames { get; set; }`

**Initialization (`OnInitializedAsync`):**
- Auto-select `PromptNames.First()` as `_selectedPromptName`
- Call `GeneratePrompt(JobApplication.Id, _selectedPromptName)`
- On error → cancel dialog

**New state fields:**
- `string _selectedPromptName` — currently selected prompt name
- `bool _isReloadingPrompt` — true while reloading prompt after selection change

**On selection change (`OnPromptNameChangedAsync`):**
1. Set `_selectedPromptName = newName`, `_isReloadingPrompt = true`
2. Call `GeneratePrompt(JobApplication.Id, newName)`
3. On success: update `_prompt` and `_unusedKeys`
4. Set `_isReloadingPrompt = false`

**`Phase.PromptReady` UI (`GenerateTextDialog.razor`):**
- Add `MudSelect` above the text area, bound to `_selectedPromptName`, calls `OnPromptNameChangedAsync` on change
- `MudSelect` is disabled while `_isReloadingPrompt = true`
- Text area shows a loading overlay / disabled state while `_isReloadingPrompt = true`
- "Send Prompt" button remains disabled while `_isReloadingPrompt = true`

## Data Flow

```
Home.razor — "Send Prompt" button clicked
  → GetPromptNames()
      empty → snackbar "Kein Prompt konfiguriert", stop
      names → open GenerateTextDialog(PromptNames = names)
          → OnInitializedAsync: GeneratePrompt(id, names[0])
          → Phase.PromptReady: MudSelect + editable text area
          → user changes selection: GeneratePrompt(id, newName), text updates
          → user clicks "Send Prompt": GenerateApplicationText(prompt, id)
          → Phase.TextReady: read-only result, copy + close
```

## Testing

- Unit tests for `GetPromptNamesQueryHandler` and `GetPromptNamesQueryValidator`
- Unit tests for updated `GeneratePromptQueryValidator` (PromptName required)
- Unit tests for updated `GeneratePromptQueryHandler` (lookup by name, not-found case)

## Out of Scope

- WPF frontend — no changes
- Prompt creation/editing — existing AiSettings dialog unchanged
- Ordering or categorization of prompts in the dropdown
