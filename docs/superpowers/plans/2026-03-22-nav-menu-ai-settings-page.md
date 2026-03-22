# Nav Menu + AI Settings Page Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the single-item drawer with a full `MudNavMenu` (Applications, Archived/Recruiters as disabled placeholders, AI Settings) and convert the AI Settings dialog into a dedicated page at `/ai-settings`.

**Architecture:** `MainLayout` gains a `MudNavMenu` and auto-closes the drawer on navigation via `IDisposable` + `NavigationManager.LocationChanged`. A new `AiSettings` page replaces `AiSettingsDialog` — same logic, no dialog chrome, no Cancel button. The dialog files are deleted.

**Tech Stack:** Blazor WASM, MudBlazor, .NET 10. No backend changes.

**Spec:** `docs/superpowers/specs/2026-03-22-nav-menu-ai-settings-page-design.md`

---

## Chunk 1: Nav Menu + AI Settings Page

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Layout/MainLayout.razor`
- Modify: `AppTrack.BlazorUi/Components/Layout/MainLayout.razor.cs`
- Create: `AppTrack.BlazorUi/Components/Pages/AiSettings.razor`
- Create: `AppTrack.BlazorUi/Components/Pages/AiSettings.razor.cs`
- Delete: `AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor`
- Delete: `AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor.cs`

Build command: `dotnet build AppTrack.BlazorUi/AppTrack.BlazorUi.csproj --configuration Release`

---

### Task 1: Update `MainLayout.razor` — replace nav link with full menu

- [ ] **Modify `AppTrack.BlazorUi/Components/Layout/MainLayout.razor`** — replace the existing `<MudNavMenu>` block (lines 56–61) with:

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

`Match="NavLinkMatch.All"` on Applications ensures it only highlights on exact `/`.
`Disabled="true"` on Archived and Recruiters — no `Href`, not clickable.

---

### Task 2: Update `MainLayout.razor.cs` — IDisposable + remove dialog logic

- [ ] **Replace entire content of `AppTrack.BlazorUi/Components/Layout/MainLayout.razor.cs`** with:

```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Layout;

public partial class MainLayout : IDisposable
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    internal static readonly MudTheme AzureTheme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#0078D4",
            PrimaryDarken = "#005A9E",
            PrimaryLighten = "#50A0D8",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#637381",
            SecondaryContrastText = "#FFFFFF",
            AppbarBackground = "#0078D4",
            AppbarText = "#FFFFFF",
            Background = "#F0F3F7",
            BackgroundGray = "#E4E8EE",
        }
    };

    private bool _drawerOpen = false;

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += OnLocationChanged;
    }

    private void ToggleDrawer() => _drawerOpen = !_drawerOpen;

    private void Login() => Navigation.NavigateToLogin("authentication/login");

    private void Logout() => Navigation.NavigateToLogout("authentication/logout");

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _drawerOpen = false;
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
    }
}
```

Changes vs original:
- Removed: `using AppTrack.BlazorUi.Components.Dialogs;`
- Removed: `[Inject] IDialogService`
- Removed: `_aiSettingsDialogOptions` field
- Removed: `OpenAiSettingsDialogAsync()` method
- Added: `IDisposable`, `OnInitialized`, `OnLocationChanged`, `Dispose`
- Added: `using Microsoft.AspNetCore.Components.Routing;`

---

### Task 3: Create `AiSettings.razor`

- [ ] **Create `AppTrack.BlazorUi/Components/Pages/AiSettings.razor`** with this content (ported from `AiSettingsDialog.razor`, dialog chrome removed, page header and Save row added):

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
        <MudStack AlignItems="AlignItems.Center" Class="py-8">
            <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
            <MudText Typo="Typo.body2" Color="Color.Secondary">Loading settings…</MudText>
        </MudStack>
    }
    else
    {
        <MudGrid Spacing="2">
            <!-- Chat Model -->
            <MudItem xs="12" sm="6">
                <MudSelect T="ChatModel"
                           Label="Chat Model"
                           Value="_selectedChatModel"
                           ValueChanged="@OnChatModelChanged"
                           ToStringFunc="@(m => m?.Name ?? string.Empty)"
                           Variant="Variant.Outlined"
                           AdornmentIcon="@Icons.Material.Outlined.Psychology"
                           Class="mb-1">
                    @foreach (var model in _chatModels)
                    {
                        <MudSelectItem T="ChatModel" Value="@model">
                            <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="1">
                                <MudText>@model.Name</MudText>
                                @if (!string.IsNullOrWhiteSpace(model.Description))
                                {
                                    <MudText Typo="Typo.caption" Color="Color.Secondary">— @model.Description</MudText>
                                }
                            </MudStack>
                        </MudSelectItem>
                    }
                </MudSelect>
            </MudItem>
            <!-- Prompts -->
            <MudItem xs="12">
                <MudStack Row="true" AlignItems="AlignItems.Center" Class="mb-2">
                    <MudText Typo="Typo.subtitle1">Prompts</MudText>
                    <MudSpacer />
                    <MudButton Variant="Variant.Outlined"
                               Color="Color.Primary"
                               StartIcon="@Icons.Material.Filled.Add"
                               Size="Size.Small"
                               OnClick="AddPromptAsync">
                        Add
                    </MudButton>
                </MudStack>
                @if (_model.Prompts.Count == 0)
                {
                    <MudText Typo="Typo.body2" Color="Color.Secondary" Class="ml-1">
                        No prompts defined.
                    </MudText>
                }
                else
                {
                    <MudPaper Outlined="true" Class="pa-0" Style="max-height: 260px; overflow-y: auto;">
                        @foreach (var prompt in _model.Prompts)
                        {
                            var captured = prompt;
                            <MudStack Row="true"
                                      AlignItems="AlignItems.Center"
                                      Class="pa-3"
                                      Style="border-bottom: 1px solid var(--mud-palette-divider);">
                                <MudStack Spacing="0" Style="flex: 1; min-width: 0;">
                                    <MudText Typo="Typo.subtitle2" Style="word-break: break-word;">@captured.Name</MudText>
                                    <MudText Typo="Typo.body2"
                                             Color="Color.Secondary"
                                             Style="word-break: break-word; white-space: pre-wrap;">@captured.PromptTemplate</MudText>
                                </MudStack>
                                <MudStack Row="true" Spacing="0">
                                    <MudTooltip Text="Edit">
                                        <MudIconButton Icon="@Icons.Material.Filled.Edit"
                                                       Size="Size.Small"
                                                       Color="Color.Primary"
                                                       OnClick="@(() => EditPromptAsync(captured))" />
                                    </MudTooltip>
                                    <MudTooltip Text="Delete">
                                        <MudIconButton Icon="@Icons.Material.Filled.Delete"
                                                       Size="Size.Small"
                                                       Color="Color.Error"
                                                       OnClick="@(() => DeletePrompt(captured))" />
                                    </MudTooltip>
                                </MudStack>
                            </MudStack>
                        }
                    </MudPaper>
                }
            </MudItem>
            <!-- Prompt Parameters -->
            <MudItem xs="12">
                <MudStack Row="true" AlignItems="AlignItems.Center" Class="mb-2">
                    <MudText Typo="Typo.subtitle1">Prompt Parameters</MudText>
                    <MudSpacer />
                    <MudButton Variant="Variant.Outlined"
                               Color="Color.Primary"
                               StartIcon="@Icons.Material.Filled.Add"
                               Size="Size.Small"
                               OnClick="AddPromptParameterAsync">
                        Add
                    </MudButton>
                </MudStack>
                @if (_model.PromptParameter.Count == 0)
                {
                    <MudText Typo="Typo.body2" Color="Color.Secondary" Class="ml-1">
                        No prompt parameters defined.
                    </MudText>
                }
                else
                {
                    <MudPaper Outlined="true" Class="pa-0" Style="max-height: 260px; overflow-y: auto;">
                        @foreach (var param in _model.PromptParameter)
                        {
                            var captured = param;
                            <MudStack Row="true"
                                      AlignItems="AlignItems.Center"
                                      Class="pa-3"
                                      Style="border-bottom: 1px solid var(--mud-palette-divider);">
                                <MudStack Spacing="0" Style="flex: 1; min-width: 0;">
                                    <MudText Typo="Typo.subtitle2" Style="word-break: break-word;">@captured.Key</MudText>
                                    <MudText Typo="Typo.body2"
                                             Color="Color.Secondary"
                                             Style="word-break: break-word; white-space: pre-wrap;">@captured.Value</MudText>
                                </MudStack>
                                <MudStack Row="true" Spacing="0">
                                    <MudTooltip Text="Edit">
                                        <MudIconButton Icon="@Icons.Material.Filled.Edit"
                                                       Size="Size.Small"
                                                       Color="Color.Primary"
                                                       OnClick="@(() => EditPromptParameterAsync(captured))" />
                                    </MudTooltip>
                                    <MudTooltip Text="Delete">
                                        <MudIconButton Icon="@Icons.Material.Filled.Delete"
                                                       Size="Size.Small"
                                                       Color="Color.Error"
                                                       OnClick="@(() => DeletePromptParameter(captured))" />
                                    </MudTooltip>
                                </MudStack>
                            </MudStack>
                        }
                    </MudPaper>
                }
            </MudItem>
        </MudGrid>

        <MudStack Row="true" JustifyContent="Justify.FlexEnd" Class="mt-4">
            <MudButton Variant="Variant.Filled"
                       Color="Color.Primary"
                       StartIcon="@Icons.Material.Filled.Save"
                       OnClick="SubmitAsync"
                       Disabled="_isBusy">
                @if (_isBusy)
                {
                    <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
                    <span>Saving…</span>
                }
                else
                {
                    <span>Save</span>
                }
            </MudButton>
        </MudStack>
    }
</MudContainer>
```

---

### Task 4: Create `AiSettings.razor.cs`

- [ ] **Create `AppTrack.BlazorUi/Components/Pages/AiSettings.razor.cs`** with this content (ported from `AiSettingsDialog.razor.cs`, `IMudDialogInstance` and `Cancel()` removed, `MudDialog.Close(...)` removed from `SubmitAsync`):

```csharp
using AppTrack.BlazorUi.Components.Dialogs;
using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Pages;

public partial class AiSettings
{
    [Inject] private IAiSettingsService AiSettingsService { get; set; } = null!;
    [Inject] private IChatModelsService ChatModelsService { get; set; } = null!;
    [Inject] private IModelValidator<AiSettingsModel> ModelValidator { get; set; } = null!;
    [Inject] private IErrorHandlingService ErrorHandlingService { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private static readonly DialogOptions _paramDialogOptions = new()
    {
        BackdropClick = false,
        MaxWidth = MaxWidth.Small,
        FullWidth = true,
    };

    private AiSettingsModel _model = new();
    private List<ChatModel> _chatModels = [];
    private ChatModel? _selectedChatModel;
    private bool _isLoading;
    private bool _isBusy;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        var settingsTask = AiSettingsService.GetForUserAsync();
        var chatModelsTask = ChatModelsService.GetChatModels();

        await Task.WhenAll(settingsTask, chatModelsTask);

        var settingsResponse = settingsTask.Result;
        var chatModelsResponse = chatModelsTask.Result;

        if (!ErrorHandlingService.HandleResponse(settingsResponse) ||
            !ErrorHandlingService.HandleResponse(chatModelsResponse))
        {
            _isLoading = false;
            return;
        }

        _model = settingsResponse.Data ?? new AiSettingsModel();
        _chatModels = chatModelsResponse.Data ?? [];

        if (_model.SelectedChatModelId == 0 && _chatModels.Count > 0)
            _model.SelectedChatModelId = _chatModels[0].Id;

        _selectedChatModel = _chatModels.FirstOrDefault(m => m.Id == _model.SelectedChatModelId);

        _isLoading = false;
    }

    private void OnChatModelChanged(ChatModel value)
    {
        _selectedChatModel = value;
    }

    private async Task AddPromptParameterAsync()
    {
        var parameters = new DialogParameters<PromptParameterDialog>
        {
            { x => x.SiblingParameters, _model.PromptParameter },
        };

        var dialog = await DialogService.ShowAsync<PromptParameterDialog>("", parameters, _paramDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not PromptParameterModel newParam) return;

        _model.PromptParameter.Add(newParam);
        await InvokeAsync(StateHasChanged);
    }

    private async Task EditPromptParameterAsync(PromptParameterModel param)
    {
        var parameters = new DialogParameters<PromptParameterDialog>
        {
            { x => x.ExistingParameter, param },
            { x => x.SiblingParameters, _model.PromptParameter },
        };

        var dialog = await DialogService.ShowAsync<PromptParameterDialog>("", parameters, _paramDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not PromptParameterModel updatedParam) return;

        param.Key = updatedParam.Key;
        param.Value = updatedParam.Value;

        await InvokeAsync(StateHasChanged);
    }

    private void DeletePromptParameter(PromptParameterModel param)
    {
        _model.PromptParameter.Remove(param);
    }

    private async Task AddPromptAsync()
    {
        var parameters = new DialogParameters<PromptDialog>
        {
            { x => x.SiblingPrompts, _model.Prompts },
        };

        var dialog = await DialogService.ShowAsync<PromptDialog>("", parameters, _paramDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not PromptModel newPrompt) return;

        _model.Prompts.Add(newPrompt);
        await InvokeAsync(StateHasChanged);
    }

    private async Task EditPromptAsync(PromptModel prompt)
    {
        var parameters = new DialogParameters<PromptDialog>
        {
            { x => x.ExistingPrompt, prompt },
            { x => x.SiblingPrompts, _model.Prompts },
        };

        var dialog = await DialogService.ShowAsync<PromptDialog>("", parameters, _paramDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not PromptModel updatedPrompt) return;

        prompt.Name = updatedPrompt.Name;
        prompt.PromptTemplate = updatedPrompt.PromptTemplate;

        await InvokeAsync(StateHasChanged);
    }

    private void DeletePrompt(PromptModel prompt)
    {
        _model.Prompts.Remove(prompt);
    }

    private async Task SubmitAsync()
    {
        _model.SelectedChatModelId = _selectedChatModel?.Id ?? _model.SelectedChatModelId;

        if (!ModelValidator.Validate(_model)) return;

        _isBusy = true;
        var response = await AiSettingsService.UpdateAsync(_model.Id, _model);
        _isBusy = false;
        await InvokeAsync(StateHasChanged);

        if (!ErrorHandlingService.HandleResponse(response)) return;

        ErrorHandlingService.ShowSuccess("AI settings saved successfully.");
    }
}
```

---

### Task 5: Delete dialog files, build, commit

- [ ] **Delete `AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor`**

```bash
rm AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor
rm AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor.cs
```

- [ ] **Build:**

```bash
dotnet build AppTrack.BlazorUi/AppTrack.BlazorUi.csproj --configuration Release
```

Expected: 0 errors, 0 warnings.

If there are errors: the most likely cause is a missing `using` directive or a stale reference to `AiSettingsDialog` somewhere. Search with:
```bash
grep -r "AiSettingsDialog" AppTrack.BlazorUi/ --include="*.cs" --include="*.razor"
```

- [ ] **Commit:**

```bash
git add AppTrack.BlazorUi/Components/Layout/MainLayout.razor \
        AppTrack.BlazorUi/Components/Layout/MainLayout.razor.cs \
        AppTrack.BlazorUi/Components/Pages/AiSettings.razor \
        AppTrack.BlazorUi/Components/Pages/AiSettings.razor.cs
git rm AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor \
       AppTrack.BlazorUi/Components/Dialogs/AiSettingsDialog.razor.cs
git commit -m "feat: add MudNavMenu to drawer, convert AI Settings dialog to page"
```
