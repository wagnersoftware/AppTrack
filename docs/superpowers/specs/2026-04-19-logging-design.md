# Logging Architecture — AppTrack

**Date:** 2026-04-19
**Branch:** feature/structured-logging
**Status:** Draft

---

## Overview

This document describes the structured logging architecture for AppTrack. The goal is to provide semantic, enriched, centralized logging with correlation IDs, environment-aware sinks, PII protection, and controllable cost in Azure.

---

## Requirements

| Requirement | Decision |
|---|---|
| Semantic / structured logging | Serilog with JSON output |
| Correlation ID | W3C TraceId via `Activity.Current.TraceId` |
| Log levels | Debug (dev), Warning+ (prod); Microsoft.* always Warning |
| Enrichment | CorrelationId, RequestPath, RequestMethod, StatusCode, Environment, UserId (GUID only) |
| No sensitive data in logs | PII Destructuring Policy + no request body logging |
| Centralized logging | Application Insights (prod), File + Console (dev) |
| Cost control | Log level floor, Daily Cap, environment-variable-based sink activation |
| Local dev output | Console (colored) + rolling daily JSON file |

---

## Architecture

```
AppTrack.Api
├── Program.cs
│   ├── Log.Logger = Serilog bootstrap logger (before host build)
│   ├── builder.Host.UseSerilog()
│   └── app.UseSerilogRequestLogging()   ← outermost middleware; registered first in app pipeline
│
├── Middleware/
│   └── ExceptionMiddleware (unchanged — uses IAppLogger<T>)
│
└── Controllers → Handlers → IAppLogger<T>
                                   │
                                   ▼
                          LoggingAdapter<T> (AppTrack.Infrastructure)
                                   │
                                   ▼
                          Microsoft ILogger<T>
                                   │
                                   ▼
                              Serilog Core
                     ┌─────────────────────────────┐
         Dev:        │ ConsoleSink (colored)        │
                     │ FileSink (rolling, JSON)     │
         Prod:       │ ApplicationInsightsSink      │
                     └─────────────────────────────┘
```

The existing `IAppLogger<T>` abstraction is **preserved**. `LoggingAdapter<T>` continues to wrap `ILogger<T>` — no changes to handlers or the Application layer are required.

---

## NuGet Packages

New packages are added with **inline versions** in `AppTrack.Api.csproj`, consistent with the existing project convention (only `SonarAnalyzer.CSharp` is currently in `Directory.Packages.props`).

| Package | Minimum Version | Purpose |
|---|---|---|
| `Serilog.AspNetCore` | 8.0.0 | HTTP request logging, ASP.NET Core host integration |
| `Serilog.Enrichers.Environment` | 3.0.0 | `Environment` property enricher |
| `Serilog.Sinks.File` | 6.0.0 | Local rolling file sink |
| `Serilog.Sinks.ApplicationInsights` | 5.0.1 | Azure Monitor sink (targets `Microsoft.ApplicationInsights` ≥ 2.23.0 < 3.0.0; compatible with .NET 10) |

`Serilog.Enrichers.Process` and `Serilog.Enrichers.Thread` are **not** included — `ProcessId` and `ThreadId` provide no diagnostic value for this single-process application.

---

## Enrichment

The following properties are automatically attached to every log event:

| Property | Source | Example |
|---|---|---|
| `CorrelationId` | `Activity.Current?.TraceId` (W3C); `"none"` if no active Activity | `4bf92f3577b34da6a3ce929d0e0e4736` |
| `RequestPath` | HTTP Context | `/api/jobapplications` |
| `RequestMethod` | HTTP Context | `GET` |
| `StatusCode` | HTTP Context | `200` |
| `Environment` | `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` |
| `UserId` | JWT Claim `sub` (GUID only) | `a3c4e...` |

`MachineName` is intentionally **excluded** — it provides no value for single-instance deployments.

### UserId Enrichment

`UserId` is pushed into the Serilog diagnostic context via the `EnrichDiagnosticContext` callback of `UseSerilogRequestLogging`:

```csharp
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var userId = httpContext.User.FindFirst("sub")?.Value;
        if (userId is not null)
            diagnosticContext.Set("UserId", userId);
        diagnosticContext.Set("Environment",
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown");
    };
});
```

In non-HTTP contexts (background jobs, hosted services), `UserId` is simply absent from log events — no fallback is needed.

### CorrelationId Null Guard

The `CorrelationId` enricher reads `Activity.Current?.TraceId`. During application startup (before the HTTP pipeline is active), `Activity.Current` is `null`. The enricher must guard against this:

```csharp
var correlationId = Activity.Current?.TraceId.ToString() ?? "none";
diagnosticContext.Set("CorrelationId", correlationId);
```

A bootstrap log event (e.g., "Application starting") will emit `CorrelationId: "none"`, which is acceptable.

### IAppLogger\<T\> Debug Limitation

`IAppLogger<T>` only exposes `LogInformation`, `LogWarning`, and `LogError`. The `Debug` log level is therefore only emitted by Serilog's own infrastructure (e.g., `UseSerilogRequestLogging`) — not by application handler code. This is a known pre-existing limitation. Extending `IAppLogger<T>` with `LogDebug` is out of scope for this implementation.

---

## PII Masking

The following fields must **never** appear in log output:

- Email addresses
- Passwords and API keys
- JWT / Bearer tokens
- User names (given name, family name, display name)

**Implementation strategy:**

1. **No request body logging** — `UseSerilogRequestLogging` only logs path, method, status code, and duration. Request/response bodies are never captured.
2. **Authorization header excluded** — the `Authorization` header is explicitly removed from logged HTTP headers via `RequestLoggingOptions`.
3. **Destructuring Policy** — a custom `IDestructuringPolicy` registered in `AppTrack.Api` masks known sensitive property names (`Password`, `ApiKey`, `Token`, `Email`, `Name`, `GivenName`, `FamilyName`) by replacing their values with `[REDACTED]`.
4. **UserId from JWT only** — the `UserId` enricher reads only the `sub` claim (a GUID), never email or name claims.

---

## Log Levels

| Level | Development | Production |
|---|---|---|
| `Debug` | ✓ (framework/infra only, see limitation above) | — |
| `Information` | ✓ | — |
| `Warning` | ✓ | ✓ |
| `Error` | ✓ | ✓ |
| `Fatal` | ✓ | ✓ |
| `Microsoft.*` | `Warning` | `Warning` |
| `System.*` | `Warning` | `Warning` |

---

## Sinks per Environment

### Development

- **Console sink** — colored, human-readable output in terminal
- **File sink** — rolling daily, JSON Lines format (`Serilog.Formatting.Compact`)
  - Path: `logs/apptrack-.log` (relative to process working directory — intentionally dev-only)
  - Retention: 7 days
  - Format: one JSON object per line (queryable with `jq`)

### Production

- **Application Insights sink** — structured telemetry to Azure Monitor
- No console sink, no file sink

---

## Middleware Ordering

In ASP.NET Core, middleware registered first in `Program.cs` is the outermost wrapper — it executes first on the request path and last on the response path. The exact registration order:

```csharp
// 1. Serilog request logging — outermost wrapper.
//    Calls next(), waits for the pipeline to complete, then logs the summary.
//    ExceptionMiddleware (registered below) catches exceptions and sets the final
//    status code before control returns here — so Serilog sees the correct 4xx/5xx.
app.UseSerilogRequestLogging(options => { ... });

// 2. Exception middleware — catches all unhandled exceptions, sets status codes.
app.UseMiddleware<ExceptionMiddleware>();

// 3. All other middleware (auth, routing, controllers, etc.)
```

There is no double-logging: `UseSerilogRequestLogging` logs the **HTTP request summary** (path, method, status code, duration); `ExceptionMiddleware` logs the **exception details** (type, message, stack trace). These are complementary, non-overlapping concerns.

---

## Configuration

### `appsettings.json` (base)

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  }
}
```

### `appsettings.Development.json`

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Debug"
  }
}
```

### `appsettings.Production.json`

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Warning"
  }
}
```

### Sink Activation

The Application Insights sink is registered **only if** the environment variable `APPLICATIONINSIGHTS_CONNECTION_STRING` is set. If it is absent (e.g., to disable Azure logging for cost reasons), the sink is silently skipped — no code change required.

---

## Cost Control (Azure)

| Mechanism | Configuration | Effect |
|---|---|---|
| Log level `Warning+` in prod | `appsettings.Production.json` | **Primary** cost control — eliminates all `Info`/`Debug` telemetry before it reaches the sink |
| Adaptive Sampling | Enabled by default in App Insights SDK | Safety net — reduces request trace volume if the level floor is ever lowered; has no effect at the current `Warning` floor |
| Daily Ingestion Cap | Set in Azure Portal (e.g., 1 GB/day) | Hard stop on ingestion when cap is reached |
| Sink deactivation | Remove `APPLICATIONINSIGHTS_CONNECTION_STRING` env var | Completely disables Azure logging without code change |

---

## Integration Test Impact

The project's API integration tests (`AppTrack.Api.IntegrationTests`) use `WebApplicationFactory<Program>`. To prevent Serilog sinks from writing to files or Azure during test runs, `builder.UseEnvironment("Test")` is added **only to `FakeAuthWebApplicationFactory`** (the base class):

```csharp
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.UseEnvironment("Test");   // ← add this line
    builder.UseSetting("OpenAiSettings:ApiKey", "test-api-key-integration");
    // ... rest of existing configuration
}
```

`FakeAiTextWebApplicationFactory` and `FakeCvStorageWebApplicationFactory` both call `base.ConfigureWebHost(builder)` and therefore inherit the `UseEnvironment("Test")` call automatically — no changes are required in those subclasses.

The Serilog configuration in `Program.cs` checks this environment and skips all sinks:

```csharp
var env = builder.Environment.EnvironmentName;

loggerConfig
    .WriteTo.Conditional(_ => env != "Test",
        wt => wt.Console())                  // dev only
    .WriteTo.Conditional(_ => env == "Development",
        wt => wt.File(...))                  // dev only
    .WriteTo.Conditional(_ => env == "Production"
                              && connectionString is not null,
        wt => wt.ApplicationInsights(...));  // prod only
```

Note: this check applies to the **main Serilog configuration** (after `builder.Build()`). The **bootstrap logger** (created before `WebApplication.CreateBuilder`) uses a minimal console-only configuration unconditionally and is replaced by the main logger before any test code runs.

---

## Implementation Scope

Changes are confined to:

1. **`AppTrack.Api/Program.cs`** — Serilog bootstrap logger, `builder.Host.UseSerilog()`, register `UseSerilogRequestLogging` as the **first** middleware (before the existing `UseMiddleware<ExceptionMiddleware>()`)
2. **`AppTrack.Api`** — new `PiiDestructuringPolicy.cs`, updated `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`
3. **`AppTrack.Api.csproj`** — new package references with inline versions
4. **`AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeAuthWebApplicationFactory.cs`** — add `builder.UseEnvironment("Test")` to `ConfigureWebHost`

**No changes required in:**
- `AppTrack.Application` (handlers use `IAppLogger<T>` — unchanged)
- `AppTrack.Infrastructure` (`LoggingAdapter<T>` — unchanged)
- Any other layer
