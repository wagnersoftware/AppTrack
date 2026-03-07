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

## Key File Paths
- `AppTrack.Api/AppTrack.Api.csproj` — no AppTrack.Identity ref; has Azure.Identity, Azure.Extensions, Microsoft.Identity.Web
- `AppTrack.Api/Program.cs` — AddMicrosoftIdentityWebApi, Key Vault config
- `AppTrack.Api/Helper/MigrationsHelper.cs` — migrates AppTrackDatabaseContext only
- `AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeAuthWebApplicationFactory.cs` — Identity-free
- `AppTrack.Api.IntegrationTests/SeedData/User/ApplicationUserSeedHelper.cs` — GUID-only, no DbContext
