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

**`AppTrack.Domain/Enums/RemotePreference.cs`**
```
Remote | Hybrid | OnSite
```

**`AppTrack.Domain/Enums/ApplicationLanguage.cs`**
```
German | English
```

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
| `WorkMode` | `RemotePreference?` | Optional enum |
| `Skills` | `string?` | Optional; free text |
| `Language` | `ApplicationLanguage?` | Optional; preferred AI text language |

`SelectedRateType` (frontend `RateKind` UI toggle) is **not** stored — inferred from which rate field is non-null.

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

Mirrors all domain entity properties (plus `Id`, `CreationDate`, `ModifiedDate` from `BaseEntity`).

### Commands

**`UpsertFreelancerProfileCommand`** (implements `IRequest<FreelancerProfileDto>`, `IUserScopedRequest`)

Fields: `FirstName`, `LastName`, `HourlyRate?`, `DailyRate?`, `AvailableFrom?`, `WorkMode?`, `Skills?`, `Language?`.
`UserId` is `[JsonIgnore]` — always set from JWT by the mediator pipeline.

**`UpsertFreelancerProfileCommandValidator`** (inherits `AbstractValidator<UpsertFreelancerProfileCommand>`)

Rules:
- `FirstName` — `NotEmpty`, `MaxLength(100)`
- `LastName` — `NotEmpty`, `MaxLength(100)`
- `HourlyRate` — `GreaterThan(0)` when not null
- `DailyRate` — `GreaterThan(0)` when not null
- `Skills` — `MaxLength(1000)` when not null

**`UpsertFreelancerProfileCommandHandler`**

1. Validate command → throw `BadRequestException` on failure
2. `GetByUserIdAsync(command.UserId)`
3. If null → map command to new `FreelancerProfile`, call `UpsertAsync` (create)
4. If found → apply command fields to existing entity, call `UpsertAsync` (update)
5. Return DTO

### Query

**`GetFreelancerProfileQuery`** (implements `IRequest<FreelancerProfileDto?>`, `IUserScopedRequest`)

`UserId` is `[JsonIgnore]`.

**`GetFreelancerProfileQueryHandler`**

1. `GetByUserIdAsync(query.UserId)`
2. If null → throw `NotFoundException`
3. Return DTO

### Mappings

`FreelancerProfileMappings.cs` (internal static class in `AppTrack.Application/Mappings/`):
- `UpsertFreelancerProfileCommand.ToNewDomain()` → `FreelancerProfile`
- `UpsertFreelancerProfileCommand.ApplyTo(FreelancerProfile entity)`
- `FreelancerProfile.ToDto()` → `FreelancerProfileDto`

---

## Persistence Layer (`AppTrack.Persistance`)

### Entity Configuration

**`Configurations/FreelancerProfileConfiguration.cs`**

- `UserId` — required, max 450 chars (standard ASP.NET Identity key length)
- `FirstName` — required, max 100 chars
- `LastName` — required, max 100 chars
- `HourlyRate` — precision(18, 2)
- `DailyRate` — precision(18, 2)
- `Skills` — max 1000 chars

### Repository

**`Repositories/FreelancerProfileRepository.cs`**

Extends `GenericRepository<FreelancerProfile>`, implements `IFreelancerProfileRepository`.

`GetByUserIdAsync` — `_context.FreelancerProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId)`

`UpsertAsync` — checks if entity has `Id > 0` to decide between `_context.Add` + `SaveChanges` (create) or `_context.Update` + `SaveChanges` (update).

### DbContext

Add `DbSet<FreelancerProfile> FreelancerProfiles` to `AppTrackDatabaseContext`.

### Migration

Generate via `dotnet ef migrations add AddFreelancerProfileTable`. Auto-applied on startup.

---

## API Layer (`AppTrack.Api`)

### Controller

**`Controllers/ProfileController.cs`** — `[Route("api/profile")]`, `[ApiController]`, `[Authorize]`

| Method | Route | Command/Query | Response |
|---|---|---|---|
| `GET` | `/api/profile` | `GetFreelancerProfileQuery` | 200 `FreelancerProfileDto` / 404 |
| `PUT` | `/api/profile` | `UpsertFreelancerProfileCommand` | 200 `FreelancerProfileDto` |

Both endpoints get `UserId` from JWT via the mediator pipeline — it is never in the request body or route.

---

## Frontend API Service (`ApiService`)

### NSwag Client Regeneration

After the API controller is in place and the API can be started, run NSwag regeneration via `clientsettings.nswag` to update `ServiceClient.cs` with:
- `ProfileGETAsync()` → `FreelancerProfileDto`
- `ProfilePUTAsync(UpsertFreelancerProfileCommand)` → `FreelancerProfileDto`

### Service Contract

**`Contracts/IFreelancerProfileService.cs`**

```csharp
Task<Response<FreelancerProfileDto>> GetProfileAsync();
Task<Response<FreelancerProfileDto>> UpsertProfileAsync(FreelancerProfileModel model);
```

### Service Implementation

**`Services/FreelancerProfileService.cs`** — extends `BaseHttpService`

Maps `FreelancerProfileModel` → `UpsertFreelancerProfileCommand` via internal mappings, calls NSwag client methods.

### Mappings

**`Mappings/FreelancerProfileMappings.cs`**

- `FreelancerProfileModel.ToUpsertCommand()` → `UpsertFreelancerProfileCommand`
  - `SelectedRateType` is NOT mapped (UI-only field)
- `FreelancerProfileDto.ToModel()` → `FreelancerProfileModel`
  - Sets `SelectedRateType` based on which rate is non-null (HourlyRate → `RateKind.Hourly`, DailyRate → `RateKind.Daily`)

---

## Blazor UI Wiring (`AppTrack.BlazorUi`)

### `FreelancerProfileForm`

Expose `[Parameter] EventCallback<FreelancerProfileModel> OnSave` and rename the private `_model` backing field to support external read-back, OR expose the form model via a `[Parameter]` so the parent (page/dialog) can read and submit it.

**Chosen approach:** expose `[Parameter] public FreelancerProfileModel Model { get; set; }` with `[Parameter] public EventCallback OnSaved { get; set; }`. Parent components own the model instance, pass it in, and handle saving.

### `ProfileSetup` page

1. Inject `IFreelancerProfileService`
2. On init: call `GetProfileAsync()` — if success, populate form model; if 404, start with empty model
3. Save button: call `UpsertProfileAsync(model)`, show snackbar success/error, navigate to `/` on success

### `ProfileSetupDialog`

1. Inject `IFreelancerProfileService`
2. On init: call `GetProfileAsync()` — populate model if exists
3. Save button: call `UpsertProfileAsync(model)`, close dialog on success
4. Skip button: still calls `MudDialog.Cancel()` (unchanged)

---

## Unit Tests (`AppTrack.Application.UnitTests`)

### `UpsertFreelancerProfileCommandHandlerTests`

- `Handle_ShouldCreate_WhenNoExistingProfile` — mock returns null, verifies `UpsertAsync` called with new entity
- `Handle_ShouldUpdate_WhenProfileExists` — mock returns existing entity, verifies `UpsertAsync` called with updated entity
- `Handle_ShouldThrowBadRequestException_WhenFirstNameIsEmpty`
- `Handle_ShouldThrowBadRequestException_WhenHourlyRateIsNegative`
- `Handle_ShouldReturnDto_WhenCommandIsValid`

### `UpsertFreelancerProfileCommandValidatorTests`

- Valid command → no errors
- Empty `FirstName` → error on `FirstName`
- Empty `LastName` → error on `LastName`
- `HourlyRate = 0` → error
- `HourlyRate = -1` → error
- `HourlyRate = null` → no error (optional)
- `Skills` exceeding 1000 chars → error

### `GetFreelancerProfileQueryHandlerTests`

- Profile exists → returns DTO
- Profile not found → throws `NotFoundException`

---

## Service Registration

In `PersistanceServiceRegistration.cs`:
```csharp
services.AddScoped<IFreelancerProfileRepository, FreelancerProfileRepository>();
```

In `ApiServiceRegistration.cs` (frontend):
```csharp
services.AddScoped<IFreelancerProfileService, FreelancerProfileService>();
```

---

## Out of Scope

- CV file upload (still decorative)
- Employee profile type
- Auto-trigger of ProfileSetupDialog on first login
- Profile deletion
