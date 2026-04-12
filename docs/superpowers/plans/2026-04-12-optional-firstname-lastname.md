# Optional FirstName/LastName on FreelancerProfile — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make `FirstName` and `LastName` optional (nullable) on `FreelancerProfile` so a user can upload a CV without providing any other profile data.

**Architecture:** Change the domain entity and DB configuration from non-nullable to nullable, propagate `string?` through the DTO and mappings, manually patch the NSwag-generated client, and add a unit test for the null-name create path.

**Tech Stack:** .NET 10, EF Core 10, xUnit, Shouldly, Moq, NSwag (generated client)

---

## Chunk 1: Domain, DTO, Persistence, and Mappings

### Task 1: Write the failing test first (TDD)

**Files:**
- Modify: `AppTrack.Application.UnitTests/Features/FreelancerProfile/Commands/UpsertFreelancerProfileCommandHandlerTests.cs`

The test can be added now because `UpsertFreelancerProfileCommand.FirstName` is already `string?`, so the test compiles. It will fail because the mapping still uses `?? string.Empty`, causing `result.FirstName` to return `""` instead of `null`. The test verifies the behavior we are about to implement.

`MockFreelancerProfileRepository` returns `null` for any `UserId` that is not `ExistingUserId` (`"user-1"`). Use `"nameless-user"` to hit the create-path (which calls `ToNewDomain`).

- [ ] **Step 1: Add the failing test**

Add this test to `UpsertFreelancerProfileCommandHandlerTests.cs`, after the existing tests:

```csharp
[Fact]
public async Task Handle_ShouldCreate_WhenNamesAreNull()
{
    // Arrange — use a userId with no existing profile to hit the create-path
    var command = new UpsertFreelancerProfileCommand
    {
        UserId = "nameless-user",
        FirstName = null,
        LastName = null,
    };

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.ShouldNotBeNull();
    result.FirstName.ShouldBeNull();
    result.LastName.ShouldBeNull();
    _mockRepo.Verify(r => r.UpsertAsync(It.Is<AppTrack.Domain.FreelancerProfile>(p => p.Id == 0)), Times.Once);
}
```

- [ ] **Step 2: Run the test and verify it fails**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~Handle_ShouldCreate_WhenNamesAreNull" --configuration Release
```
Expected: **FAIL** — `result.FirstName` is `""` (empty string) not `null`, because `ToNewDomain` uses `command.FirstName ?? string.Empty`.

---

### Task 2: Make domain entity and DTO nullable (atomic change)

> These two files must be changed in the same commit to avoid a transient build error: changing the domain to `string?` while the DTO is still `string` causes `string? → string` in `ToDto`, which fails under `Nullable=enable` + `TreatWarningsAsErrors`.

**Files:**
- Modify: `AppTrack.Domain/FreelancerProfile.cs:9-10`
- Modify: `AppTrack.Application/Features/FreelancerProfile/Dto/FreelancerProfileDto.cs:9-10`

- [ ] **Step 1: Change `FreelancerProfile.FirstName` and `LastName` to `string?`**

In `AppTrack.Domain/FreelancerProfile.cs`, change lines 9–10 from:
```csharp
public string FirstName { get; set; } = string.Empty;
public string LastName { get; set; } = string.Empty;
```
to:
```csharp
public string? FirstName { get; set; }
public string? LastName { get; set; }
```

- [ ] **Step 2: Change `FreelancerProfileDto.FirstName` and `LastName` to `string?`**

In `AppTrack.Application/Features/FreelancerProfile/Dto/FreelancerProfileDto.cs`, change lines 9–10 from:
```csharp
public string FirstName { get; set; } = string.Empty;
public string LastName { get; set; } = string.Empty;
```
to:
```csharp
public string? FirstName { get; set; }
public string? LastName { get; set; }
```

- [ ] **Step 3: Build to verify no compile errors**

```bash
dotnet build AppTrack.sln --configuration Release
```
Expected: `Build succeeded` with 0 errors and 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Domain/FreelancerProfile.cs
git add "AppTrack.Application/Features/FreelancerProfile/Dto/FreelancerProfileDto.cs"
git commit -m "feat: make FirstName and LastName nullable on FreelancerProfile domain and DTO"
```

---

### Task 3: Remove `IsRequired()` from DB configuration and create migration

**Files:**
- Modify: `AppTrack.Persistance/Configurations/FreelancerProfileConfiguration.cs:15-21`
- Create: new EF Core migration (auto-named by `dotnet ef`)
- Auto-updated: `AppTrack.Persistance/Migrations/AppTrackDatabaseContextModelSnapshot.cs`

- [ ] **Step 1: Remove `IsRequired()` for FirstName and LastName**

In `AppTrack.Persistance/Configurations/FreelancerProfileConfiguration.cs`, change:
```csharp
builder.Property(x => x.FirstName)
    .IsRequired()
    .HasMaxLength(100);

builder.Property(x => x.LastName)
    .IsRequired()
    .HasMaxLength(100);
```
to:
```csharp
builder.Property(x => x.FirstName)
    .HasMaxLength(100);

builder.Property(x => x.LastName)
    .HasMaxLength(100);
```

- [ ] **Step 2: Create the EF Core migration**

Run from the solution root:
```bash
dotnet ef migrations add MakeFirstNameLastNameNullable --project AppTrack.Persistance --startup-project AppTrack.Api
```
Expected: a new migration file appears under `AppTrack.Persistance/Migrations/` and `AppTrackDatabaseContextModelSnapshot.cs` is updated.

Verify the generated migration's `Up` method alters both columns to nullable. It should look roughly like:
```csharp
migrationBuilder.AlterColumn<string>(
    name: "LastName",
    table: "FreelancerProfiles",
    type: "nvarchar(100)",
    maxLength: 100,
    nullable: true, // <-- this must be true
    oldClrType: typeof(string),
    oldType: "nvarchar(100)",
    oldMaxLength: 100);
// same for FirstName
```

- [ ] **Step 3: Build to verify no compile errors**

```bash
dotnet build AppTrack.sln --configuration Release
```
Expected: `Build succeeded`.

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Persistance/Configurations/FreelancerProfileConfiguration.cs
git add AppTrack.Persistance/Migrations/
git commit -m "feat: allow NULL for FirstName and LastName in DB (EF Core migration)"
```

---

### Task 4: Remove null-coercion from backend mappings — and verify test now passes

**Files:**
- Modify: `AppTrack.Application/Mappings/FreelancerProfileMappings.cs:11-13,23-24`

The mapping currently coerces `null` → `""` with `?? string.Empty`. Since the domain now accepts `string?`, this coercion must be removed so null is stored as null.

- [ ] **Step 1: Fix `ToNewDomain`**

In `AppTrack.Application/Mappings/FreelancerProfileMappings.cs`, change `ToNewDomain`:
```csharp
// Before
FirstName = command.FirstName ?? string.Empty,
LastName = command.LastName ?? string.Empty,
```
```csharp
// After
FirstName = command.FirstName,
LastName = command.LastName,
```

- [ ] **Step 2: Fix `ApplyTo`**

In the same file, change `ApplyTo`:
```csharp
// Before
entity.FirstName = command.FirstName ?? string.Empty;
entity.LastName = command.LastName ?? string.Empty;
```
```csharp
// After
entity.FirstName = command.FirstName;
entity.LastName = command.LastName;
```

(`ToDto` is a direct assignment `FirstName = entity.FirstName` — no change needed.)

- [ ] **Step 3: Run the test from Task 1 and verify it now passes**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~Handle_ShouldCreate_WhenNamesAreNull" --configuration Release
```
Expected: **PASS**.

- [ ] **Step 4: Run all unit tests to confirm no regressions**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```
Expected: all tests pass, 0 failures.

- [ ] **Step 5: Commit**

```bash
git add "AppTrack.Application/Mappings/FreelancerProfileMappings.cs"
git add "AppTrack.Application.UnitTests/Features/FreelancerProfile/Commands/UpsertFreelancerProfileCommandHandlerTests.cs"
git commit -m "feat: remove null-coercion in backend mappings; test: add Handle_ShouldCreate_WhenNamesAreNull"
```

---

## Chunk 2: Frontend ApiService and ServiceClient Patch

### Task 5: Remove null-coercion from frontend ApiService mappings

**Files:**
- Modify: `ApiService/Mappings/FreelancerProfileMappings.cs:10-11`

- [ ] **Step 1: Fix `ToUpsertCommand`**

In `ApiService/Mappings/FreelancerProfileMappings.cs`, change:
```csharp
// Before
FirstName = model.FirstName ?? string.Empty,
LastName = model.LastName ?? string.Empty,
```
```csharp
// After
FirstName = model.FirstName,
LastName = model.LastName,
```

(`ToModel` assigns `FirstName = dto.FirstName` — direct assignment, no change needed.)

- [ ] **Step 2: Build to verify**

```bash
dotnet build AppTrack.sln --configuration Release
```
Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```bash
git add ApiService/Mappings/FreelancerProfileMappings.cs
git commit -m "feat: remove null-coercion in frontend ApiService FreelancerProfile mappings"
```

---

### Task 6: Manually patch NSwag-generated ServiceClient

**Files:**
- Modify: `ApiService/Base/ServiceClient.cs:2153,2156,2481,2484`

`clientsettings.nswag` has `"generateNullableReferenceTypes": false`, so regeneration will not produce `string?`. Patch manually. Do not change the NSwag settings — that would affect all other generated types.

- [ ] **Step 1: Patch `FreelancerProfileDto` in ServiceClient**

In `ApiService/Base/ServiceClient.cs`, change lines 2153 and 2156:
```csharp
// Before
public string FirstName { get; set; }
public string LastName { get; set; }
```
```csharp
// After
public string? FirstName { get; set; }
public string? LastName { get; set; }
```

- [ ] **Step 2: Patch `UpsertFreelancerProfileCommand` in ServiceClient**

In the same file, change lines 2481 and 2484:
```csharp
// Before
public string FirstName { get; set; }
public string LastName { get; set; }
```
```csharp
// After
public string? FirstName { get; set; }
public string? LastName { get; set; }
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build AppTrack.sln --configuration Release
```
Expected: `Build succeeded`.

- [ ] **Step 4: Commit**

```bash
git add ApiService/Base/ServiceClient.cs
git commit -m "feat: patch NSwag ServiceClient — FirstName/LastName nullable in FreelancerProfileDto and UpsertFreelancerProfileCommand"
```

---

## Chunk 3: Final Verification

### Task 7: Full build and test run

- [ ] **Step 1: Full solution build**

```bash
dotnet build AppTrack.sln --configuration Release
```
Expected: `Build succeeded`, 0 errors, 0 warnings.

- [ ] **Step 2: Run all unit tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```
Expected: all tests pass.

- [ ] **Step 3: Verify migration is in the list**

```bash
dotnet ef migrations list --project AppTrack.Persistance --startup-project AppTrack.Api
```
Expected: `MakeFirstNameLastNameNullable` appears in the list.
