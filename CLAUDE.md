# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AppTrack is a job application management system with AI-powered text generation. It is primarily a playground for exploring Clean Architecture, CQRS, and the Mediator pattern in .NET 8.

## Build & Test Commands

```bash
# Restore dependencies
dotnet restore AppTrack.sln

# Build (Release)
dotnet build AppTrack.sln --configuration Release --no-restore

# Run all unit tests
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release

# Run a single unit test (by name filter)
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --filter "FullyQualifiedName~TestNameHere"

# Run persistence integration tests (requires LocalDB)
dotnet test test/AppTrack.Persistance.IntegrationTests/AppTrack.Persistance.IntegrationTests.csproj --configuration Release

# Run API integration tests (requires Docker for Testcontainers)
dotnet test test/AppTrack.Api.IntegrationTests/AppTrack.Api.IntegrationTests.csproj --configuration Release
```

`TreatWarningsAsErrors = true` is enforced globally in `Directory.Build.props` — all compiler warnings are build errors.

## Architecture

The solution follows **Clean Architecture** with these layers (inner layers cannot depend on outer ones):

```
Domain → Application → Infrastructure/Persistence/Identity → API
```

### Projects

| Layer | Project | Role |
|-------|---------|------|
| Domain | `AppTrack.Domain` | Entities, enums, value objects (no dependencies) |
| Application | `AppTrack.Application` | CQRS handlers, DTOs, FluentValidation validators, AutoMapper profiles, interfaces |
| Infrastructure | `AppTrack.Infrastructure` | OpenAI text generation, SendGrid email |
| Persistence | `AppTrack.Persistance` | EF Core DbContext, migrations (auto-applied on startup) |
| Identity | `AppTrack.Identity` | JWT generation/validation, ASP.NET Identity |
| API | `AppTrack.Api` | ASP.NET controllers, middleware, DI wiring |
| WPF Frontend | `AppTrack.WpfUi` | MVVM desktop client using CommunityToolkit.Mvvm + ModernWpfUI |
| Frontend Shared | `AppTrack.Frontend.Models` / `AppTrack.Frontend.ApiService` | Shared DTOs and NSwag-generated API client |

### CQRS & Mediator Pattern

All business operations in `AppTrack.Application` are expressed as **Commands** (mutations) or **Queries** (reads), dispatched via the Mediator pattern. Controllers in `AppTrack.Api` only call `IMediator.Send(...)` — no business logic lives in controllers.

Each feature follows this structure under `AppTrack.Application`:
- `Commands/` — command objects + handlers
- `Queries/` — query objects + handlers
- `DTOs/` — data transfer objects
- `Validators/` — FluentValidation classes
- `MappingProfiles/` — AutoMapper profiles
- `Contracts/` — interfaces consumed by handlers (implemented in Infrastructure/Persistence)

### Authentication

Global authorization policy requires JWT Bearer authentication for all endpoints. Exceptions are marked `[AllowAnonymous]`. The `/health` endpoint is publicly accessible.

### Database

MS SQL Server via EF Core. LocalDB (`Server=(localdb)\MSSQLLocalDB;Database=AppTrack_Local`) for development. Migrations run automatically on startup via `MigrationsHelper.TryApplyDatabaseMigrations` — no manual `dotnet ef database update` needed during development.

### Testing Strategy

- **Unit tests** (`AppTrack.Application.UnitTests`): xUnit + Moq + Shouldly, testing CQRS handlers and validators in isolation via mock repositories.
- **Persistence integration tests** (`AppTrack.Persistance.IntegrationTests`): EF Core InMemory provider.
- **API integration tests** (`AppTrack.Api.IntegrationTests`): `WebApplicationFactory` + Testcontainers (real SQL Server in Docker container).

## Code Style

Enforced via `.editorconfig` and SonarAnalyzer.CSharp:
- Interfaces must be prefixed with `I` (error-level rule)
- Types and non-field members use PascalCase
- 4-space indentation, block-scoped namespaces
- Nullable reference types enabled, implicit usings enabled
- XAML formatted with XAML Styler (`Settings.XamlStyler`)

## Key Configuration

`AppTrack.Api/appsettings.json` (development values — override via user secrets or environment variables in production):
- `ConnectionStrings:DefaultConnection` — LocalDB connection string
- `JwtSettings` — Key, Issuer, Audience, DurationInMinutes
- `OpenAiSettings` — ApiUrl, TimeoutInSeconds (API key stored per-user in DB via AiSettings)
- `EmailSettings` — SendGrid ApiKey, FromAddress

## NuGet Version Management

All NuGet package versions are centrally managed in `Directory.Packages.props` at the solution root. Do not specify versions in individual `.csproj` files.
