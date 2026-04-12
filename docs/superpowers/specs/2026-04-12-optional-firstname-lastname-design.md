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

## Behaviour After Change

- A user can upload a CV without any profile data — the handler creates `new FreelancerProfile { UserId = ... }` with `FirstName = null`, `LastName = null`.
- `UpsertFreelancerProfile` with `FirstName = null` sets the name to `null` (full-replace semantics, consistent with existing upsert behaviour).
- All validation rules for `FirstName`/`LastName` remain in place (max length 100) and fire only when a value is provided.

## Affected Files

### Domain
- `AppTrack.Domain/FreelancerProfile.cs` — change `string FirstName/LastName` to `string?`

### Persistence
- `AppTrack.Persistance/Configurations/FreelancerProfileConfiguration.cs` — remove `IsRequired()` for both fields
- New EF Core migration — allow NULL on `FirstName` and `LastName` columns

### Application
- `AppTrack.Application/Features/FreelancerProfile/Dto/FreelancerProfileDto.cs` — change `string FirstName/LastName` to `string?`
- `AppTrack.Application/Mappings/FreelancerProfileMappings.cs` — remove `?? string.Empty` in `ToNewDomain` and `ApplyTo`

### Frontend ApiService
- `ApiService/Mappings/FreelancerProfileMappings.cs` — remove `?? string.Empty` in `ToUpsertCommand`

### Tests
- `AppTrack.Application.UnitTests/Features/FreelancerProfile/Commands/UpsertFreelancerProfileCommandHandlerTests.cs` — update test fixtures using non-nullable names
- `AppTrack.Application.UnitTests/Features/FreelancerProfile/Commands/UploadCvCommandHandlerTests.cs` — update test fixtures if needed
- `AppTrack.Application.UnitTests/Features/FreelancerProfile/Queries/GetFreelancerProfileQueryHandlerTests.cs` — update test fixtures if needed

## Out of Scope

- No changes to validation rules (max length, etc.)
- No data migration of existing empty-string rows to NULL
- No UI changes — both WPF and Blazor frontends already use `string?` models
