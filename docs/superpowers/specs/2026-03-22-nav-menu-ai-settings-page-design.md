# Nav Menu + AI Settings Page — Design Spec

**Date:** 2026-03-22
**Scope:** Blazor frontend only (`AppTrack.BlazorUi`). Backend unchanged.

## Overview

Replace the single-item drawer with a full `MudNavMenu`, add disabled placeholder entries for Archived and Recruiters, and convert AI Settings from a dialog to a dedicated page at `/ai-settings`.

## Changes

### 1. `MainLayout.razor` — Drawer content

Replace the existing `MudNavLink` (which calls `OpenAiSettingsDialogAsync`) with a full nav menu:

```razor
<MudNavMenu>
    <MudNavLink Href="/" Icon="@Icons.Material.Filled.Work" Match="NavLinkMatch.All">
        Applications
    </MudNavLink>
    <MudNavLink Icon="@Icons.Material.Filled.Archive" Disabled="true">
        Archived
    </MudNavLink>
    <MudNavLink Icon="@Icons.Material.Filled.People" Disabled="true">
        Recruiters
    </MudNavLink>
    <MudDivider Class="my-2" />
    <MudNavLink Href="/ai-settings" Icon="@Icons.Material.Filled.SmartToy">
        AI Settings
    </MudNavLink>
</MudNavMenu>
```

- `Applications` uses `Match="NavLinkMatch.All"` so it only highlights on exact `/` (not on every sub-path).
- `Archived` and `Recruiters` are `Disabled="true"` — visible but not clickable, no `Href`.
- `AI Settings` navigates to `/ai-settings` via `Href`.

### 2. `MainLayout.razor.cs` — Remove dialog logic, close drawer on navigation

**Remove:**
- `using AppTrack.BlazorUi.Components.Dialogs;` (no longer references `AiSettingsDialog`)
- `[Inject] private IDialogService DialogService`
- `private static readonly DialogOptions _aiSettingsDialogOptions`
- `private async Task OpenAiSettingsDialogAsync()`

**Modify class declaration** — implement `IDisposable`:
```csharp
public partial class MainLayout : IDisposable
```

**Add:** Subscribe to `NavigationManager.LocationChanged` to close the drawer automatically when the user navigates:

```csharp
protected override void OnInitialized()
{
    Navigation.LocationChanged += OnLocationChanged;
}

private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
{
    _drawerOpen = false;
    InvokeAsync(StateHasChanged);
}

public void Dispose()
{
    Navigation.LocationChanged -= OnLocationChanged;
}
```

### 3. New `AiSettings.razor` — `@page "/ai-settings"`

**File:** `AppTrack.BlazorUi/Components/Pages/AiSettings.razor`

Structure: `MudContainer` wrapping the content previously inside `<DialogContent>`. The loading spinner and all three sections (Chat Model, Prompts, Prompt Parameters) are kept verbatim.

Action row (replaces `<DialogActions>`): a `MudStack Row` at the bottom with a Save button (same spinner/disabled logic as before). No Cancel button — the user navigates away via the browser back button or the nav menu.

Authorization is handled globally by `AuthorizeRouteView` in `Routes.razor` — no per-page `<AuthorizeView>` wrapper needed.

```razor
@page "/ai-settings"
<PageTitle>AI Settings - AppTrack</PageTitle>

<MudContainer MaxWidth="MaxWidth.Medium" Class="mt-6">
    <MudStack Row="true" AlignItems="AlignItems.Center" Class="mb-4">
        <MudIcon Icon="@Icons.Material.Filled.SmartToy" Class="mr-2" Color="Color.Primary" />
        <MudText Typo="Typo.h5">AI Settings</MudText>
    </MudStack>

    @if (_isLoading)
    {
        <!-- existing spinner markup -->
    }
    else
    {
        <!-- existing MudGrid content: Chat Model, Prompts, Prompt Parameters -->

        <MudStack Row="true" JustifyContent="Justify.FlexEnd" Class="mt-4">
            <MudButton Variant="Variant.Filled"
                       Color="Color.Primary"
                       StartIcon="@Icons.Material.Filled.Save"
                       OnClick="SubmitAsync"
                       Disabled="_isBusy">
                @if (_isBusy) { <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" /><span>Saving…</span> }
                else { <span>Save</span> }
            </MudButton>
        </MudStack>
    }
</MudContainer>
```

### 4. New `AiSettings.razor.cs`

**File:** `AppTrack.BlazorUi/Components/Pages/AiSettings.razor.cs`

Identical logic to `AiSettingsDialog.razor.cs` with these changes:

- Namespace: `AppTrack.BlazorUi.Components.Pages` (not `Dialogs`)
- Class name: `AiSettings` (not `AiSettingsDialog`)
- **Remove:** `[CascadingParameter] private IMudDialogInstance MudDialog`
- **Remove:** `Cancel()` method
- **Keep:** `_paramDialogOptions` (currently defined in `AiSettingsDialog.razor.cs` — move it to `AiSettings.razor.cs`; it is used by the sub-dialog methods, not by `MainLayout`)
- **`SubmitAsync()`:** Remove `MudDialog.Close(DialogResult.Ok(true))` — after save, just show the success snackbar and stay on the page (no navigation away)

All service injections, field declarations, and sub-dialog methods (`AddPromptAsync`, `EditPromptAsync`, etc.) are kept unchanged.

### 5. Delete `AiSettingsDialog`

Delete both:
- `AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor`
- `AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor.cs`

## Out of Scope

- Archived page implementation
- Recruiters page implementation
- Backend changes
- WPF frontend
