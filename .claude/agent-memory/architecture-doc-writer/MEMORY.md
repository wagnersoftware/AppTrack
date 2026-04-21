# Architecture Doc Writer Memory

## Naming Conventions
- Persistence project: `AppTrack.Persistance` (NOT `Persistence` — typo is intentional/historical)
- WPF project namespace: `AppTrack.WpfUi` (folder is `AppTrack.Wpf`)
- MessageBoxService namespace: `AppTrack.WpfUi.MessageBoxService`

## Key File Paths
- docs/: `C:\Users\danie\source\repos\AppTrack\docs\`
- Exception types: `AppTrack.Application/Exceptions/`
- ExceptionMiddleware: `AppTrack.Api/Middleware/ExceptionMiddleware.cs`
- Response<T>: `ApiService/Base/Response.cs`
- BaseHttpService: `ApiService/Base/BaseHttpService.cs`
- ApiErrorHelper: `ApiService/Helper/ApiErrorHelper.cs`
- ErrorHandlingService (Blazor): `AppTrack.BlazorUi/Services/ErrorHandlingService.cs`
- MessageBoxService (WPF): `AppTrack.Wpf/MessageBoxService/MessageBoxService.cs`
- CustomProblemDetails: `AppTrack.Api/Models/CustomProblemDetails.cs`

## Error Handling Architecture (verified Mar 2026)
- 4 exception types: BadRequestException (400), NotFoundException (404), ConflictException (409, unused), ExternalServiceException (UpstreamStatusCode ?? 502)
- ExceptionMiddleware: LogWarning for client errors (400/404/409), LogError for ExternalServiceException and unhandled
- Default case (unhandled) returns StackTrace in Detail field — only suitable for Development
- Response<T>.DisplayMessage = ErrorDetails ?? ErrorMessage — always use DisplayMessage in UI
- TryExecuteAsync catches: AccessTokenNotAvailableException → Redirect, OperationCanceledException, ApiException → ConvertApiException, Exception
- ApiErrorHelper formats: "Status: 400\nMessage: ...\nFieldName: error message"
- Blazor pattern: if (!ErrorHandlingService.HandleResponse(response)) return;
- WPF pattern: if (apiResponse.Success == false) { ErrorMessage = apiResponse.DisplayMessage; return; }

## CQRS Handler Validation Pattern
- Handlers instantiate validator with `new`, call ValidateAsync, throw BadRequestException(message, validationResult) on errors
- No pipeline behavior for validation — validation is explicit in each handler

## Logging Architecture (verified Apr 2026)
- Serilog wired as backend of Microsoft.ILogger<T> via builder.Host.UseSerilog()
- IAppLogger<T> (Application) + LoggingAdapter<T> (Infrastructure) are unchanged
- Bootstrap logger: Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger() before host build
- Enrichers: CorrelationIdEnricher (W3C TraceId, "none" if null), WithProperty("Environment") globally, UserId via EnrichDiagnosticContext (sub claim, HTTP only)
- Environment is set GLOBALLY via .Enrich.WithProperty — NOT via EnrichDiagnosticContext (do not repeat in EnrichDiagnosticContext)
- Sinks: Console (non-Test), File rolling daily JSON (Development only), ApplicationInsights (Production + conn string set)
- Test env: builder.UseEnvironment("Test") in FakeAuthWebApplicationFactory — all sink predicates false
- PiiDestructuringPolicy: masks Password/ApiKey/Token/Email/Name/GivenName/FamilyName → [REDACTED]; getter throws → [ERROR_READING_PROPERTY]; indexers filtered before reflection
- Middleware order: UseSerilogRequestLogging (outermost) → ExceptionMiddleware → rest
- NuGet (inline versions in AppTrack.Api.csproj): Serilog.AspNetCore 8.0.3, Serilog.Sinks.File 6.0.0, Serilog.Sinks.ApplicationInsights 5.0.1
- Key files: AppTrack.Api/Logging/CorrelationIdEnricher.cs, AppTrack.Api/Logging/PiiDestructuringPolicy.cs

## Docs Already Written
- `docs/error-handling-architecture.md` — full error handling, all layers, EN language
- `docs/mapping-architecture.md` — AutoMapper profiles
- `docs/validation-architecture.md` — shared validation, FluentValidation
- `docs/logging-architecture.md` — Serilog structured logging, enrichment, PII policy, sinks
