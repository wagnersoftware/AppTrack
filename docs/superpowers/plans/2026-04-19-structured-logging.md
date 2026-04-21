# Structured Logging Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the bare `Microsoft.Extensions.Logging` backend with Serilog, adding structured enrichment, PII masking, a rolling file sink for development, and an Application Insights sink for production.

**Architecture:** Serilog is wired as the `ILogger<T>` backend via `builder.Host.UseSerilog()` — no changes to the Application or Infrastructure layers are needed. Two new utility classes (`CorrelationIdEnricher`, `PiiDestructuringPolicy`) live in `AppTrack.Api/Logging/`. All sink selection is environment-driven; no sink fires during integration tests.

**Tech Stack:** Serilog 4.x, Serilog.AspNetCore 8.x, Serilog.Sinks.File 6.x, Serilog.Sinks.ApplicationInsights 5.x, .NET 10

---

## File Map

| Action | File | Responsibility |
|---|---|---|
| Create | `AppTrack.Api/Logging/CorrelationIdEnricher.cs` | Attaches `CorrelationId` (W3C TraceId) to every log event |
| Create | `AppTrack.Api/Logging/PiiDestructuringPolicy.cs` | Replaces known PII properties with `[REDACTED]` when Serilog destructs objects |
| Modify | `AppTrack.Api/AppTrack.Api.csproj` | Add 3 Serilog package references (`Serilog.Enrichers.Environment` is NOT added — `Enrich.WithProperty` is used directly, making the package unnecessary) |
| Modify | `AppTrack.Api/Program.cs` | Bootstrap logger, `UseSerilog`, middleware reorder, `UseSerilogRequestLogging` |
| Modify | `AppTrack.Api/appsettings.json` | Replace `Logging` section with `Serilog` section |
| Modify | `AppTrack.Api/appsettings.Development.json` | Override `MinimumLevel.Default` to `Debug` |
| Modify | `AppTrack.Api/appsettings.Production.json` | Override `MinimumLevel.Default` to `Warning` |
| Modify | `AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeAuthWebApplicationFactory.cs` | Add `builder.UseEnvironment("Test")` to suppress sinks in tests |

---

## Chunk 1: NuGet Packages

### Task 1: Add Serilog packages to AppTrack.Api.csproj

**Files:**
- Modify: `AppTrack.Api/AppTrack.Api.csproj`

- [ ] **Step 1: Add the three package references**

  Open `AppTrack.Api/AppTrack.Api.csproj` and add inside the existing `<ItemGroup>`:

  ```xml
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
  <PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
  <PackageReference Include="Serilog.Sinks.ApplicationInsights" Version="5.0.1" />
  ```

- [ ] **Step 2: Restore and build to verify packages resolve**

  ```bash
  dotnet restore AppTrack.Api/AppTrack.Api.csproj
  dotnet build AppTrack.Api/AppTrack.Api.csproj --configuration Release --no-restore
  ```

  Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 3: Commit**

  ```bash
  git add AppTrack.Api/AppTrack.Api.csproj
  git commit -m "feat: add Serilog NuGet packages to AppTrack.Api"
  ```

---

## Chunk 2: Logging Utility Classes

### Task 2: Create CorrelationIdEnricher

**Files:**
- Create: `AppTrack.Api/Logging/CorrelationIdEnricher.cs`

- [ ] **Step 1: Create the file**

  ```csharp
  using System.Diagnostics;
  using Serilog.Core;
  using Serilog.Events;

  namespace AppTrack.Api.Logging;

  public class CorrelationIdEnricher : ILogEventEnricher
  {
      public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
      {
          var correlationId = Activity.Current?.TraceId.ToString() ?? "none";
          logEvent.AddPropertyIfAbsent(
              propertyFactory.CreateProperty("CorrelationId", correlationId));
      }
  }
  ```

- [ ] **Step 2: Build to verify it compiles**

  ```bash
  dotnet build AppTrack.Api/AppTrack.Api.csproj --configuration Release
  ```

  Expected: `Build succeeded. 0 Error(s).`

### Task 3: Create PiiDestructuringPolicy

**Files:**
- Create: `AppTrack.Api/Logging/PiiDestructuringPolicy.cs`

- [ ] **Step 1: Create the file**

  ```csharp
  using Serilog.Core;
  using Serilog.Events;

  namespace AppTrack.Api.Logging;

  public class PiiDestructuringPolicy : IDestructuringPolicy
  {
      private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
      {
          "Password", "ApiKey", "Token", "Email", "Name", "GivenName", "FamilyName"
      };

      public bool TryDestructure(
          object value,
          ILogEventPropertyValueFactory propertyValueFactory,
          out LogEventPropertyValue result)
      {
          result = null!;
          if (value is null) return false;

          var properties = value.GetType().GetProperties();
          if (!properties.Any(p => SensitiveFields.Contains(p.Name)))
              return false;

          var logProperties = properties.Select(p =>
          {
              LogEventPropertyValue propValue = SensitiveFields.Contains(p.Name)
                  ? new ScalarValue("[REDACTED]")
                  : propertyValueFactory.CreatePropertyValue(p.GetValue(value), true);
              return new LogEventProperty(p.Name, propValue);
          });

          result = new StructureValue(logProperties);
          return true;
      }
  }
  ```

- [ ] **Step 2: Build to verify it compiles**

  ```bash
  dotnet build AppTrack.Api/AppTrack.Api.csproj --configuration Release
  ```

  Expected: `Build succeeded. 0 Error(s).`

- [ ] **Step 3: Commit**

  ```bash
  git add AppTrack.Api/Logging/CorrelationIdEnricher.cs AppTrack.Api/Logging/PiiDestructuringPolicy.cs
  git commit -m "feat: add CorrelationIdEnricher and PiiDestructuringPolicy"
  ```

---

## Chunk 3: Serilog Configuration in Program.cs

### Task 4: Add Serilog bootstrap logger and UseSerilog

**Files:**
- Modify: `AppTrack.Api/Program.cs`

The current `Program.cs` has no try/catch around startup. This task wraps it in the standard Serilog bootstrap pattern and wires `UseSerilog`.

- [ ] **Step 1: Add using directives at the top of Program.cs**

  Add these usings (after the existing using block):

  ```csharp
  using AppTrack.Api.Logging;
  using Serilog;
  using Serilog.Formatting.Compact;
  using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
  ```

- [ ] **Step 2: Add bootstrap logger and wrap app in try/catch/finally**

  Replace the entire content of `Program.cs` with the following. The full existing service registration and middleware are preserved; only the Serilog bootstrap and try/catch wrapper are new:

  ```csharp
  using AppTrack.Api.Helper;
  using AppTrack.Api.Logging;
  using AppTrack.Api.Middleware;
  using AppTrack.Application;
  using AppTrack.Infrastructure;
  using AppTrack.Infrastructure.AiTextGeneration;
  using AppTrack.Persistance;
  using Azure.Identity;
  using Microsoft.AspNetCore.Authentication.JwtBearer;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Server.Kestrel.Core;
  using Microsoft.Identity.Web;
  using Microsoft.OpenApi.Models;
  using Serilog;
  using Serilog.Formatting.Compact;
  using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

  // Bootstrap logger — active only until the host is built.
  // Replaced by the full Serilog configuration below via UseSerilog().
  Log.Logger = new LoggerConfiguration()
      .WriteTo.Console()
      .CreateBootstrapLogger();

  try
  {
      var builder = WebApplication.CreateBuilder(args);

      // ── Serilog ───────────────────────────────────────────────────────────
      builder.Host.UseSerilog((context, services, loggerConfig) =>
      {
          var env = context.HostingEnvironment.EnvironmentName;
          var appInsightsConnStr =
              context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
              ?? Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

          loggerConfig
              .ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
              .Enrich.FromLogContext()
              .Enrich.With<CorrelationIdEnricher>()
              .Destructure.With<PiiDestructuringPolicy>()
              // Console: all environments except Test (suppressed in Test to keep test output clean)
              .WriteTo.Conditional(_ => env != "Test",
                  wt => wt.Console())
              // File: dev only, rolling daily JSON, 7-day retention
              .WriteTo.Conditional(_ => env == "Development",
                  wt => wt.File(
                      new CompactJsonFormatter(),
                      "logs/apptrack-.log",
                      rollingInterval: RollingInterval.Day,
                      retainedFileCountLimit: 7))
              // Application Insights: prod only, only when connection string is present
              .WriteTo.Conditional(
                  _ => env == "Production" && appInsightsConnStr is not null,
                  wt => wt.ApplicationInsights(
                      appInsightsConnStr,
                      TelemetryConverter.Traces));
      });
      // ─────────────────────────────────────────────────────────────────────

      if (!builder.Environment.IsDevelopment())
      {
          var keyVaultUri = builder.Configuration["KeyVaultUri"];
          if (!string.IsNullOrEmpty(keyVaultUri))
          {
              builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
          }
      }

      builder.Services.AddHealthChecks();

      builder.Services.AddApplicationServices();
      builder.Services.AddInfrastructureServices(builder.Configuration);
      builder.Services.AddPersistanceServices(builder.Configuration);

      builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddMicrosoftIdentityWebApi(
              jwtBearerOptions =>
              {
                  jwtBearerOptions.MapInboundClaims = false;

                  var instance = builder.Configuration["AzureAd:Instance"]?.TrimEnd('/');
                  var tenantId = builder.Configuration["AzureAd:TenantId"];
                  jwtBearerOptions.MetadataAddress =
                      $"{instance}/{tenantId}/v2.0/.well-known/openid-configuration";
              },
              microsoftIdentityOptions =>
              {
                  builder.Configuration.GetSection("AzureAd").Bind(microsoftIdentityOptions);
              });

      builder.Services.AddControllers();

      var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];

      builder.Services.AddCors(options =>
      {
          options.AddPolicy("All", policy => policy
              .WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
      });

      builder.Services.AddOptions<OpenAiOptions>()
          .Bind(builder.Configuration.GetSection("OpenAiSettings"))
          .ValidateDataAnnotations()
          .ValidateOnStart();

      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen(c =>
      {
          c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
          c.UseAllOfToExtendReferenceSchemas();
          c.SchemaFilter<AppTrack.Api.Swagger.NullableEnumSchemaFilter>();

          c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
          {
              Name = "Authorization",
              Type = SecuritySchemeType.Http,
              Scheme = "Bearer",
              BearerFormat = "JWT",
              In = ParameterLocation.Header,
              Description = "Bearer + token value"
          });

          c.AddSecurityRequirement(new OpenApiSecurityRequirement
          {
              {
                  new OpenApiSecurityScheme
                  {
                      Reference = new OpenApiReference
                      {
                          Type = ReferenceType.SecurityScheme,
                          Id = "Bearer"
                      }
                  },
                  new string[] {}
              }
          });
      });

      // Global Authorization Policy
      builder.Services.AddAuthorization(options =>
      {
          options.FallbackPolicy = new AuthorizationPolicyBuilder()
              .RequireAuthenticatedUser()
              .Build();
      });

      // Remove the "Server" header from responses for security hardening
      builder.Services.PostConfigure<KestrelServerOptions>(options =>
      {
          options.AddServerHeader = false;
      });

      builder.Host.UseDefaultServiceProvider(options =>
      {
          options.ValidateOnBuild = true;
      });

      var app = builder.Build();

      // ── Middleware pipeline ───────────────────────────────────────────────
      // UseSerilogRequestLogging must be registered FIRST (outermost wrapper).
      // It calls next(), waits for the full pipeline to complete, then logs
      // the HTTP summary — so it observes the final status code set by
      // ExceptionMiddleware below.
      app.UseSerilogRequestLogging(options =>
      {
          options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
          {
              var userId = httpContext.User.FindFirst("sub")?.Value;
              if (userId is not null)
                  diagnosticContext.Set("UserId", userId);
              // Note: "Environment" is NOT set here — it is already attached to
              // every log event globally via .Enrich.WithProperty("Environment", ...)
              // in UseSerilog above. Setting it here again would be redundant.
          };
      });
      app.UseMiddleware<ExceptionMiddleware>();
      // ─────────────────────────────────────────────────────────────────────

      await MigrationsHelper.TryApplyDatabaseMigrations(app);

      if (app.Environment.IsDevelopment())
      {
          app.UseSwagger();
          app.UseSwaggerUI();
      }

      app.UseCors("All");
      app.UseAuthentication();
      app.UseAuthorization();
      app.MapControllers();
      app.MapHealthChecks("/health").AllowAnonymous();

      await app.RunAsync();
  }
  catch (Exception ex)
  {
      Log.Fatal(ex, "Application terminated unexpectedly");
      return 1;
  }
  finally
  {
      await Log.CloseAndFlushAsync();
  }

  return 0;

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1118:Utility classes should not have public constructors", Justification = "For integration testing")]
  public partial class Program { }
  ```

- [ ] **Step 3: Build to verify no errors**

  ```bash
  dotnet build AppTrack.Api/AppTrack.Api.csproj --configuration Release
  ```

  Expected: `Build succeeded. 0 Error(s).`

- [ ] **Step 4: Run the API locally and verify console output shows structured logs**

  ```bash
  dotnet run --project AppTrack.Api/AppTrack.Api.csproj
  ```

  Expected: Serilog-formatted log lines appear in the console, e.g.:
  ```
  [14:23:01 INF] Now listening on: https://localhost:7001
  [14:23:01 INF] Application started.
  ```

  Make a request to `https://localhost:7001/health` and verify a line like:
  ```
  [14:23:05 INF] HTTP GET /health responded 200 in 12.3ms
  ```

  Verify a `logs/apptrack-YYYYMMDD.log` file is created in the project directory.

- [ ] **Step 5: Commit**

  ```bash
  git add AppTrack.Api/Program.cs
  git commit -m "feat: wire Serilog with bootstrap logger, enrichers, and environment sinks"
  ```

---

## Chunk 4: appsettings Configuration

### Task 5: Replace Logging sections with Serilog sections

**Files:**
- Modify: `AppTrack.Api/appsettings.json`
- Modify: `AppTrack.Api/appsettings.Development.json`
- Modify: `AppTrack.Api/appsettings.Production.json`

- [ ] **Step 1: Update appsettings.json — replace the Logging section**

  Replace:
  ```json
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  ```

  With:
  ```json
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  },
  ```

- [ ] **Step 2: Update appsettings.Development.json — replace the Logging section**

  The existing `Logging` section is intentionally removed — it is superseded by the `Serilog` section. Replace the entire file content with:
  ```json
  {
    "Serilog": {
      "MinimumLevel": {
        "Default": "Debug"
      }
    }
  }
  ```

- [ ] **Step 3: Update appsettings.Production.json — add the Serilog section**

  Add the Serilog section (keep existing content):
  ```json
  {
    "Serilog": {
      "MinimumLevel": {
        "Default": "Warning"
      }
    },
    "ConnectionStrings": {
      "AppTrackConnectionString": ""
    },
    "AllowedOrigins": [
      "https://proud-ocean-05a67ad03.1.azurestaticapps.net"
    ]
  }
  ```

- [ ] **Step 4: Build to verify appsettings changes don't break anything**

  ```bash
  dotnet build AppTrack.sln --configuration Release
  ```

  Expected: `Build succeeded. 0 Error(s).`

- [ ] **Step 5: Commit**

  ```bash
  git add AppTrack.Api/appsettings.json AppTrack.Api/appsettings.Development.json AppTrack.Api/appsettings.Production.json
  git commit -m "feat: replace Logging config with Serilog section in appsettings"
  ```

---

## Chunk 5: Integration Test Fix

### Task 6: Suppress Serilog sinks in integration tests

**Files:**
- Modify: `AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeAuthWebApplicationFactory.cs`

`FakeAiTextWebApplicationFactory` and `FakeCvStorageWebApplicationFactory` both call `base.ConfigureWebHost(builder)`, so changing only the base class is sufficient.

- [ ] **Step 1: Add UseEnvironment("Test") to FakeAuthWebApplicationFactory.ConfigureWebHost**

  Open `AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeAuthWebApplicationFactory.cs`.

  In `ConfigureWebHost`, add `builder.UseEnvironment("Test")` as the **first line**:

  ```csharp
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
      builder.UseEnvironment("Test");    // ← add this line
      builder.UseSetting("OpenAiSettings:ApiKey", "test-api-key-integration");

      builder.ConfigureTestServices(services =>
      {
          // Replace SQL with Testcontainer DB
          services.RemoveAll(typeof(DbContextOptions<AppTrackDatabaseContext>));

          services.AddDbContext<AppTrackDatabaseContext>(options =>
              options.UseSqlServer(_dbContainer.GetConnectionString()));

          // Add Fake Authentication
          services.AddAuthentication(options =>
          {
              options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
              options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
          })
          .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
              TestAuthHandler.AuthenticationScheme, _ => { });
      });
  }
  ```

- [ ] **Step 2: Run all API integration tests to verify they still pass**

  (Requires Docker running for Testcontainers)

  ```bash
  dotnet test AppTrack.Api.IntegrationTests/AppTrack.Api.IntegrationTests.csproj --configuration Release
  ```

  Expected: All tests pass. No file sink output is created during the test run.

- [ ] **Step 3: Run unit tests to verify nothing is broken**

  ```bash
  dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
  ```

  Expected: All tests pass.

- [ ] **Step 4: Commit**

  ```bash
  git add AppTrack.Api.IntegrationTests/WebApplicationFactory/FakeAuthWebApplicationFactory.cs
  git commit -m "test: suppress Serilog sinks in integration test environment"
  ```

---

## Final Verification

- [ ] **Full solution build**

  ```bash
  dotnet build AppTrack.sln --configuration Release
  ```

  Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **All unit tests pass**

  ```bash
  dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
  ```

- [ ] **All integration tests pass** (requires Docker)

  ```bash
  dotnet test AppTrack.Api.IntegrationTests/AppTrack.Api.IntegrationTests.csproj --configuration Release
  ```

- [ ] **Smoke test: verify log file is created in dev**

  Start the API in Development mode, make any authenticated request, confirm:
  - Console shows Serilog-formatted output
  - A `logs/apptrack-YYYYMMDD.log` file is created with JSON content
  - The JSON log entries contain `CorrelationId` and `Environment` properties
  - No `Email`, `Password`, or `Token` values appear in log output

- [ ] **Smoke test: verify PII masking**

  Using Swagger UI or a REST client, make a login attempt with a body containing a `Password` field. Confirm the password does not appear in the console log or log file.
