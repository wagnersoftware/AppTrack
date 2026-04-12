# CV Upload Design Spec

**Feature:** CV upload for freelancer profile
**Branch:** `feature/profile-setup-wizard`
**Date:** 2026-04-07

---

## Goal

Allow users to upload a PDF CV from the profile form. The file is stored in Azure Blob Storage and the extracted text is saved to the `FreelancerProfile` row in SQL. Upload happens immediately on file selection (separate from the profile Save button). CV upload works even if no profile exists yet — a minimal profile is auto-created.

---

## Architecture

```
Browser → POST /api/profile/cv (multipart/form-data)
        → UploadCvCommand (Stream + metadata)
        → UploadCvCommandHandler
              ├── Validate (PDF, ≤ 10 MB)
              ├── Extract text (PdfPig, in-memory)
              ├── Upload blob (ICvStorageService)
              ├── Save to DB (UpsertAsync)
              └── On DB failure: delete blob (compensating transaction)
```

**New packages:**
- `Azure.Storage.Blobs` — in `AppTrack.Infrastructure`
- `UglyToad.PdfPig` — in `AppTrack.Infrastructure`

**Dev storage:** Azurite (`UseDevelopmentStorage=true`), run via Docker:
```
docker run -p 10000:10000 mcr.microsoft.com/azure-storage/azurite azurite-blob
```

---

## Domain Changes (`AppTrack.Domain`)

Three new nullable fields on `FreelancerProfile`:

| Property | Type | Column | Purpose |
|---|---|---|---|
| `CvBlobPath` | `string?` | `nvarchar(500)` | Blob key `{userId}/cv.pdf` |
| `CvFileName` | `string?` | `nvarchar(255)` | Original filename for display |
| `CvText` | `string?` | `nvarchar(max)` | PDF-extracted text |

EF configuration added in `FreelancerProfileConfiguration`. New EF migration required.

---

## Application Layer (`AppTrack.Application`)

### `ICvStorageService` (new interface in `Contracts/`)

```csharp
public interface ICvStorageService
{
    Task<string> UploadAsync(string userId, Stream stream, string fileName);
    Task DeleteAsync(string blobPath);
}
```

### `UploadCvCommand`

```csharp
public class UploadCvCommand : IRequest<FreelancerProfileDto>, IUserScopedRequest
{
    [JsonIgnore] public string UserId { get; set; } = string.Empty;
    public Stream FileStream { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}
```

### `UploadCvCommandValidator`

- `ContentType` must be `application/pdf` OR `FileName` must end with `.pdf`
- `FileSizeBytes` ≤ 10 MB (10 × 1024 × 1024)
- `FileName` not empty

### `UploadCvCommandHandler` — compensating transaction

```
1. Validate command
2. Extract text from FileStream using PdfPig (in-memory, no IO)
3. Reset stream position to 0
4. blobPath = await _cvStorageService.UploadAsync(userId, stream, fileName)
5. profile = await _repo.GetByUserIdAsync(userId)
   if profile == null → create new FreelancerProfile { UserId = userId }
6. Set profile.CvBlobPath, profile.CvFileName, profile.CvText
7. await _repo.UpsertAsync(profile)
   on failure → await _cvStorageService.DeleteAsync(blobPath) then rethrow
8. return profile.ToDto()
```

### `FreelancerProfileDto` — three new fields

```csharp
public string? CvBlobPath { get; set; }
public string? CvFileName { get; set; }
public string? CvText { get; set; }
```

### `FreelancerProfileMappings` — `ToDto()` extended

Maps the three new CV fields from entity to DTO.

---

## Infrastructure Layer (`AppTrack.Infrastructure`)

### `AzureBlobStorageService : ICvStorageService`

- Constructor receives `IOptions<AzureStorageSettings>`
- `UploadAsync`: creates container if not exists, uploads blob with key `{userId}/cv.pdf`, overwrites if exists
- `DeleteAsync`: deletes blob if it exists (no error if already gone)

### `AzureStorageSettings`

```csharp
public class AzureStorageSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}
```

Registered via `IOptions<AzureStorageSettings>` in `InfrastructureServicesRegistration`.

### `appsettings.json` addition

```json
"AzureStorageSettings": {
  "ConnectionString": "UseDevelopmentStorage=true",
  "ContainerName": "cv-uploads"
}
```

---

## Persistence Layer (`AppTrack.Persistance`)

No new repository methods required — existing `UpsertAsync` handles both create and update.

---

## API Layer (`AppTrack.Api`)

### New endpoint on `ProfileController`

```
POST /api/profile/cv
Content-Type: multipart/form-data
Body: file (IFormFile)

→ 200 FreelancerProfileDto
→ 400 validation error (not PDF, too large)
→ 500 storage/DB error
```

Controller reads `IFormFile`, opens stream, creates `UploadCvCommand`, calls `_mediator.Send(command)`.

---

## Frontend (`AppTrack.BlazorUi`)

### `FreelancerProfileModel` — new field

```csharp
public string? CvFileName { get; set; }
```

(`CvBlobPath` and `CvText` are not needed in the frontend model.)

### `FreelancerProfileMappings.ToModel()` — extended

Maps `dto.CvFileName` to `model.CvFileName`.

### `IFreelancerProfileService` — new method

```csharp
Task<Response<FreelancerProfileModel>> UploadCvAsync(IBrowserFile file);
```

### `FreelancerProfileService.UploadCvAsync`

Reads `IBrowserFile` into a `MultipartFormDataContent`, calls `ProfileCVPOSTAsync` (NSwag-generated after regen).

### `FreelancerProfileForm.razor` — `MudFileUpload` wired up

- `FilesChanged` callback → calls `UploadCvAsync`
- Shows `MudProgressLinear` (or spinner) while uploading
- On success: update `Model.CvFileName`, show Snackbar "CV uploaded"
- On error: show Snackbar with error message
- Button label: `"Upload CV (PDF)"` when no CV, `"Replace CV · {CvFileName}"` when CV exists

### NSwag regeneration (manual step)

After API changes are deployed locally, regen `ServiceClient.cs` via `nswag run clientsettings.nswag`.

---

## Unit Tests

- `UploadCvCommandValidatorTests` — PDF type check, size limit, empty filename
- `UploadCvCommandHandlerTests` — success (existing profile), success (no profile → auto-create), blob-fail path, DB-fail + compensating delete

---

## Out of Scope

- CV download endpoint
- CV deletion (only replacement)
- Non-PDF formats
- OCR for scanned PDFs (PdfPig extracts embedded text only)
