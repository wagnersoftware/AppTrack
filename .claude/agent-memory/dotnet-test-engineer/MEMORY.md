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

## Key Rules
- `TreatWarningsAsErrors = true` — zero warnings tolerated, build must be clean
- NuGet versions are NOT in `.csproj` files — all managed in `Directory.Packages.props` at solution root
- `UserId` is always set by backend from JWT; never validate it in command validators
