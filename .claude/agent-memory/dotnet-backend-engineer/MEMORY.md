# AppTrack Backend Agent Memory

## Authentication Architecture (as of Mar 2026 - branch: RefactoringAuthForCloud)
- Auth provider migrated from custom AppTrack.Identity (ASP.NET Identity + JWT) to Azure AD External Identities (CIAM)
- `AppTrack.Identity` project reference removed from `AppTrack.Api.csproj`
- `Microsoft.Identity.Web` 3.8.2 added; auth configured via `AddMicrosoftIdentityWebApi(config.GetSection("AzureAd"))`
- `Azure.Identity` 1.13.2 + `Azure.Extensions.AspNetCore.Configuration.Secrets` 1.3.2 added for Key Vault support
- Key Vault URI: `https://apptrack-kv.vault.azure.net/` — injected via `DefaultAzureCredential` in non-Development environments
- `appsettings.json` AzureAd section: Instance, TenantId, ClientId, Audience (replaces JwtSettings)
- `AuthenticationController` (login/register) deleted — auth is now handled by Azure AD
- `MigrationsHelper.cs` updated: Identity DB migration call removed, only `AppTrackDatabaseContext` migrated on startup

## Integration Test Architecture (as of Mar 2026)
- `IdentityWebApplicationFactory` deleted (was entirely Identity-dependent, used only by auth controller tests)
- `AuthenticationControllerTests/` folder deleted (LoginTests.cs, RegisterTests.cs — now irrelevant)
- `FakeAuthWebApplicationFactory` updated: removed `AppTrackIdentityDbContext` seeding, only migrates `AppTrackDatabaseContext`
- `ApplicationUserSeedHelper` rewritten as pure in-memory GUID generator (no DbContext dependency)
  - Signature preserved for call-site compatibility: `CreateTestUserAsync(IServiceProvider, string? userName, string? userId)`
  - Used by `UpdateAiSettingsTests`, `UpdateJobApplicationDefaultsTests`, `SeedHelper.cs` — all just need a GUID string as userId
- `TestAuthHandler` uses `"sub"` claim (value `"test-user-id"`) — no `"oid"` claim in test context

## User Scoping Pipeline (as of Mar 2026 - branch: RefactoringAuthForCloud)
- `IUserScopedRequest` (in `AppTrack.Application/Contracts/Mediator/`) — marker with `string UserId { get; set; }`
- `IUserContext` (in `AppTrack.Application/Contracts/`) — abstraction for resolving current user ID
- `HttpContextUserContext` (in `AppTrack.Infrastructure/Identity/`) — reads `"oid"` claim first, falls back to `"sub"`
- `Mediator.Send()` (in `AppTrack.Infrastructure/Mediator/Mediator.cs`) — injects UserId before dispatching handler
- All 9 commands/queries implement `IUserScopedRequest`; controllers never set UserId
- Validators no longer have `NotEmpty`/`Matches` rules for UserId; DB-ownership checks still use `command.UserId`

## Key File Paths
- `AppTrack.Api/AppTrack.Api.csproj` — no AppTrack.Identity ref; has Azure.Identity, Azure.Extensions, Microsoft.Identity.Web
- `AppTrack.Api/Program.cs` — AddMicrosoftIdentityWebApi, Key Vault config
- `AppTrack.Api/Helper/MigrationsHelper.cs` — migrates AppTrackDatabaseContext only
- `AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeAuthWebApplicationFactory.cs` — Identity-free
- `AppTrack.Api.IntegrationTests/SeedData/User/ApplicationUserSeedHelper.cs` — GUID-only, no DbContext
- `AppTrack.Application/Contracts/Mediator/IUserScopedRequest.cs` — marker interface for user-scoped requests
- `AppTrack.Application/Contracts/IUserContext.cs` — abstraction for user ID resolution
- `AppTrack.Infrastructure/Identity/HttpContextUserContext.cs` — reads oid/sub claims
- `AppTrack.Infrastructure/Mediator/Mediator.cs` — custom mediator; injects UserId before dispatch

## Handler Validation Pattern (updated Apr 2026)
- Validators are injected via `IValidator<TRequest>` constructor parameter, stored as `private readonly IValidator<TRequest> _validator`
- `ApplicationServiceRegistration` registers all validators: `services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true)`
- In handlers: `var validationResult = await _validator.ValidateAsync(request, cancellationToken);`
- Check `validationResult.Errors.Count > 0` (not `.Any()`) to avoid LINQ allocation — consistent with TreatWarningsAsErrors style
- `UpsertFreelancerProfileCommandHandler` uses optimistic upsert: `GetByUserIdAsync` → create if null, else `ApplyTo` + update
- `GenerateAiTextCommandHandler` constructor: `IJobApplicationRepository` removed — it was only needed by the old inline validator; injected `IValidator<GenerateAiTextCommand>` handles that now. Sonar S4487 fires on unread private fields (build error).
- Unit tests: use `Mock<IValidator<TRequest>>` with default `.ReturnsAsync(new ValidationResult())`, override per-test for failure paths with `new ValidationResult([new ValidationFailure(...)])`

## FreelancerProfile Feature (Application layer, branch: feature/profile-setup-wizard)
- `IFreelancerProfileRepository` at `AppTrack.Application/Contracts/Persistance/IFreelancerProfileRepository.cs`
- `FreelancerProfileDto` at `AppTrack.Application/Features/FreelancerProfile/Dto/FreelancerProfileDto.cs`
- Command: `UpsertFreelancerProfileCommand` — implements `IFreelancerProfileValidatable` (from Shared.Validation)
- Query: `GetFreelancerProfileQuery` — no validatable interface (only UserId needed, injected by mediator)
- Mappings: `AppTrack.Application/Mappings/FreelancerProfileMappings.cs` — `ToNewDomain`, `ApplyTo`, `ToDto`
- `GetFreelancerProfileQueryHandler` throws `NotFoundException` when no profile found for user

## Infrastructure Project Note
- `AppTrack.Infrastructure` uses `Microsoft.NET.Sdk` (not Web SDK)
- Adding `<FrameworkReference Include="Microsoft.AspNetCore.App" />` grants access to all ASP.NET Core types
- After adding FrameworkReference, individual `Microsoft.Extensions.*` PackageReferences become redundant (NU1510 = build error due to TreatWarningsAsErrors); remove them all — only keep non-framework packages (e.g. SendGrid)

## NuGet Package ID vs. Namespace: PdfPig
- NuGet package ID: `PdfPig` (NOT `UglyToad.PdfPig`) — the `UglyToad.PdfPig` package ID only has pre-release versions
- C# using namespace: `using UglyToad.PdfPig;` — correct, this is the library's namespace inside the `PdfPig` package
- Latest stable version: 0.1.9 (as of Apr 2026; 0.1.14 also available)
- `PdfDocument.Open(stream)` returns an `IDisposable`; use `using var document = ...`

## Namespace vs. Domain Type Collision (recurring gotcha)
- When a handler or test file is nested under `AppTrack.Application.Features.FreelancerProfile.*` (or any other Feature namespace), `FreelancerProfile` resolves as the namespace segment, not `AppTrack.Domain.FreelancerProfile`
- Same issue in test namespace `AppTrack.Application.UnitTests.Features.FreelancerProfile.Commands` — `FreelancerProfile` and `AiSettings` both clash
- Fix: use fully-qualified type names (`AppTrack.Domain.FreelancerProfile`, `AppTrack.Domain.AiSettings`, `AppTrack.Domain.PromptParameter`) instead of a `using AppTrack.Domain;` import in those files
- Private helper methods in handlers must not carry `CancellationToken` unless they actually pass it to an async call — SonarAnalyzer (S1172) treats unused method parameters as errors

## BuiltIn Prompt Prefix Convention (as of Apr 2026 - branch: feature/builtinprompt-parameters)
- Reserved prefix for built-in prompts is `builtIn_` — centralised in `BuiltInParameterKeys.Prefix` (was magic string before Apr 2026 refactor)
- `BuiltInParameterKeys` static class at `AppTrack.Domain/BuiltInParameterKeys.cs` — `Prefix` + 8 named key constants (FirstName, LastName, HourlyRate, DailyRate, AvailableFrom, WorkMode, Skills, CvText)
- `BuiltInPrompt.Create()` guard: uses `BuiltInParameterKeys.Prefix`
- `PromptBaseValidator<T>` and `PromptParameterBaseValidator<T>`: use `BuiltInParameterKeys.Prefix` (OrdinalIgnoreCase) — `AppTrack.Shared.Validation` now references `AppTrack.Domain`
- `GeneratePromptQueryHandler` + `GeneratePromptQueryValidator`: route by `BuiltInParameterKeys.Prefix` (Ordinal) to `IBuiltInPromptRepository`
- Seed data in `BuiltInPromptConfiguration.HasData`: ids 1–4 named `builtIn_Cover_Letter` etc. — full prompt name strings are data values, not replaced by constants
- Migration `20260414193220_RenameDefaultPrefixToBuiltIn` renames ids 1–8 in `DefaultPrompts` table
  - Ids 5–8 (English variants from `AddEnglishDefaultPrompts` raw InsertData) renamed with `_en` suffix to avoid unique index collision

## builtIn_ PromptParameter Sync (updated Apr 2026 - branch: feature/builtinprompt-parameters)
- After profile upsert, handler syncs 7 `builtIn_` keys into `AiSettings.BuiltInPromptParameter` (separate table, not `PromptParameter`)
- `BuiltInPromptParameter` domain entity at `AppTrack.Domain/BuiltInPromptParameter.cs` — FK `AiSettingsId` on `AiSettings`
- `AiSettings` has two collections: `PromptParameter` (user-editable) and `BuiltInPromptParameter` (handler-managed, read-only for user)
- `UpdateAiSettings` command does NOT touch `BuiltInPromptParameter`
- `GeneratePromptQueryHandler` merges both collections: `PromptParameter.Concat(builtInParameters)` before building prompt
- EF config: `BuiltInPromptParameterConfiguration.cs` — Key max 50, Value max 500, unique index (AiSettingsId, Key)
- `AiSettingsRepository.GetByUserIdWithPromptsReadOnlyAsync` and `GetByUserIdWithPromptParameterAsync` both include `BuiltInPromptParameter`
- `AiSettingsDto.BuiltInPromptParameter` added as `List<PromptParameterDto>` — mapped in `AiSettingsMappings.ToDto`
- Migration: `20260414195556_AddBuiltInPromptParameterTable` — includes data migration SQL to move existing `builtIn_%` rows from `PromptParameter`
- `MockAiSettingsRepository.ExistingUserId = "user1"` — constant added to facilitate test setup
- Rule: null/empty field → remove existing param if present; non-empty → add or update in-place

## AI Text Generation Feature (renamed Apr 2026 - branch: feature/update-builtin-application-prompt)
- `IAiTextGenerator` at `AppTrack.Application/Contracts/AiTextGenerator/IAiTextGenerator.cs` — method: `GenerateAiTextAsync`
- `OpenAiAiTextGenerator` at `AppTrack.Infrastructure/AiTextGeneration/OpenAiAiTextGenerator.cs` — implements `IAiTextGenerator`
- `GenerateAiTextCommand` / `GenerateAiTextCommandHandler` / `GenerateAiTextCommandValidator` at `AppTrack.Application/Features/AiSettings/Commands/GenerateAiText/`
- `GeneratedAiTextDto` at `AppTrack.Application/Features/JobApplications/Dto/GeneratedAiTextDto.cs` — property: `GeneratedText`
- DI registration: `services.AddHttpClient<IAiTextGenerator, OpenAiAiTextGenerator>()` in `InfrastructureServicesRegistration`
- `OpenAiOptions` (settings class) moved to `AppTrack.Infrastructure/AiTextGeneration/Settings/OpenAiOptions.cs` — namespace `AppTrack.Infrastructure.AiTextGeneration`
- NOTE: `JobApplication.ApplicationText` domain property was NOT renamed — only the DTO property changed
- NOTE: NSwag-generated `ServiceClient.cs` and frontend `ApplicationTextService.cs` still use old generated names — updated on next NSwag regeneration

## CS9175: Collection Expression Not Allowed in Expression Trees (recurring)
- CS9175: `An expression tree may not contain a collection expression` — affects any code that compiles to expression trees
- Contexts: EF Core `HasConversion` lambdas, Moq `It.Is<T>()` predicate lambdas, any LINQ expression tree
- Fix: replace `[]` or `["a", "b"]` with `new List<string>()` or `new List<string> { "a", "b" }`
- EF example: `v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()`
- Moq example: `It.Is<T>(s => s.Items.SequenceEqual(new List<string> { "a", "b" }))` (not `["a", "b"]`)

## RSS Feed Monitoring Persistence (as of Apr 2026 - branch: feature/rss-feed-monitoring)
- 4 new entities: `RssPortal`, `UserRssSubscription`, `RssMonitoringSettings`, `ProcessedFeedItem`
- EF configurations at `AppTrack.Persistance/Configurations/Rss*.cs` and `ProcessedFeedItemConfiguration.cs`
- `RssPortalConfiguration` seeds one Stepstone portal (Id=1, ParserType stored as string via `HasConversion<string>()`)
- `RssMonitoringSettings.Keywords` stored as JSON (`nvarchar(max)`) via `JsonSerializer` value converter
- `UserRssSubscription`: unique index on (UserId, RssPortalId); FK to RssPortal with cascade delete
- `ProcessedFeedItem`: unique index on (UserId, FeedItemUrl) — prevents duplicate processing
- Repositories at `AppTrack.Persistance/Repositories/Rss*.cs` and `ProcessedFeedItemRepository.cs`
- `UserRssSubscriptionRepository.GetActiveSubscriptionsWithPortalsAsync`: no `.AsNoTracking()` — entity may be updated by background service
- `RssMonitoringSettingsRepository.UpsertAsync` updates Keywords, PollIntervalMinutes, AND NotificationEmail on existing record
- Migration: `20260422091653_AddRssFeedMonitoring`

## CV Storage Feature (Infrastructure layer, branch: feature/profile-setup-wizard)
- `AzureStorageSettings` at `AppTrack.Infrastructure/CvStorage/AzureStorageSettings.cs` — ConnectionString + ContainerName
- `AzureBlobStorageService` implements `ICvStorageService` — creates `BlobServiceClient` per call (stateless, scoped DI)
- `PdfPigTextExtractor` implements `IPdfTextExtractor` — registered as Singleton (stateless)
- Blob path convention: `{userId}/cv.pdf` (one CV per user, overwrite on re-upload)
- Dev appsettings: `"ConnectionString": "UseDevelopmentStorage=true"` (Azurite), `"ContainerName": "cv-uploads"`
