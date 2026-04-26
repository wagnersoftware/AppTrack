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

## Key Rules
- `TreatWarningsAsErrors = true` — zero warnings tolerated, build must be clean
- NuGet versions are NOT in `.csproj` files — all managed in `Directory.Packages.props` at solution root
- `UserId` is always set by backend from JWT; never validate it in command validators
- Do NOT pass `null!` for non-nullable record/class members to avoid nullable warning; use `""` or a valid substitute instead
