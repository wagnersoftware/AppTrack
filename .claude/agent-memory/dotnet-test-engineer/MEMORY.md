# Test Engineer Agent Memory

## Project Layout
- Unit test project root: `AppTrack.Application.UnitTests/` (NOT under `test/`)
- Persistence integration tests: `test/AppTrack.Persistance.IntegrationTests/`
- Feature tests follow mirror structure: `Features/<Feature>/Commands/` and `Features/<Feature>/Queries/`
- Reusable mocks live in `AppTrack.Application.UnitTests/Mocks/`

## FluentValidation.TestHelper
- `FluentValidation.TestHelper` is available as a transitive dependency (via `AppTrack.Application` -> `FluentValidation 12.0.0`)
- No need to add an explicit `<PackageReference>` for it in the unit test `.csproj`
- Use `await _validator.TestValidateAsync(command)` (async) for all validator tests

## Shared Validation Base Validators
- `FreelancerProfileBaseValidator<T>` in `AppTrack.Shared.Validation/Validators/` validates:
  - `FirstName`/`LastName`: NotEmpty + MaximumLength(100)
  - `HourlyRate`/`DailyRate`: GreaterThan(0) When HasValue
  - `Skills`: MaximumLength(1000) When not null
- `UpsertFreelancerProfileCommandValidator` inherits this base validator with no additional rules

## Standard Validator Test Pattern
```csharp
private readonly SomeValidator _validator = new();  // no DI needed for pure validators

private static SomeCommand ValidCommand() => new() { ... };

[Fact]
public async Task Validate_ShouldPass_WhenCommandIsValid()
{
    var result = await _validator.TestValidateAsync(ValidCommand());
    result.IsValid.ShouldBeTrue();
}

[Fact]
public async Task Validate_ShouldHaveError_WhenXIsY()
{
    var command = ValidCommand();
    command.Property = badValue;
    var result = await _validator.TestValidateAsync(command);
    result.ShouldHaveValidationErrorFor(x => x.Property);
}
```

## Namespace Collision: FreelancerProfile Type vs. Namespace Segment
- Test files under `Features/FreelancerProfile/Commands/` have namespace `AppTrack.Application.UnitTests.Features.FreelancerProfile.Commands`
- The segment `FreelancerProfile` shadows the `AppTrack.Domain.FreelancerProfile` entity type
- `using AppTrack.Domain;` does NOT resolve this — the compiler still sees `FreelancerProfile` as the namespace segment
- Fix: use fully qualified type name `AppTrack.Domain.FreelancerProfile` in all `It.Is<>` and `It.IsAny<>` calls
- Do NOT add `using AppTrack.Domain;` in these test files (it is unnecessary and confusing)

## IBuiltInPromptRepository: GetAsync → GetByLanguageAsync
- `IBuiltInPromptRepository.GetAsync()` was replaced by `GetByLanguageAsync(string languageCode)` where `languageCode` is `"en"` or `"de"` derived from `AiSettings.Language`
- Any test that mocked `r.GetAsync()` on `IBuiltInPromptRepository` must be updated to `r.GetByLanguageAsync(It.IsAny<string>())` (or a specific code string if verifying routing)
- Affected test files: `GeneratePromptQueryHandlerTests.cs`, `GeneratePromptQueryValidatorTests.cs`, `GetAiSettingsByUserIdQueryHandlerTests.cs`
- Default (fallback) constructor setup pattern: `.Setup(r => r.GetByLanguageAsync(It.IsAny<string>())).ReturnsAsync(new List<BuiltInPrompt>())`

## Persistence Integration Tests: InMemory Pattern
- `test/AppTrack.Persistance.IntegrationTests/` was empty until Apr 2026 — bootstrapped with a `.csproj` referencing `Microsoft.EntityFrameworkCore.InMemory` + Shouldly + xunit
- Use a unique DB name per test method (pass `nameof(TestMethod)` to `UseInMemoryDatabase`) for full isolation — no teardown needed
- Call `context.Database.EnsureCreated()` to apply `HasData` seeds from `IEntityTypeConfiguration` classes
- `ProjectPortalConfiguration` seeds `ProjectPortal { Id = 1, Name = "Freelancermap", ... }` — available in every InMemory test after `EnsureCreated()`
- Repository under test is instantiated directly with the test context: `new ScrapedProjectRepository(context)`
- No Moq needed; exercise the real EF Core repository against real InMemory storage

## ScrapedProjectRepository.AddNewForPortalAsync: Tested Behaviours
- New projects are inserted
- URL-based deduplication skips existing URLs for the same portalId
- Deduplication is case-insensitive (HashSet with OrdinalIgnoreCase)
- Cross-portal: same URL on a different portalId is NOT treated as duplicate
- All-duplicate input triggers early return without inserting anything
- `Description` (nvarchar(max)) is persisted correctly

## Testing Infrastructure-Layer Classes (e.g. FreelancermapScraper)
- `FreelancermapScraper` is in `AppTrack.Infrastructure` — requires adding a `<ProjectReference>` to Infrastructure in the unit test `.csproj`
- Infrastructure uses `<FrameworkReference Include="Microsoft.AspNetCore.App" />` — transitive pull into a `Microsoft.NET.Sdk` test project is fine on net10.0
- Use a `FakeHttpMessageHandler : HttpMessageHandler` (inner sealed class) that maps URL strings to response bodies via `Dictionary<string, string>`; unmapped URLs return 404 so `HttpClient.GetStringAsync` throws `HttpRequestException`
- Construct scraper directly: `new FreelancermapScraper(new HttpClient(handler), NullLogger<FreelancermapScraper>.Instance)`
- `NullLogger<T>.Instance` is from `Microsoft.Extensions.Logging.Abstractions` — no package reference needed, it is transitively available
- Key scenarios: happy path, card missing title link (skipped), detail fetch 404 (empty description), detail page without `.ql-editor` (empty description), relative href resolved to absolute URL

## ScrapedProject Entity: No ScrapedAt Property
- `ScrapedProject` domain entity does NOT have a `ScrapedAt` property (it was removed)
- Do not set `ScrapedAt` in `MakeProject` helpers or anywhere else in tests

## IUnitOfWork.ExecuteInTransactionAsync Mock Pattern
- Use `.Returns((Func<CancellationToken, Task> action, CancellationToken ct) => action(ct))` so the callback actually executes
- Constructor-level setup is fine: set it up in the test class constructor so all tests in the class benefit
- Verify it was called with `Times.Once` to confirm transactional wrapping

## Handler Tests: Injecting Real Validators (no mock needed)
- When a handler takes `IValidator<TCommand>` and the validator has no dependencies, inject the real validator directly
- Example: `new UpdateProjectMonitoringSettingsCommandHandler(_repo.Object, new UpdateProjectMonitoringSettingsCommandValidator())`
- This is simpler than `Mock<IValidator<T>>` and exercises the real validation logic
- Test `BadRequestException` thrown on invalid input using `await Should.ThrowAsync<BadRequestException>(() => handler.Handle(...))`

## Unit Type in Test Assertions
- `Unit` struct lives in `AppTrack.Application.Shared` — add `using AppTrack.Application.Shared;`
- Assert: `result.ShouldBe(Unit.Value);`

## [JsonIgnore] Fields on Commands
- Fields decorated `[JsonIgnore]` (e.g., `UserId`, `NotificationEmail` on `UpdateProjectMonitoringSettingsCommand`) are set by the backend, not from JSON
- In handler tests, set these fields directly on the command object — they flow through to the domain entity normally
- The validator for `UpdateProjectMonitoringSettingsCommand` DOES validate `NotificationEmail` with `NotEmpty()` even though it is `[JsonIgnore]`

## Two Persistence Integration Test Projects (both exist)
- Root-level: `AppTrack.Persistance.IntegrationTests/` — in solution, has `AppTrackDatabaseContextTests`, seed tests
- `test/AppTrack.Persistance.IntegrationTests/` — NOT in solution, has `Repositories/` subfolder with repository integration tests
- New repository tests go in `test/AppTrack.Persistance.IntegrationTests/Repositories/`
- Run: `dotnet test test/AppTrack.Persistance.IntegrationTests/AppTrack.Persistance.IntegrationTests.csproj`

## ScrapingScheduleRepository: Singleton Row Pattern
- Always reads/writes `Id = 1` (private `SingletonId = 1`)
- `GetNextRunAfterAsync` returns `null` when no row exists (normal first-run state)
- `SetNextRunAfterAsync` inserts if missing, updates if present — upsert pattern
- Round-trip test: `SetNextRunAfterAsync` then `GetNextRunAfterAsync` confirms persistence

## ScrapedProjectRepository.GetExistingUrlsForPortalAsync: Tested Behaviours
- Empty set returned when no projects exist for the portal
- Only returns URLs for the specified portalId (not other portals)
- Returns a case-insensitive `HashSet<string>` (OrdinalIgnoreCase) — test by checking lookup with different casing

## ScrapePortalsFunction: Time-Window Unit Testing Limitation
- `ScrapePortalsFunction.Run` uses `DateTime.UtcNow` directly (not injectable `TimeProvider`)
- Operating window: 09:00–17:00 CET. Tests that require the function body to execute are time-sensitive
- Use `[Trait("Category", "BusinessHoursDependent")]` to mark time-sensitive tests
- Deterministic tests: verify mediator NOT called when `GetNextRunAfterAsync` returns far-future time (skips regardless of gate that fires)
- New test project: `AppTrack.Functions.UnitTests/` — references `AppTrack.Functions` directly
- `TimerInfo` has a parameterless constructor — use `new TimerInfo()` in test setup; `Run()` does not use the timer parameter

## AppTrack.Functions.UnitTests: Project Setup
- Location: `AppTrack.Functions.UnitTests/` at repo root (added to `AppTrack.sln`)
- References `AppTrack.Functions.csproj` which has `OutputType=Exe` — referencing Exe projects is fine for testing
- No extra packages needed; Moq + xunit + Shouldly follow same pattern as other test projects

## ScrapingResult Factory Methods: Key Contracts
- `Success(items, listingItemCount)`: `ListingSucceeded=true`, `ErrorMessage=null`, items/count set independently
- `Failure(errorMessage)`: `ListingSucceeded=false`, `Items=[]`, `ListingItemCount=0`, `ErrorMessage` set
- `listingItemCount` counts total listed items, `items.Count` may be less (known URLs filtered before detail fetch)

## FreelancermapScraper: Warm-Up and Delay Behaviour
- Warm-up GET to `scheme://host/` fires before listing fetch; failure is swallowed (not a `ScrapingResult.Failure`)
- Delay count: 1 warm-up delay + (N-1) between-detail delays for N new items (first detail has no preceding delay)
- All-known-URL input: only warm-up delay fires (no detail fetches, no between-page delays)

## Key Rules
- `TreatWarningsAsErrors = true` — zero warnings tolerated, build must be clean
- NuGet versions are NOT in `.csproj` files — all managed in `Directory.Packages.props` at solution root
- `UserId` is always set by backend from JWT; never validate it in command validators
- Do NOT pass `null!` for non-nullable record/class members to avoid nullable warning; use `""` or a valid substitute instead
