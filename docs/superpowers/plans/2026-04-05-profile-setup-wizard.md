# Profile Setup Wizard Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the Blazor UI surface for a freelancer profile setup wizard — a shared form component embedded in both a MudDialog and a standalone `/profile/setup` page.

**Architecture:** `FreelancerProfileForm` is a self-contained shared component owned by both `ProfileSetupDialog` and the `ProfileSetup` page. The model (`FreelancerProfileModel`) lives in `AppTrack.Frontend.Models` following the established pattern. No data is saved; the UI is purely presentational in this iteration.

**Tech Stack:** Blazor WebAssembly (.NET 10), MudBlazor 9.1.0, `AppTrack.Frontend.Models`, branch `feature/profile-setup-wizard`

**Spec:** `docs/superpowers/specs/2026-04-05-profile-setup-wizard-design.md`

> **Working directory:** All shell commands assume the repository root `C:\Users\danie\source\repos\AppTrack` as the current working directory.

> **Note on TDD:** This feature contains no business logic — it is pure UI surface. There are no handlers, validators, or service methods to unit test. Build verification (compile + visual inspection) replaces the red-green-refactor cycle here.

---

## Chunk 1: Model & Enum

### Task 1: `RemotePreference` enum

**Files:**
- Create: `Models/RemotePreference.cs`

The file goes directly in the `Models/` root, consistent with `StatusOption.cs` — no subfolder needed.

- [ ] **Step 1: Create the file**

```csharp
namespace AppTrack.Frontend.Models;

public enum RemotePreference
{
    Remote,
    Hybrid,
    OnSite
}
```

- [ ] **Step 2: Build to verify no errors**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 3: Commit**

```bash
git add Models/RemotePreference.cs
git commit -m "feat: add RemotePreference enum to Frontend.Models"
```

---

### Task 2: `FreelancerProfileModel`

**Files:**
- Create: `Models/FreelancerProfileModel.cs`

- [ ] **Step 1: Create the file**

`FreelancerProfileModel` intentionally does **not** extend `ModelBase` — it is a pure form model with no persisted identity in this iteration. When backend integration is added, update the model to extend `ModelBase`.

```csharp
namespace AppTrack.Frontend.Models;

public class FreelancerProfileModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal? DailyRate { get; set; }
    public decimal? HourlyRate { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public RemotePreference? WorkMode { get; set; }
    public string? Skills { get; set; }
}
```

- [ ] **Step 2: Build to verify no errors**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 3: Commit**

```bash
git add Models/FreelancerProfileModel.cs
git commit -m "feat: add FreelancerProfileModel to Frontend.Models"
```

---

## Chunk 2: FreelancerProfileForm Component

> **Prerequisite:** Chunk 1 must be completed first. `RemotePreference` and `FreelancerProfileModel` must exist in `AppTrack.Frontend.Models` before this chunk is implemented.

### Task 3: `FreelancerProfileForm` shared component

**Files:**
- Create: `AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor`
- Create: `AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor.cs`
- Modify: `AppTrack.BlazorUi/Components/_Imports.razor` — add Profile namespace

- [ ] **Step 1: Add Profile namespace to `_Imports.razor`**

Open `AppTrack.BlazorUi/Components/_Imports.razor` and add after the existing `@using AppTrack.BlazorUi.Components.Dialogs` line:

```razor
@using AppTrack.BlazorUi.Components.Profile
```

- [ ] **Step 2: Create `FreelancerProfileForm.razor.cs`**

```csharp
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;

namespace AppTrack.BlazorUi.Components.Profile;

public partial class FreelancerProfileForm
{
    private readonly FreelancerProfileModel _model = new();
    private string _selectedType = "Freelancer";
    private DateTime? _availableFrom;

    private void SelectFreelancer() => _selectedType = "Freelancer";

    private void OnAvailableFromChanged(DateTime? date)
    {
        _availableFrom = date;
        _model.AvailableFrom = date.HasValue ? DateOnly.FromDateTime(date.Value) : null;
    }

    private string GetCardStyle(string type) =>
        _selectedType == type
            ? "cursor: pointer; border: 2px solid var(--mud-palette-primary);"
            : "cursor: pointer;";
}
```

- [ ] **Step 3: Create `FreelancerProfileForm.razor`**

```razor
<MudStack Spacing="4">

    <!-- ── Section 1: Profile Type ─────────────────────────────── -->
    <div>
        <MudText Typo="Typo.subtitle1" Class="mb-2">Profiltyp</MudText>
        <MudGrid Spacing="2">
            <MudItem xs="12" sm="6">
                <MudCard @onclick="SelectFreelancer"
                         Style="@GetCardStyle("Freelancer")"
                         Elevation="2">
                    <MudCardContent>
                        <MudStack AlignItems="AlignItems.Center" Spacing="1">
                            <MudIcon Icon="@Icons.Material.Filled.WorkspacePremium"
                                     Size="Size.Large"
                                     Color="Color.Primary" />
                            <MudText Typo="Typo.h6">Freelancer</MudText>
                        </MudStack>
                    </MudCardContent>
                </MudCard>
            </MudItem>
            <MudItem xs="12" sm="6">
                <MudTooltip Text="Kommt bald">
                    <MudCard Style="pointer-events: none; opacity: 0.5;" Elevation="1">
                        <MudCardContent>
                            <MudStack AlignItems="AlignItems.Center" Spacing="1">
                                <MudIcon Icon="@Icons.Material.Filled.BusinessCenter"
                                         Size="Size.Large"
                                         Color="Color.Secondary" />
                                <MudText Typo="Typo.h6">Festangestellter</MudText>
                            </MudStack>
                        </MudCardContent>
                    </MudCard>
                </MudTooltip>
            </MudItem>
        </MudGrid>
    </div>

    <!-- ── Section 2: Freelancer Details ───────────────────────── -->
    <div>
        <MudText Typo="Typo.subtitle1" Class="mb-2">Persönliche Daten</MudText>
        <MudGrid Spacing="2">

            <!-- First / Last Name -->
            <MudItem xs="12" sm="6">
                <MudTextField T="string"
                              Label="Vorname"
                              @bind-Value="_model.FirstName"
                              Variant="Variant.Outlined"
                              Adornment="Adornment.Start"
                              AdornmentIcon="@Icons.Material.Outlined.Person" />
            </MudItem>
            <MudItem xs="12" sm="6">
                <MudTextField T="string"
                              Label="Nachname"
                              @bind-Value="_model.LastName"
                              Variant="Variant.Outlined"
                              Adornment="Adornment.Start"
                              AdornmentIcon="@Icons.Material.Outlined.Person" />
            </MudItem>

            <!-- Day Rate / Hourly Rate -->
            <MudItem xs="12" sm="6">
                <MudNumericField T="decimal?"
                                 Label="Tagessatz (€)"
                                 @bind-Value="_model.DailyRate"
                                 Variant="Variant.Outlined"
                                 Adornment="Adornment.Start"
                                 AdornmentIcon="@Icons.Material.Outlined.Euro" />
            </MudItem>
            <MudItem xs="12" sm="6">
                <MudNumericField T="decimal?"
                                 Label="Stundensatz (€)"
                                 @bind-Value="_model.HourlyRate"
                                 Variant="Variant.Outlined"
                                 Adornment="Adornment.Start"
                                 AdornmentIcon="@Icons.Material.Outlined.Euro" />
            </MudItem>

            <!-- Available From -->
            <MudItem xs="12">
                <MudDatePicker Label="Verfügbar ab"
                               Date="_availableFrom"
                               DateChanged="OnAvailableFromChanged"
                               Variant="Variant.Outlined"
                               DateFormat="dd.MM.yyyy"
                               Editable="true" />
            </MudItem>

            <!-- Remote Preference -->
            <MudItem xs="12">
                <MudSelect T="RemotePreference?"
                           Label="Remote-Präferenz"
                           Value="_model.WorkMode"
                           ValueChanged="@((RemotePreference? v) => _model.WorkMode = v)"
                           Variant="Variant.Outlined"
                           Clearable="true"
                           AdornmentIcon="@Icons.Material.Outlined.LocationOn">
                    <MudSelectItem T="RemotePreference?" Value="@((RemotePreference?)RemotePreference.Remote)">Remote</MudSelectItem>
                    <MudSelectItem T="RemotePreference?" Value="@((RemotePreference?)RemotePreference.Hybrid)">Hybrid</MudSelectItem>
                    <MudSelectItem T="RemotePreference?" Value="@((RemotePreference?)RemotePreference.OnSite)">Vor Ort</MudSelectItem>
                </MudSelect>
            </MudItem>

            <!-- Skills -->
            <MudItem xs="12">
                <MudTextField T="string"
                              Label="Skills"
                              @bind-Value="_model.Skills"
                              Variant="Variant.Outlined"
                              Lines="3"
                              Placeholder="z. B. C#, .NET, Azure, SQL" />
            </MudItem>

            <!-- CV Upload (decorative — no FilesChanged handler wired) -->
            <MudItem xs="12">
                <MudFileUpload T="IBrowserFile" Accept=".pdf,.doc,.docx">
                    <CustomContent>
                        <MudButton Variant="Variant.Outlined"
                                   StartIcon="@Icons.Material.Filled.AttachFile"
                                   Color="Color.Default"
                                   OnClick="@context.OpenFilePickerAsync">
                            Lebenslauf hochladen (PDF)
                        </MudButton>
                    </CustomContent>
                </MudFileUpload>
            </MudItem>

        </MudGrid>
    </div>

</MudStack>
```

- [ ] **Step 4: Build to verify no errors**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 5: Commit**

```bash
git add AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor \
        AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor.cs \
        AppTrack.BlazorUi/Components/_Imports.razor
git commit -m "feat: add FreelancerProfileForm shared component"
```

---

## Chunk 3: Dialog, Page & Nav Link

> **Prerequisite:** Chunk 2 must be completed first. `FreelancerProfileForm` must exist before the dialog and page can embed it.

### Task 4: `ProfileSetupDialog`

**Files:**
- Create: `AppTrack.BlazorUi/Components/Dialogs/ProfileSetupDialog.razor`
- Create: `AppTrack.BlazorUi/Components/Dialogs/ProfileSetupDialog.razor.cs`

- [ ] **Step 1: Create `ProfileSetupDialog.razor.cs`**

```csharp
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class ProfileSetupDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    private void Skip() => MudDialog.Cancel();
    private void Save() => MudDialog.Close();
}
```

- [ ] **Step 2: Create `ProfileSetupDialog.razor`**

```razor
<MudDialog Class="apptrack-dialog">
    <TitleContent>
        <MudText Typo="Typo.h6">
            <MudIcon Icon="@Icons.Material.Filled.AccountCircle" Class="mr-2" />
            Profil einrichten
        </MudText>
    </TitleContent>
    <DialogContent>
        <FreelancerProfileForm />
    </DialogContent>
    <DialogActions>
        <MudButton Variant="Variant.Text" OnClick="Skip">Überspringen</MudButton>
        <MudSpacer />
        <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="Save">Speichern</MudButton>
    </DialogActions>
</MudDialog>
```

- [ ] **Step 3: Build to verify no errors**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Commit**

```bash
git add AppTrack.BlazorUi/Components/Dialogs/ProfileSetupDialog.razor \
        AppTrack.BlazorUi/Components/Dialogs/ProfileSetupDialog.razor.cs
git commit -m "feat: add ProfileSetupDialog"
```

---

### Task 5: `ProfileSetup` page

**Files:**
- Create: `AppTrack.BlazorUi/Components/Pages/ProfileSetup.razor`
- Create: `AppTrack.BlazorUi/Components/Pages/ProfileSetup.razor.cs`

- [ ] **Step 1: Create `ProfileSetup.razor.cs`**

```csharp
using Microsoft.AspNetCore.Components;

namespace AppTrack.BlazorUi.Components.Pages;

public partial class ProfileSetup
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private void Save() => Navigation.NavigateTo("/");
}
```

- [ ] **Step 2: Create `ProfileSetup.razor`**

```razor
@page "/profile/setup"

<PageTitle>Profil einrichten - AppTrack</PageTitle>

<MudContainer MaxWidth="MaxWidth.Small" Class="mt-6">
    <MudStack Row="true" AlignItems="AlignItems.Center" Class="mb-4">
        <MudIcon Icon="@Icons.Material.Filled.AccountCircle" Class="mr-2" Color="Color.Primary" />
        <MudText Typo="Typo.h5">Profil einrichten</MudText>
    </MudStack>

    <FreelancerProfileForm />

    <MudStack Row="true" Justify="Justify.FlexEnd" Class="mt-4">
        <MudButton Variant="Variant.Filled"
                   Color="Color.Primary"
                   StartIcon="@Icons.Material.Filled.Save"
                   OnClick="Save">
            Speichern
        </MudButton>
    </MudStack>
</MudContainer>
```

- [ ] **Step 3: Build to verify no errors**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Commit**

```bash
git add AppTrack.BlazorUi/Components/Pages/ProfileSetup.razor \
        AppTrack.BlazorUi/Components/Pages/ProfileSetup.razor.cs
git commit -m "feat: add ProfileSetup page at /profile/setup"
```

---

### Task 6: Add nav link to `MainLayout`

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Layout/MainLayout.razor`

- [ ] **Step 1: Add nav link**

In `MainLayout.razor`, find the `<MudNavMenu>` block and add after the `<MudDivider Class="my-2" />` line and before the `AI Settings` link:

```razor
<MudNavLink Href="/profile/setup" Icon="@Icons.Material.Filled.AccountCircle">
    Mein Profil
</MudNavLink>
```

- [ ] **Step 2: Build to verify no errors**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 3: Final commit**

```bash
git add AppTrack.BlazorUi/Components/Layout/MainLayout.razor
git commit -m "feat: add Mein Profil nav link to MainLayout"
```
