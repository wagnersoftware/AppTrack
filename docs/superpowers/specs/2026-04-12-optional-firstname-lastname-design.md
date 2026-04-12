# Design: Make FirstName and LastName Optional on FreelancerProfile

**Date:** 2026-04-12
**Branch:** feature/profile-setup-wizard
**Status:** Approved

## Context

`FreelancerProfile.FirstName` and `FreelancerProfile.LastName` are currently required at the domain and database level. This prevents a user from uploading a CV without first providing their name. The intent is to allow CV upload as a standalone action with no other profile data required.

## Current State

| Layer | Type | Notes |
|---|---|---|
| `UpsertFreelancerProfileCommand` | `string?` | Already nullable — no change needed |
| `IFreelancerProfileValidatable` | `string?` | Already nullable — no change needed |
| `FreelancerProfileBaseValidator` | `When`-guard | Already optional — no change needed |
| `FreelancerProfileModel` (Frontend) | `string?` | Already nullable — no change needed |
| `FreelancerProfile` (Domain entity) | `string = ""` | Must change to `string?` |
| DB columns `FirstName`, `LastName` | `NOT NULL` | Must change to nullable |
| `FreelancerProfileDto` | `string = ""` | Must change to `string?` |
| Backend mappings (`?? string.Empty`) | Coerces null → `""` | Must remove coercion |
| Frontend ApiService mappings (`?? string.Empty`) | Coerces null → `""` | Must remove coercion |
| `ApiService/Base/ServiceClient.cs` (NSwag-generated) | `public string FirstName/LastName` | Must be manually patched to `string?` (see note) |

**NSwag note:** `clientsettings.nswag` has `"generateNullableReferenceTypes": false`. Regenerating the client will not produce `string?` and would also risk overwriting the patch. The correct approach is to **manually patch** lines 2153, 2156, 2481, and 2484 in `ServiceClient.cs` to `string?`. Do not change `generateNullableReferenceTypes` as it would affect all other generated types.

## Behaviour After Change

- A user can upload a CV without any profile data — `UploadCvCommandHandler` creates `new FreelancerProfile { UserId = ... }` with `FirstName = null`, `LastName = null`.
- `UpsertFreelancerProfile` with `FirstName = null` sets the name to `null` (full-replace semantics, consistent with existing upsert behaviour).
- All validation rules for `FirstName`/`LastName` remain in place (max length 100) and fire only when a value is provided.

## Affected Files

### Domain
- `AppTrack.Domain/FreelancerProfile.cs` — change `string FirstName/LastName` to `string?`

### Persistence
- `AppTrack.Persistance/Configurations/FreelancerProfileConfiguration.cs` — remove `IsRequired()` for both fields
- New EF Core migration — allow NULL on `FirstName` and `LastName` columns
- `AppTrack.Persistance/Migrations/AppTrackDatabaseContextModelSnapshot.cs` — auto-updated by `dotnet ef migrations add`; no manual edits needed

### Application
- `AppTrack.Application/Features/FreelancerProfile/Dto/FreelancerProfileDto.cs` — change `string FirstName/LastName` to `string?`
- `AppTrack.Application/Mappings/FreelancerProfileMappings.cs`:
  - `ToNewDomain` and `ApplyTo`: remove `?? string.Empty` coercion
  - `ToDto`: no code change needed; `FirstName = entity.FirstName` is a direct assignment that carries `string?` naturally once both sides are nullable

> **Note:** The domain entity (`FreelancerProfile.cs`) and the DTO (`FreelancerProfileDto.cs`) changes must be applied in the same commit. If the domain is changed to `string?` while the DTO still has `string`, the `ToDto` assignment `FirstName = entity.FirstName` becomes `string? → string`, which is a compile error under `Nullable=enable` + `TreatWarningsAsErrors`.

### Frontend ApiService
- `ApiService/Mappings/FreelancerProfileMappings.cs`:
  - `ToUpsertCommand`: remove `?? string.Empty` for `FirstName`/`LastName`
  - `ToModel`: no code change needed; `FirstName = dto.FirstName` is a direct assignment — safe in both directions (`string` → `string?` widens; `string?` → `string?` is identical)
- `ApiService/Base/ServiceClient.cs` — manually patch 4 lines to `string?`:
  - Line 2153: `FreelancerProfileDto.FirstName`
  - Line 2156: `FreelancerProfileDto.LastName`
  - Line 2481: `UpsertFreelancerProfileCommand.FirstName`
  - Line 2484: `UpsertFreelancerProfileCommand.LastName`

### Tests
- `AppTrack.Application.UnitTests/Features/FreelancerProfile/Commands/UpsertFreelancerProfileCommandHandlerTests.cs`:
  - Existing test `Handle_ShouldReturnDto_WhenCommandIsValid` asserts `result.FirstName.ShouldBe("Anna")`. Shouldly's `ShouldBe` works on `string?` — this assertion continues to compile and pass unchanged.
  - Existing test `Handle_ShouldCreate_WhenNoExistingProfile` already covers the create-path with non-null names; it remains valid unchanged.
  - **Add new test:** `Handle_ShouldCreate_WhenNamesAreNull` — must use a `UserId` that has no existing profile in the mock (create-path, same pattern as `Handle_ShouldCreate_WhenNoExistingProfile`), so `ToNewDomain` is called with `FirstName = null`/`LastName = null`. Verifies the DTO returns `null` for both fields.
- `AppTrack.Application.UnitTests/Features/FreelancerProfile/Commands/UploadCvCommandHandlerTests.cs` — existing tests remain valid; handler never sets `FirstName`/`LastName`, so no fixture changes are needed
- `AppTrack.Application.UnitTests/Features/FreelancerProfile/Queries/GetFreelancerProfileQueryHandlerTests.cs` — no changes needed

## Not Affected

- `AppTrack.Shared.Validation/Interfaces/IFreelancerProfileValidatable.cs` — already `string?`
- `AppTrack.Shared.Validation/Validators/FreelancerProfileBaseValidator.cs` — already uses `When`-guards
- `Models/FreelancerProfileModel.cs` — already `string?`
- `AppTrack.Application.UnitTests/Mocks/MockFreelancerProfileRepository.cs` — supplies `FirstName = "Anna"` / `LastName = "Müller"`; both assign literal strings, which are compatible with `string?`
- `AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor` — uses `ValueChanged` callbacks, no `required` attributes; no changes needed
- WPF UI — no direct `FirstName`/`LastName` usage in ViewModels or XAML

## Out of Scope

- No changes to validation rules (max length, etc.)
- No data migration of existing empty-string rows to NULL
