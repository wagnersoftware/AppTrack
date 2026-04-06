# Profile Form: English UI, Rate Toggle, Language Selector — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Translate the freelancer profile UI to English, replace the dual rate fields with a single toggle-driven rate input, and add a language selector for AI-generated application text.

**Architecture:** All changes are UI-only (Blazor WASM frontend + shared model project). Two new enum files are added to `AppTrack.Frontend.Models`; `FreelancerProfileModel` gains two new nullable properties; the form component is updated for the new UX and English strings.

**Tech Stack:** Blazor WASM (.NET 10), MudBlazor 7.x, `AppTrack.Frontend.Models` (shared model project at `Models/`)

**Spec:** `docs/superpowers/specs/2026-04-06-profile-form-english-rate-language-design.md`

---

## Chunk 1: Data model additions

### Task 1: Add `RateKind` enum

**Files:**
- Create: `Models/RateKind.cs`

- [ ] **Step 1: Create the file**

```csharp
namespace AppTrack.Frontend.Models;

public enum RateKind
{
    Hourly,
    Daily
}
```

File: `Models/RateKind.cs`

- [ ] **Step 2: Build to verify no errors**

```bash
dotnet build Models/AppTrack.Frontend.Models.csproj --configuration Release
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add Models/RateKind.cs
git commit -m "feat: add RateKind enum to Frontend.Models"
```

---

### Task 2: Add `ApplicationLanguage` enum

**Files:**
- Create: `Models/ApplicationLanguage.cs`

- [ ] **Step 1: Create the file**

```csharp
namespace AppTrack.Frontend.Models;

public enum ApplicationLanguage
{
    German,
    English
}
```

File: `Models/ApplicationLanguage.cs`

- [ ] **Step 2: Build**

```bash
dotnet build Models/AppTrack.Frontend.Models.csproj --configuration Release
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add Models/ApplicationLanguage.cs
git commit -m "feat: add ApplicationLanguage enum to Frontend.Models"
```

---

### Task 3: Extend `FreelancerProfileModel`

**Files:**
- Modify: `Models/FreelancerProfileModel.cs`

Current content for reference:
```csharp
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

- [ ] **Step 1: Add `SelectedRateType` and `Language` properties**

Replace the file content with:

```csharp
namespace AppTrack.Frontend.Models;

public class FreelancerProfileModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public RateKind? SelectedRateType { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? DailyRate { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public RemotePreference? WorkMode { get; set; }
    public string? Skills { get; set; }
    public ApplicationLanguage? Language { get; set; }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build Models/AppTrack.Frontend.Models.csproj --configuration Release
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add Models/FreelancerProfileModel.cs
git commit -m "feat: add SelectedRateType and Language to FreelancerProfileModel"
```

---

## Chunk 2: Form component — code-behind

### Task 4: Update `FreelancerProfileForm.razor.cs` with rate toggle logic

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor.cs`

Current content:
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

- [ ] **Step 1: Replace with updated code-behind**

```csharp
using AppTrack.Frontend.Models;

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

    private void OnRateKindChanged(RateKind? newKind)
    {
        if (_model.SelectedRateType == newKind) return;
        _model.SelectedRateType = newKind;
        if (newKind == RateKind.Hourly) _model.DailyRate = null;
        else if (newKind == RateKind.Daily) _model.HourlyRate = null;
        else { _model.HourlyRate = null; _model.DailyRate = null; }
    }

    private void OnRateValueChanged(decimal? value)
    {
        if (_model.SelectedRateType == RateKind.Hourly) _model.HourlyRate = value;
        else if (_model.SelectedRateType == RateKind.Daily) _model.DailyRate = value;
    }
}
```

- [ ] **Step 2: Build the BlazorUI project**

```bash
dotnet build AppTrack.BlazorUi/AppTrack.BlazorUi.csproj --configuration Release
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor.cs
git commit -m "feat: add OnRateKindChanged and OnRateValueChanged handlers to FreelancerProfileForm"
```

---

## Chunk 3: Form component — markup

### Task 5: Update `FreelancerProfileForm.razor` — English strings, rate toggle, language selector

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor`

- [ ] **Step 1: Replace the entire file content**

```razor
<MudStack Spacing="4">

    <!-- ── Section 1: Profile Type ─────────────────────────────── -->
    <div>
        <MudText Typo="Typo.subtitle1" Class="mb-2">Profile Type</MudText>
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
                <MudTooltip Text="Coming soon">
                    <MudCard Style="pointer-events: none; opacity: 0.5;" Elevation="1">
                        <MudCardContent>
                            <MudStack AlignItems="AlignItems.Center" Spacing="1">
                                <MudIcon Icon="@Icons.Material.Filled.BusinessCenter"
                                         Size="Size.Large"
                                         Color="Color.Secondary" />
                                <MudText Typo="Typo.h6">Employee</MudText>
                            </MudStack>
                        </MudCardContent>
                    </MudCard>
                </MudTooltip>
            </MudItem>
        </MudGrid>
    </div>

    <!-- ── Section 2: Personal Details ─────────────────────────── -->
    <div>
        <MudText Typo="Typo.subtitle1" Class="mb-2">Personal Details</MudText>
        <MudGrid Spacing="2">

            <!-- First / Last Name -->
            <MudItem xs="12" sm="6">
                <MudTextField T="string"
                              Label="First Name"
                              @bind-Value="_model.FirstName"
                              Variant="Variant.Outlined"
                              Adornment="Adornment.Start"
                              AdornmentIcon="@Icons.Material.Outlined.Person" />
            </MudItem>
            <MudItem xs="12" sm="6">
                <MudTextField T="string"
                              Label="Last Name"
                              @bind-Value="_model.LastName"
                              Variant="Variant.Outlined"
                              Adornment="Adornment.Start"
                              AdornmentIcon="@Icons.Material.Outlined.Person" />
            </MudItem>

            <!-- Rate Type Toggle -->
            <MudItem xs="12">
                <MudToggleGroup T="RateKind?" Value="_model.SelectedRateType"
                                ValueChanged="OnRateKindChanged"
                                SelectionMode="SelectionMode.ToggleSelection"
                                Color="Color.Primary" Outlined="true">
                    <MudToggleItem Value="@((RateKind?)RateKind.Hourly)">Hourly Rate</MudToggleItem>
                    <MudToggleItem Value="@((RateKind?)RateKind.Daily)">Daily Rate</MudToggleItem>
                </MudToggleGroup>
            </MudItem>

            <!-- Active Rate Field (hidden when no rate type selected) -->
            @if (_model.SelectedRateType != null)
            {
                <MudItem xs="12" sm="6">
                    <MudNumericField T="decimal?"
                                     Label="@(_model.SelectedRateType == RateKind.Hourly ? "Hourly Rate (€)" : "Daily Rate (€)")"
                                     Value="@(_model.SelectedRateType == RateKind.Hourly ? _model.HourlyRate : _model.DailyRate)"
                                     ValueChanged="OnRateValueChanged"
                                     Variant="Variant.Outlined"
                                     Adornment="Adornment.Start"
                                     AdornmentIcon="@Icons.Material.Outlined.Euro" />
                </MudItem>
            }

            <!-- Available From -->
            <MudItem xs="12">
                <MudDatePicker Label="Available From"
                               Date="_availableFrom"
                               DateChanged="OnAvailableFromChanged"
                               Variant="Variant.Outlined"
                               DateFormat="dd.MM.yyyy"
                               Editable="true" />
            </MudItem>

            <!-- Remote Preference -->
            <MudItem xs="12">
                <MudSelect T="RemotePreference?"
                           Label="Remote Preference"
                           Value="_model.WorkMode"
                           ValueChanged="@((RemotePreference? v) => _model.WorkMode = v)"
                           Variant="Variant.Outlined"
                           Clearable="true"
                           AdornmentIcon="@Icons.Material.Outlined.LocationOn">
                    <MudSelectItem T="RemotePreference?" Value="@((RemotePreference?)RemotePreference.Remote)">Remote</MudSelectItem>
                    <MudSelectItem T="RemotePreference?" Value="@((RemotePreference?)RemotePreference.Hybrid)">Hybrid</MudSelectItem>
                    <MudSelectItem T="RemotePreference?" Value="@((RemotePreference?)RemotePreference.OnSite)">On-Site</MudSelectItem>
                </MudSelect>
            </MudItem>

            <!-- Skills -->
            <MudItem xs="12">
                <MudTextField T="string"
                              Label="Skills"
                              @bind-Value="_model.Skills"
                              Variant="Variant.Outlined"
                              Lines="3"
                              Placeholder="e.g. C#, .NET, Azure, SQL" />
            </MudItem>

            <!-- Application Language -->
            <MudItem xs="12">
                <MudSelect T="ApplicationLanguage?"
                           Label="Application Language"
                           Value="_model.Language"
                           ValueChanged="@((ApplicationLanguage? v) => _model.Language = v)"
                           Variant="Variant.Outlined"
                           Clearable="true"
                           HelperText="Language for AI-generated application text"
                           AdornmentIcon="@Icons.Material.Outlined.Translate">
                    <MudSelectItem T="ApplicationLanguage?" Value="@((ApplicationLanguage?)ApplicationLanguage.German)">German</MudSelectItem>
                    <MudSelectItem T="ApplicationLanguage?" Value="@((ApplicationLanguage?)ApplicationLanguage.English)">English</MudSelectItem>
                </MudSelect>
            </MudItem>

            <!-- CV Upload (decorative — no FilesChanged handler wired) -->
            <MudItem xs="12">
                <MudFileUpload T="IBrowserFile" Accept=".pdf,.doc,.docx">
                    <CustomContent>
                        <MudButton Variant="Variant.Outlined"
                                   StartIcon="@Icons.Material.Filled.AttachFile"
                                   Color="Color.Default"
                                   OnClick="@context.OpenFilePickerAsync">
                            Upload CV (PDF)
                        </MudButton>
                    </CustomContent>
                </MudFileUpload>
            </MudItem>

        </MudGrid>
    </div>

</MudStack>
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.BlazorUi/AppTrack.BlazorUi.csproj --configuration Release
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor
git commit -m "feat: English UI, rate toggle, language selector in FreelancerProfileForm"
```

---

## Chunk 4: Remaining UI strings

### Task 6: Translate `ProfileSetupDialog.razor`

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Dialogs/ProfileSetupDialog.razor`

- [ ] **Step 1: Replace file content**

```razor
<MudDialog Class="apptrack-dialog">
    <TitleContent>
        <MudText Typo="Typo.h6">
            <MudIcon Icon="@Icons.Material.Filled.AccountCircle" Class="mr-2" />
            Set Up Profile
        </MudText>
    </TitleContent>
    <DialogContent>
        <FreelancerProfileForm />
    </DialogContent>
    <DialogActions>
        <MudButton Variant="Variant.Text" OnClick="Skip">Skip</MudButton>
        <MudSpacer />
        <MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="Save">Save</MudButton>
    </DialogActions>
</MudDialog>
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.BlazorUi/AppTrack.BlazorUi.csproj --configuration Release
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add AppTrack.BlazorUi/Components/Dialogs/ProfileSetupDialog.razor
git commit -m "feat: English strings in ProfileSetupDialog"
```

---

### Task 7: Translate `ProfileSetup.razor`

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Pages/ProfileSetup.razor`

- [ ] **Step 1: Replace file content**

```razor
@page "/profile/setup"

<PageTitle>My Profile - AppTrack</PageTitle>

<MudContainer MaxWidth="MaxWidth.Small" Class="mt-6">
    <MudStack Row="true" AlignItems="AlignItems.Center" Class="mb-4">
        <MudIcon Icon="@Icons.Material.Filled.AccountCircle" Class="mr-2" Color="Color.Primary" />
        <MudText Typo="Typo.h5">My Profile</MudText>
    </MudStack>

    <FreelancerProfileForm />

    <MudStack Row="true" Justify="Justify.FlexEnd" Class="mt-4">
        <MudButton Variant="Variant.Filled"
                   Color="Color.Primary"
                   StartIcon="@Icons.Material.Filled.Save"
                   OnClick="Save">
            Save
        </MudButton>
    </MudStack>
</MudContainer>
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.BlazorUi/AppTrack.BlazorUi.csproj --configuration Release
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add AppTrack.BlazorUi/Components/Pages/ProfileSetup.razor
git commit -m "feat: English strings in ProfileSetup page"
```

---

### Task 8: Translate `MainLayout.razor` nav link

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Layout/MainLayout.razor`

- [ ] **Step 1: Change `Mein Profil` to `My Profile`**

Find this line in `MainLayout.razor`:
```razor
                    <MudNavLink Href="/profile/setup" Icon="@Icons.Material.Filled.AccountCircle">
                        Mein Profil
                    </MudNavLink>
```

Replace with:
```razor
                    <MudNavLink Href="/profile/setup" Icon="@Icons.Material.Filled.AccountCircle">
                        My Profile
                    </MudNavLink>
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.BlazorUi/AppTrack.BlazorUi.csproj --configuration Release
```

Expected: `Build succeeded.`

- [ ] **Step 3: Final full-solution build**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add AppTrack.BlazorUi/Components/Layout/MainLayout.razor
git commit -m "feat: rename Mein Profil nav link to My Profile"
```
