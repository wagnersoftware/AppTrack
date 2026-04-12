# Profile Backend — Design Spec

**Date:** 2026-04-07
**Branch:** feature/profile-setup-wizard
**Scope:** Full-stack backend integration for the freelancer profile feature. The Blazor UI surface already exists (all frontend commits on this branch). This spec covers Domain → API, NSwag client regeneration, frontend service, Blazor Save wiring, and unit tests.

---

## Context

The Blazor UI for the freelancer profile (form, dialog, page) was implemented in earlier commits on this branch. The Save button currently performs no API call. This spec adds the backend stack required to persist and retrieve a user's profile.

---

## Domain Layer (`AppTrack.Domain`)

### New Enums

These are new types in `AppTrack.Domain.Enums` — separate from the identically-named enums in `AppTrack.Frontend.Models`. The frontend enums are kept; the domain enums are new additions. Mappings between them use explicit integer casts (same pattern as `JobApplicationStatus` ↔ `JobApplicationStatus` in `JobApplicationMappings.cs`).

**`AppTrack.Domain/Enums/RemotePreference.cs`**
```
Remote = 0 | Hybrid = 1 | OnSite = 2
```

**`AppTrack.Domain/Enums/ApplicationLanguage.cs`**
```
German = 0 | English = 1
```

Both enums must have **matching integer values** with their `AppTrack.Frontend.Models` counterparts to allow safe cast-based conversion in `ApiService/Mappings/FreelancerProfileMappings.cs`.

NSwag will generate additional standalone enum types (`RemotePreference`, `ApplicationLanguage`) in `AppTrack.Frontend.ApiService.Base` when it reads the Swagger doc. These are used as cast targets in `FreelancerProfileMappings.cs` (see ApiService section).

### New Entity

**`AppTrack.Domain/FreelancerProfile.cs`** — extends `BaseEntity`

| Property | Type | Notes |
|---|---|---|
| `UserId` | `string` | FK to identity user; one profile per user |
| `FirstName` | `string` | Required |
| `LastName` | `string` | Required |
| `HourlyRate` | `decimal?` | Optional |
| `DailyRate` | `decimal?` | Optional |
| `AvailableFrom` | `DateOnly?` | Optional; EF Core 8+ maps to SQL `date` |
| `WorkMode` | `Domain.Enums.RemotePreference?` | Optional enum |
| `Skills` | `string?` | Optional; free text |
| `Language` | `Domain.Enums.ApplicationLanguage?` | Optional; preferred AI text language |

`SelectedRateType` (frontend `RateKind` UI toggle) is **not** stored — inferred from which rate field is non-null.

---

## Shared Validation Layer (`AppTrack.Shared.Validation`)

### New Interface

**`Interfaces/IFreelancerProfileValidatable.cs`**

```csharp
public interface IFreelancerProfileValidatable
{
    string FirstName { get; }
    string LastName { get; }
    decimal? HourlyRate { get; }
    decimal? DailyRate { get; }
    string? Skills { get; }
}
```

### New Base Validator

**`Validators/FreelancerProfileBaseValidator.cs`**

```csharp
public abstract class FreelancerProfileBaseValidator<T> : AbstractValidator<T>
    where T : IFreelancerProfileValidatable
{
    protected FreelancerProfileBaseValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.HourlyRate).GreaterThan(0).When(x => x.HourlyRate.HasValue);
        RuleFor(x => x.DailyRate).GreaterThan(0).When(x => x.DailyRate.HasValue);
        RuleFor(x => x.Skills).MaximumLength(1000).When(x => x.Skills != null);
    }
}
```

---

## Application Layer (`AppTrack.Application`)

### Repository Contract

**`Contracts/Persistance/IFreelancerProfileRepository.cs`**

```csharp
Task<FreelancerProfile?> GetByUserIdAsync(string userId);
Task UpsertAsync(FreelancerProfile profile);
```

Extends `IGenericRepository<FreelancerProfile>`.

### DTO

**`Features/FreelancerProfile/Dto/FreelancerProfileDto.cs`**

| Field | Type |
|---|---|
| `Id` | `int` |
| `UserId` | `string` |
| `FirstName` | `string` |
| `LastName` | `string` |
| `HourlyRate` | `decimal?` |
| `DailyRate` | `decimal?` |
| `AvailableFrom` | `DateOnly?` |
| `WorkMode` | `Domain.Enums.RemotePreference?` |
| `Skills` | `string?` |
| `Language` | `Domain.Enums.ApplicationLanguage?` |
| `CreationDate` | `DateTime` |
| `ModifiedDate` | `DateTime` |

`UserId` is included in the DTO (same convention as `JobApplicationDto`). `CreationDate` and `ModifiedDate` are non-nullable `DateTime` (matching the `JobApplicationDto` convention — the domain `BaseEntity` nullable dates are unwrapped in the DTO with `?? default`).

### Commands

**`UpsertFreelancerProfileCommand`** (implements `IRequest<FreelancerProfileDto>`, `IUserScopedRequest`, `IFreelancerProfileValidatable`)

Fields: `FirstName`, `LastName`, `HourlyRate?`, `DailyRate?`, `AvailableFrom?`, `WorkMode?` (`Domain.Enums.RemotePreference?`), `Skills?`, `Language?` (`Domain.Enums.ApplicationLanguage?`).
`UserId` is `[JsonIgnore]` — always set from JWT by the mediator pipeline.

**`UpsertFreelancerProfileCommandValidator`** (inherits `FreelancerProfileBaseValidator<UpsertFreelancerProfileCommand>`)

Empty additional constructor — all rules come from the shared base validator.

**`UpsertFreelancerProfileCommandHandler`**

1. Validate command → throw `BadRequestException` on failure (do not call repository)
2. `GetByUserIdAsync(command.UserId)`
3. If null → `command.ToNewDomain()` → call `UpsertAsync` (create path)
4. If found → `command.ApplyTo(existingEntity)` → call `UpsertAsync` (update path)
   - `ApplyTo` updates all mutable fields but **must not overwrite `entity.Id`** (the `Id` is needed by `UpsertAsync` to detect the update path via `profile.Id > 0`)
   - `ApplyTo` **may** overwrite `entity.UserId` (same pattern as `JobApplicationMappings.ApplyTo`; the mediator always sets the correct value via JWT)
5. Return `entity.ToDto()`

### Query

**`GetFreelancerProfileQuery`** (implements `IRequest<FreelancerProfileDto>`, `IUserScopedRequest`)

`UserId` is `[JsonIgnore]`.

**`GetFreelancerProfileQueryValidator`** (inherits `AbstractValidator<GetFreelancerProfileQuery>`)

Empty constructor — no rules. Consistent with the established pattern: all existing `Get*` query validators in this codebase have empty constructors (e.g. `GetJobApplicationDefaultsByUserIdQueryValidator`). `UserId` is always set by the mediator from JWT, so no runtime rule is needed.

**`GetFreelancerProfileQueryHandler`**

1. Instantiate and call `GetFreelancerProfileQueryValidator` (validator has no rules; validation will never fail — included for consistency with the handler pattern)
2. `GetByUserIdAsync(query.UserId)`
3. If null → throw `NotFoundException("FreelancerProfile", query.UserId)`
4. Return `entity.ToDto()`

The return type is `FreelancerProfileDto` (non-nullable). The 404 case is handled entirely by exception middleware — the handler never returns null.

### Mappings

`FreelancerProfileMappings.cs` (internal static class in `AppTrack.Application/Mappings/`):

- `UpsertFreelancerProfileCommand.ToNewDomain()` → `FreelancerProfile`
  - Maps all fields; `UserId` is set from `command.UserId`
- `UpsertFreelancerProfileCommand.ApplyTo(FreelancerProfile entity)` — mutates the tracked entity in place
- `FreelancerProfile.ToDto()` → `FreelancerProfileDto`

---

## Persistence Layer (`AppTrack.Persistance`)

### Entity Configuration

**`Configurations/FreelancerProfileConfiguration.cs`**

- `UserId` — required, max 450 chars (standard ASP.NET Identity key length)
- `FirstName` — required, max 100 chars
- `LastName` — required, max 100 chars
- `HourlyRate` — `HasPrecision(18, 2)`
- `DailyRate` — `HasPrecision(18, 2)`
- `Skills` — max 1000 chars

### Repository

**`Repositories/FreelancerProfileRepository.cs`**

Extends `GenericRepository<FreelancerProfile>`, implements `IFreelancerProfileRepository`.

`GetByUserIdAsync` — `_context.FreelancerProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId)`

`UpsertAsync` — delegates to inherited `CreateAsync(profile)` when `profile.Id == 0` (new entity), and to `UpdateAsync(profile)` when `profile.Id > 0` (existing entity). This is consistent with the pattern used in other repositories that extend `GenericRepository<T>`.

### DbContext

Add `DbSet<FreelancerProfile> FreelancerProfiles` to `AppTrackDatabaseContext`.

### Migration

Generate via `dotnet ef migrations add AddFreelancerProfileTable --project AppTrack.Persistance --startup-project AppTrack.Api`. Auto-applied on startup via `MigrationsHelper`.

### Service Registration

In `PersistanceServiceRegistration.cs`:
```csharp
services.AddScoped<IFreelancerProfileRepository, FreelancerProfileRepository>();
```

---

## API Layer (`AppTrack.Api`)

### Controller

**`Controllers/ProfileController.cs`** — `[Route("api/profile")]`, `[ApiController]`, `[Authorize]`

| Method | Route | Command/Query | Success Response | Error Responses |
|---|---|---|---|---|
| `GET` | `/api/profile` | `GetFreelancerProfileQuery` | 200 `FreelancerProfileDto` | 401, 404 |
| `PUT` | `/api/profile` | `UpsertFreelancerProfileCommand` | 200 `FreelancerProfileDto` | 400, 401 |

Both endpoints get `UserId` from JWT via the mediator pipeline — it is never in the request body or route.

---

## Frontend API Service (`ApiService`)

### NSwag Client Regeneration

The `clientsettings.nswag` uses a live Swagger URL (`https://localhost:7273/swagger/v1/swagger.json`) to generate `ServiceClient.cs`. To regenerate:

1. Start `AppTrack.Api` locally (the API must be running)
2. Run NSwag generation via the NSwag CLI or the `clientsettings.nswag` document
3. The updated `ServiceClient.cs` will contain new methods:
   - `ProfileGETAsync()` → `FreelancerProfileDto`
   - `ProfilePUTAsync(UpsertFreelancerProfileCommand)` → `FreelancerProfileDto`

The `clientsettings.nswag` also contains an embedded inline JSON snapshot; the regeneration step updates both the generated C# code and the cached inline spec.

### Service Contract

**`Contracts/IFreelancerProfileService.cs`**

```csharp
Task<Response<FreelancerProfileDto>> GetProfileAsync();
Task<Response<FreelancerProfileDto>> UpsertProfileAsync(FreelancerProfileModel model);
```

### Service Implementation

**`Services/FreelancerProfileService.cs`** — extends `BaseHttpService`

Maps `FreelancerProfileModel` → `UpsertFreelancerProfileCommand` via internal mappings, calls the NSwag client.

### Mappings

**`Mappings/FreelancerProfileMappings.cs`**

NSwag generates standalone enum types `RemotePreference` and `ApplicationLanguage` in the `AppTrack.Frontend.ApiService.Base` namespace (from the Swagger doc). All casts use an integer intermediary to bridge between the three enum namespaces (Domain, Frontend.Models, ApiService.Base).

- `FreelancerProfileModel.ToUpsertCommand()` → `UpsertFreelancerProfileCommand`
  - `SelectedRateType` is **not** mapped (UI-only field)
  - `WorkMode` cast: `(AppTrack.Frontend.ApiService.Base.RemotePreference?)(int?)model.WorkMode`
  - `Language` cast: `(AppTrack.Frontend.ApiService.Base.ApplicationLanguage?)(int?)model.Language`
- `FreelancerProfileDto.ToModel()` → `FreelancerProfileModel`
  - `WorkMode` cast: `(AppTrack.Frontend.Models.RemotePreference?)(int?)dto.WorkMode`
  - `Language` cast: `(AppTrack.Frontend.Models.ApplicationLanguage?)(int?)dto.Language`
  - `SelectedRateType` — derived: `HourlyRate != null → RateKind.Hourly`, `DailyRate != null → RateKind.Daily`, else `null`

### Service Registration

In `ApiServiceRegistration.cs` (frontend):
```csharp
services.AddScoped<IFreelancerProfileService, FreelancerProfileService>();
```

---

## Frontend Models (`AppTrack.Frontend.Models`)

### `FreelancerProfileModel` Update

`FreelancerProfileModel` currently does not extend `ModelBase`. It must be updated to:

```csharp
public class FreelancerProfileModel : ModelBase, IFreelancerProfileValidatable
```

`ModelBase` provides `Id`, `CreationDate`, `ModifiedDate` (needed for round-tripping with the DTO). `IFreelancerProfileValidatable` enables the shared base validator.

### New Frontend Validator

**`Models/Validators/FreelancerProfileModelValidator.cs`**

```csharp
public class FreelancerProfileModelValidator : FreelancerProfileBaseValidator<FreelancerProfileModel>
{
    public FreelancerProfileModelValidator() { }
}
```

No additional rules — all shared rules come from the base.

### Service Registration in `Program.cs`

```csharp
builder.Services.AddTransient<IValidator<FreelancerProfileModel>, FreelancerProfileModelValidator>();
```

The generic `IModelValidator<>` → `ModelValidator<>` open-generic registration already covers this.

---

## Blazor UI Wiring (`AppTrack.BlazorUi`)

### `FreelancerProfileForm` Refactor

The form currently owns a `private readonly FreelancerProfileModel _model = new()`. It must be refactored to accept the model as a `[Parameter]` so parent components can pass in a pre-loaded model and read back the user's edits.

**Changes to `FreelancerProfileForm.razor.cs`:**
- Replace `private readonly FreelancerProfileModel _model = new()` with `[Parameter] public FreelancerProfileModel Model { get; set; } = new()`
- Inject `[Inject] private IModelValidator<FreelancerProfileModel> ModelValidator { get; set; } = null!`
- Override `OnParametersSet()` to sync `_availableFrom` from `Model.AvailableFrom`:
  ```csharp
  protected override void OnParametersSet()
  {
      _availableFrom = Model.AvailableFrom.HasValue
          ? Model.AvailableFrom.Value.ToDateTime(TimeOnly.MinValue)
          : null;
  }
  ```
- All four method bodies (`OnAvailableFromChanged`, `OnRateKindChanged`, `OnRateValueChanged`, `SelectFreelancer`) must reference `Model` instead of `_model`. `_selectedType` (the profile-type toggle) is unchanged.
- Add field-change handlers that call `ModelValidator.ResetErrors(propertyName)` for every validated field (`FirstName`, `LastName`, `HourlyRate`, `DailyRate`, `Skills`). The remaining fields (`AvailableFrom`, `WorkMode`, `Language`, rate toggle) do not have validation rules and do not need `ResetErrors`.
- Expose a `public bool Validate() => ModelValidator.Validate(Model)` method so parent components can trigger validation via a `@ref` capture.
- Add helper: `private string GetFirstError(string propertyName) => ModelValidator.Errors.GetValueOrDefault(propertyName)?.FirstOrDefault() ?? string.Empty`

**Changes to `FreelancerProfileForm.razor`:**
- All references to `_model.*` become `Model.*`.
- Add `Error` and `ErrorText` props to the five validated fields:
  - `FirstName` → `Error="@ModelValidator.Errors.ContainsKey(nameof(Model.FirstName))"` `ErrorText="@GetFirstError(nameof(Model.FirstName))"`
  - `LastName` — same pattern
  - Rate numeric field — bound to `HourlyRate` or `DailyRate` depending on `SelectedRateType`; use `ErrorText` for whichever is active
  - `Skills` — same pattern
- Replace `@bind-Value` on validated fields with explicit `Value=` + `ValueChanged=` (matching `CreateJobApplicationDialog` pattern) so `ResetErrors` can be called on change.

**Changes to `ProfileSetup.razor` and `ProfileSetupDialog.razor`:**
- Both become `<FreelancerProfileForm @ref="_form" Model="_model" />`. The `@ref` gives the parent access to `_form.Validate()`.

### `ProfileSetup` Page

**`ProfileSetup.razor.cs`** changes:
1. Inject `IFreelancerProfileService` and `ISnackbar`
2. Add `private FreelancerProfileModel _model = new()` and `private FreelancerProfileForm _form = null!`
3. `OnInitializedAsync()`:
   - Call `GetProfileAsync()` — if success, `_model = result.Data!.ToModel()`; if 404 / error, keep empty model
4. `Save()` (existing method):
   - Call `if (!_form.Validate()) return;`
   - Call `UpsertProfileAsync(_model)`
   - On success: show snackbar "Profile saved", navigate to `/`
   - On error: show snackbar with `response.ErrorMessage`

### `ProfileSetupDialog`

**`ProfileSetupDialog.razor.cs`** changes:
1. Inject `IFreelancerProfileService` and `ISnackbar`
2. Add `private FreelancerProfileModel _model = new()` and `private FreelancerProfileForm _form = null!`
3. `OnInitializedAsync()` — same as page: load existing profile if available
4. `Save()`:
   - Call `if (!_form.Validate()) return;`
   - Call `UpsertProfileAsync(_model)`
   - On success: `MudDialog.Close()`
   - On error: show snackbar with `response.ErrorMessage`
5. `Skip()` unchanged: `MudDialog.Cancel()`

---

## Unit Tests (`AppTrack.Application.UnitTests`)

### Mock

Add `MockFreelancerProfileRepository.cs` in `Mocks/` following the existing mock pattern (returns a predefined `FreelancerProfile` entity for known `UserId`, null for unknown).

### `UpsertFreelancerProfileCommandHandlerTests`

- `Handle_ShouldCreate_WhenNoExistingProfile` — mock returns null, verifies `UpsertAsync` called with new entity
- `Handle_ShouldUpdate_WhenProfileExists` — mock returns existing entity, verifies `UpsertAsync` called with updated entity
- `Handle_ShouldReturnDto_WhenCommandIsValid`
- `Handle_ShouldThrowBadRequestException_WhenFirstNameIsEmpty`
- `Handle_ShouldThrowBadRequestException_WhenLastNameIsEmpty`
- `Handle_ShouldThrowBadRequestException_WhenHourlyRateIsNegative`
- `Handle_ShouldNotCallUpsertAsync_WhenValidationFails` — verifies repository is never called when validation throws

### `UpsertFreelancerProfileCommandValidatorTests`

- Valid command → no errors
- Empty `FirstName` → error on `FirstName`
- Empty `LastName` → error on `LastName`
- `HourlyRate = 0` → error
- `HourlyRate = -1` → error
- `HourlyRate = null` → no error (optional)
- `Skills` exceeding 1000 chars → error

### `GetFreelancerProfileQueryValidatorTests`

- Valid query (non-empty `UserId`) → no errors (the validator has no rules; this test documents that the validator exists and compiles)

### `GetFreelancerProfileQueryHandlerTests`

- Profile exists → returns `FreelancerProfileDto`
- Profile not found → throws `NotFoundException`

---

## Out of Scope

- CV file upload (still decorative)
- Employee profile type
- Auto-trigger of `ProfileSetupDialog` on first login
- Profile deletion
