# Profile Form: English UI, Rate Type Toggle, Language Selector

**Date:** 2026-04-06
**Branch:** feature/profile-setup-wizard

## Overview

Three related changes to the freelancer profile feature in `AppTrack.BlazorUi`:

1. Translate all German UI strings in the profile area to English.
2. Replace the two independent rate fields (hourly + daily) with a toggle that lets the user pick one rate type, showing only the relevant input.
3. Add a `Language` property to `FreelancerProfileModel` so users can specify the language (German or English) for AI-generated application text.

---

## Scope

| File | Change |
|------|--------|
| `Models/RateKind.cs` *(new)* | New enum `RateKind { Hourly, Daily }` |
| `Models/ApplicationLanguage.cs` *(new)* | New enum `ApplicationLanguage { German, English }` |
| `Models/FreelancerProfileModel.cs` | Add `SelectedRateType`, `Language` properties |
| `AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor` | English labels, rate toggle UX, language selector |
| `AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor.cs` | Rate toggle state logic |
| `AppTrack.BlazorUi/Components/Dialogs/ProfileSetupDialog.razor` | English title and button labels |
| `AppTrack.BlazorUi/Components/Pages/ProfileSetup.razor` | English page title, heading, and Save button |
| `AppTrack.BlazorUi/Components/Layout/MainLayout.razor` | `Mein Profil` → `My Profile` |

---

## Data Model

### New enums (in `AppTrack.Frontend.Models`)

```csharp
public enum RateKind { Hourly, Daily }
public enum ApplicationLanguage { German, English }
```

Note: the enum is named `RateKind` (not `RateType`) to avoid the C# property-shadows-type-name issue when used as `public RateKind? SelectedRateType { get; set; }`.

### Updated `FreelancerProfileModel`

```csharp
public class FreelancerProfileModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Rate toggle: user picks one kind; only the active field is populated.
    public RateKind? SelectedRateType { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? DailyRate { get; set; }

    public DateOnly? AvailableFrom { get; set; }
    public RemotePreference? WorkMode { get; set; }
    public string? Skills { get; set; }

    // Language for AI-generated application text
    public ApplicationLanguage? Language { get; set; }
}
```

The two rate properties are kept so existing data round-trips cleanly. Only the active one is populated; switching the toggle clears the inactive rate.

---

## UI Changes

### `FreelancerProfileForm.razor`

**Section headers (translated):**
- `Profiltyp` → `Profile Type`
- `Persönliche Daten` → `Personal Details`

**Card labels:**
- `Festangestellter` → `Employee`
- `Kommt bald` tooltip → `Coming soon`

**Field labels:**
- `Vorname` → `First Name`
- `Nachname` → `Last Name`
- `Verfügbar ab` → `Available From`
- `Remote-Präferenz` → `Remote Preference`
- `Lebenslauf hochladen (PDF)` → `Upload CV (PDF)`
- Skills placeholder: `z. B. C#, .NET, Azure, SQL` → `e.g. C#, .NET, Azure, SQL`

**`RemotePreference` select items (display strings inside `MudSelectItem`):**
- `Remote` → `Remote` (unchanged)
- `Hybrid` → `Hybrid` (unchanged)
- `Vor Ort` → `On-Site`

**Rate selector (new UX):**
Replace the two independent `MudNumericField` components (`Tagessatz (€)` and `Stundensatz (€)` — **removed, not translated**) with:
1. A `MudToggleGroup<RateKind?>` using `SelectionMode="SelectionMode.ToggleSelection"` (allows deselection — clicking the active item returns `null`). Toggle items display the text `Hourly Rate` and `Daily Rate`.
2. A single `MudNumericField` shown **only when `SelectedRateType != null`**; its label and bound value switch based on the selection.
3. Switching the toggle: the `OnRateKindChanged` handler clears the inactive rate and updates `SelectedRateType`. Deselecting clears both rates and hides the field.

Example razor markup for the rate section:

```razor
<MudToggleGroup T="RateKind?" Value="_model.SelectedRateType"
                ValueChanged="OnRateKindChanged"
                SelectionMode="SelectionMode.ToggleSelection"
                Color="Color.Primary" Outlined="true">
    <MudToggleItem Value="@((RateKind?)RateKind.Hourly)">Hourly Rate</MudToggleItem>
    <MudToggleItem Value="@((RateKind?)RateKind.Daily)">Daily Rate</MudToggleItem>
</MudToggleGroup>

@if (_model.SelectedRateType != null)
{
    <MudNumericField T="decimal?"
                     Label="@(_model.SelectedRateType == RateKind.Hourly ? "Hourly Rate (€)" : "Daily Rate (€)")"
                     Value="@(_model.SelectedRateType == RateKind.Hourly ? _model.HourlyRate : _model.DailyRate)"
                     ValueChanged="OnRateValueChanged"
                     Variant="Variant.Outlined"
                     Adornment="Adornment.Start"
                     AdornmentIcon="@Icons.Material.Outlined.Euro" />
}
```

where `OnRateValueChanged(decimal? v)` sets `HourlyRate` or `DailyRate` based on `SelectedRateType`.

**Language selector (new field):**
A `MudSelect<ApplicationLanguage?>` after the Skills field:
- Label: `Application Language`
- Helper text: `Language for AI-generated application text`
- Options: `German`, `English`
- Clearable: `true`

### `ProfileSetupDialog.razor`

| Before | After |
|--------|-------|
| `Profil einrichten` | `Set Up Profile` |
| `Überspringen` | `Skip` |
| `Speichern` | `Save` |

### `ProfileSetup.razor`

| Before | After |
|--------|-------|
| `<PageTitle>Profil einrichten - AppTrack</PageTitle>` | `<PageTitle>My Profile - AppTrack</PageTitle>` |
| `<MudText Typo="Typo.h5">Profil einrichten</MudText>` | `<MudText Typo="Typo.h5">My Profile</MudText>` |
| `Speichern` (Save button) | `Save` |

### `MainLayout.razor`

| Before | After |
|--------|-------|
| `Mein Profil` | `My Profile` |

---

## Toggle Logic (`FreelancerProfileForm.razor.cs`)

```csharp
private void OnRateKindChanged(RateKind? newKind)
{
    if (_model.SelectedRateType == newKind) return;
    _model.SelectedRateType = newKind;
    // Clear the inactive rate; null clears both and hides the field
    if (newKind == RateKind.Hourly) _model.DailyRate = null;
    else if (newKind == RateKind.Daily) _model.HourlyRate = null;
    else { _model.HourlyRate = null; _model.DailyRate = null; }
}
```

**Initial state:** `SelectedRateType` is `null` — neither toggle option is pre-selected and the rate input is hidden.
**Deselection:** Clicking the active toggle option again returns `null` → both rate values cleared, input hidden.

---

## Out of Scope

- Backend API / command changes (no backend endpoint for profile yet).
- Validation rules (no validator files currently exist for `FreelancerProfileModel`).
- CV upload wiring (already decorative/non-functional).
- The `Employee` profile type card (already disabled/coming soon).
