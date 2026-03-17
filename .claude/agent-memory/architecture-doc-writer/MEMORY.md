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

## Docs Already Written
- `docs/error-handling-architecture.md` — full error handling, all layers, DE language
- `docs/mapping-architecture.md` — AutoMapper profiles
- `docs/validation-architecture.md` — shared validation, FluentValidation
