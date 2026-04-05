# Profile Setup Wizard — Design Spec

**Date:** 2026-04-05
**Scope:** Blazor UI only — no backend, no data persistence, UI surface only.

---

## Overview

Users can set up a freelancer profile via:

1. **`ProfileSetupDialog`** — a MudDialog (trigger mechanism added later).
2. **`/profile/setup`** — a standalone page for later corrections.

Both surfaces embed the same `FreelancerProfileForm` component to avoid duplication.

---

## New Files

```
Models/                                         ← AppTrack.Frontend.Models project
  FreelancerProfileModel.cs
  RemotePreference.cs

AppTrack.BlazorUi/
  Components/
    Profile/
      FreelancerProfileForm.razor
      FreelancerProfileForm.razor.cs
    Dialogs/
      ProfileSetupDialog.razor
      ProfileSetupDialog.razor.cs
    Pages/
      ProfileSetup.razor
      ProfileSetup.razor.cs
```

---

## Data Model — `FreelancerProfileModel`

Lives in `AppTrack.Frontend.Models`, following the established pattern for all frontend models.

| Field | Type | Required |
|---|---|---|
| `FirstName` | `string` | yes |
| `LastName` | `string` | yes |
| `DailyRate` | `decimal?` | no |
| `HourlyRate` | `decimal?` | no |
| `AvailableFrom` | `DateOnly?` | no |
| `WorkMode` | `RemotePreference?` (enum) | no |
| `Skills` | `string?` | no |

`RemotePreference` enum (defined alongside the model in `AppTrack.Frontend.Models`):
- `Remote`
- `Hybrid`
- `OnSite`

**No validator** is created in this iteration. The "Speichern" button performs no API call and saves no data, so there is nothing to validate. A validator will be added alongside the backend integration.

No CV file property — the upload input is purely decorative in this iteration.

---

## `FreelancerProfileForm` Component

A shared Razor component. It owns a **private** `FreelancerProfileModel` instance and a **private** `string _selectedType = "Freelancer"` field for the type tile selection. No `[Parameter]` properties are exposed — the component is self-contained since no data flows in or out in this iteration.

### Section 1 — Profile Type

Two `MudCard` tiles side by side:

- **Freelancer** — active, selected by default (`_selectedType == "Freelancer"`), highlighted with primary color border.
- **Festangestellter** — visually disabled (`pointer-events: none; opacity: 0.5`), with a `MudTooltip` "Kommt bald".

Clicking the Freelancer tile sets `_selectedType = "Freelancer"` (no-op since it is already selected and it is the only active option).

### Section 2 — Freelancer Details

Always visible (only profile type available is Freelancer).

| Field | Component | Notes |
|---|---|---|
| First name | `MudTextField` | Left of two-column row |
| Last name | `MudTextField` | Right of two-column row |
| Day rate | `MudNumericField<decimal?>` | Left of two-column row, optional |
| Hourly rate | `MudNumericField<decimal?>` | Right of two-column row, optional |
| Available from | `MudDatePicker` | Full width, optional |
| Remote preference | `MudSelect<RemotePreference?>` | Full width, optional; options: Remote / Hybrid / Vor Ort |
| Skills | `MudTextField` (multiline) | Full width, optional, free text, e.g. "C#, .NET, Azure, SQL" |
| CV upload | `MudFileUpload` | Decorative — no `FilesChanged` handler wired, no file stored |

---

## `ProfileSetupDialog`

- `MaxWidth.Medium`
- No close (X) button — user must explicitly act
- **Actions:** "Überspringen" (calls `MudDialog.Cancel()`) | "Speichern" (calls `MudDialog.Close()`, no data saved)
- Title: "Profil einrichten"
- Trigger logic: not implemented in this iteration

---

## `ProfileSetup` Page (`/profile/setup`)

- `@page "/profile/setup"`
- No explicit `@layout` directive — uses `MainLayout` (default for authenticated pages)
- `MudContainer MaxWidth="MaxWidth.Small"` centered on the page
- Title: "Profil einrichten" (`MudText Typo="Typo.h5"`)
- Contains `<FreelancerProfileForm />`
- **Action:** "Speichern" button below the form — navigates to `/` (no data saved)

---

## Out of Scope (this iteration)

- First-login detection / auto-trigger of the dialog
- Persisting any profile data (backend)
- CV file upload processing
- Employee profile type
- Form validation

---

## Future Integration Points

- Auto-trigger: after successful MSAL login, check backend for existing profile; open `ProfileSetupDialog` if none exists.
- Backend: add `CreateFreelancerProfileCommand` + handler + persistence.
- Validation: add `FreelancerProfileModelValidator` following the `IModelValidator<T>` pattern.
- CV: add file upload endpoint, store in blob storage or DB.
