# Validation Architecture

## Overview

AppTrack uses a **shared validation architecture** built on FluentValidation. Validation rules are defined once in abstract base validators in the `AppTrack.Shared.Validation` project and reused across both the backend (Application, Identity) and frontend (WPF).

```
AppTrack.Shared.Validation
        ├── Interfaces          ← define validatable properties
        └── Validators          ← abstract base validators with FluentValidation rules

AppTrack.Application            ← concrete Command/Query validators (+ DB checks)
AppTrack.Identity               ← concrete validators for auth requests
AppTrack.Frontend.Models        ← concrete validators for WPF models
```

---

## Project Dependencies

```
AppTrack.Shared.Validation
    ↑                   ↑
AppTrack.Application    AppTrack.Frontend.Models
AppTrack.Identity
```

Both backend and frontend projects reference `AppTrack.Shared.Validation`. The dependency is one-directional — `AppTrack.Shared.Validation` has no knowledge of any other project.

---

## Layer 1: Interfaces (`AppTrack.Shared.Validation/Interfaces/`)

Interfaces define only the properties to be validated. They represent the shared contract between backend commands/DTOs and frontend models.

| Interface | Properties | Inherits from |
|---|---|---|
| `IJobApplicationValidatable` | Name, Position, URL, JobDescription, Location, ContactPerson, DurationInMonths | — |
| `IJobApplicationDefaultsValidatable` | Name, Position, Location | — |
| `IAiSettingsValidatable` | ApiKey, PromptParameter | — |
| `IPromptParameterValidatable` | Key, Value | — |
| `IUserCredentialsValidatable` | UserName, Password | — |
| `IRegistrationValidatable` | ConfirmPassword | `IUserCredentialsValidatable` |

### Interface Inheritance

```
IUserCredentialsValidatable
        └── IRegistrationValidatable  (+ConfirmPassword)

IAiSettingsValidatable
        └── uses IPromptParameterValidatable (as collection)
```

---

## Layer 2: Base Validators (`AppTrack.Shared.Validation/Validators/`)

Abstract generic classes that define FluentValidation rules for each interface. Contain **no** database-dependent checks.

### `JobApplicationBaseValidator<T>` where T : IJobApplicationValidatable

| Property | Rules |
|---|---|
| Name | NotNull, NotEmpty, MaxLength(200) |
| Position | NotNull, NotEmpty, MaxLength(200) |
| URL | NotNull, NotEmpty, MaxLength(1000), must be a valid URL |
| JobDescription | NotNull, NotEmpty |
| Location | NotNull, NotEmpty, MaxLength(200) |
| ContactPerson | NotNull, NotEmpty, MaxLength(200) |
| DurationInMonths | MaxLength(10), must be a number if set, range 1–120 |

### `JobApplicationDefaultsBaseValidator<T>` where T : IJobApplicationDefaultsValidatable

| Property | Rules |
|---|---|
| Name | MaxLength(200) |
| Position | MaxLength(200) |
| Location | MaxLength(200) |

### `AiSettingsBaseValidator<T>` where T : IAiSettingsValidatable

| Property | Rules |
|---|---|
| ApiKey | MaxLength(200) |
| PromptParameter (collection) | Delegates to `PromptParameterBaseValidator` per element |
| PromptParameter (overall) | All keys must be unique (case-insensitive) |

### `PromptParameterBaseValidator<T>` where T : IPromptParameterValidatable

| Property | Rules |
|---|---|
| Key | NotEmpty, MaxLength(50) |
| Value | NotEmpty, MaxLength(500) |

### `UserCredentialsBaseValidator<T>` where T : IUserCredentialsValidatable

| Property | Rules |
|---|---|
| UserName | NotEmpty, MinLength(3), MaxLength(20), only [a-zA-Z0-9-_] |
| Password | NotEmpty, MinLength(6), MaxLength(100), uppercase letter, lowercase letter, digit, special character (!?*.@$#&%+-=_()) |

### `RegistrationBaseValidator<T>` where T : IRegistrationValidatable

Inherits from `UserCredentialsBaseValidator<T>` and adds:

| Property | Rules |
|---|---|
| ConfirmPassword | NotEmpty, must equal Password |

### Base Validator Inheritance Hierarchy

```
AbstractValidator<T>  (FluentValidation)
    ├── JobApplicationBaseValidator<T>
    ├── JobApplicationDefaultsBaseValidator<T>
    ├── AiSettingsBaseValidator<T>
    ├── PromptParameterBaseValidator<T>
    └── UserCredentialsBaseValidator<T>
            └── RegistrationBaseValidator<T>
```

---

## Layer 3a: Backend Validators (`AppTrack.Application`, `AppTrack.Identity`)

Concrete validators that inherit from the base validators and add database-dependent rules.

### JobApplication

```
JobApplicationBaseValidator<T>
    ├── CreateJobApplicationCommandValidator
    │       + UserId (NotNull, NotEmpty)
    │       + Status (NotNull)
    │       + StartDate (NotNull, NotEmpty)
    │
    └── UpdateJobApplicationCommandValidator
            + UserId (NotNull, NotEmpty)
            + Status (NotNull)
            + StartDate (NotNull, NotEmpty)
            + DB check: job application must exist and belong to the user
```

### JobApplicationDefaults

```
JobApplicationDefaultsBaseValidator<T>
    └── UpdateJobApplicationDefaultsCommandValidator
            + Id (GreaterThan(0), NotEmpty)
            + UserId (NotEmpty, only [a-zA-Z0-9-])
            + DB check: defaults must exist and belong to the user
```

### AiSettings

```
AiSettingsBaseValidator<T>
    └── UpdateAiSettingsCommandValidator
            + Id (GreaterThan(0), NotEmpty)
            + UserId (NotEmpty, only [a-zA-Z0-9-])
            + DB check: AI settings must exist and belong to the user
```

### PromptParameter

```
PromptParameterBaseValidator<T>
    └── PromptParameterDtoValidator       (used internally by AiSettingsBaseValidator)
```

### Authentication (`AppTrack.Identity`)

```
UserCredentialsBaseValidator<T>
    ├── AuthRequestValidator              (login)
    └── RegistrationRequestValidator      (register — inherits UserCredentials only, no ConfirmPassword)
```

> **Note:** `RegistrationRequestValidator` inherits from `UserCredentialsBaseValidator`, not `RegistrationBaseValidator`. The `ConfirmPassword` field does not exist in the API request.

---

## Layer 3b: Frontend Validators (`AppTrack.Frontend.Models/Validators/`)

Concrete validators for WPF models. Mirror the backend validators but without DB checks and with frontend-specific differences.

```
JobApplicationBaseValidator<T>
    └── JobApplicationModelValidator
            + Status (NotNull)
            + StartDate (NotEqual(DateOnly.MinValue))   ← DateOnly instead of DateTime

JobApplicationDefaultsBaseValidator<T>
    └── JobApplicationDefaultsModelValidator

AiSettingsBaseValidator<T>
    └── AiSettingsModelValidator

PromptParameterBaseValidator<T>
    └── PromptParameterModelValidator
            + Key: duplicate check within ParentCollection (via TempId)

UserCredentialsBaseValidator<T>
    └── LoginModelValidator

RegistrationBaseValidator<T>
    └── RegistrationModelValidator
```

### StartDate Special Case

`StartDate` is **not** included in `IJobApplicationValidatable` because the type differs between layers:

| | Type | Validation |
|---|---|---|
| Backend (Command) | `DateTime` | `NotEmpty()` |
| Frontend (Model) | `DateOnly` | `NotEqual(DateOnly.MinValue)` |

---

## Frontend Validation Infrastructure

The WPF frontend uses its own `IModelValidator<T>` abstraction that wraps FluentValidation.

```
IModelValidator<T>
    └── ModelValidator<T>
            - holds IValidator<T> (constructor injection)
            - Validate(T instance) → bool
            - Errors: IReadOnlyDictionary<string, List<string>>
            - ResetErrors(string propertyName)
```

`ModelValidator<T>` calls `IValidator<T>.Validate()` and groups errors by property name into a dictionary — consistent with the backend error format (`CustomProblemDetails.Errors`).

### DI Registration (`App.xaml.cs`)

```csharp
// Concrete IValidator<T> registrations
services.AddTransient<IValidator<JobApplicationModel>,         JobApplicationModelValidator>();
services.AddTransient<IValidator<JobApplicationDefaultsModel>, JobApplicationDefaultsModelValidator>();
services.AddTransient<IValidator<AiSettingsModel>,             AiSettingsModelValidator>();
services.AddTransient<IValidator<PromptParameterModel>,        PromptParameterModelValidator>();
services.AddTransient<IValidator<LoginModel>,                  LoginModelValidator>();
services.AddTransient<IValidator<RegistrationModel>,           RegistrationModelValidator>();

// Generic wrapper
services.AddTransient(typeof(IModelValidator<>), typeof(ModelValidator<>));
```

---

## Backend Validation Flow

There is **no MediatR pipeline behaviour** for validation. Each handler receives its validator via constructor injection and calls `ValidateAsync` manually.

```
HTTP Request
    → Controller
    → IMediator.Send(command)
    → CommandHandler
        → validator.ValidateAsync(request)
        → errors? → throw BadRequestException(validationResult)
    → ExceptionMiddleware
        → HTTP 400 with CustomProblemDetails { Errors: { "PropertyName": ["message"] } }
```

### Identity Special Case

`AuthService` instantiates the validator directly without DI:

```csharp
var validator = new AuthRequestValidator();
var validationResult = await validator.ValidateAsync(request);
```

All other handlers receive their validator via constructor injection.

---

## Full Class Relationships

```
AppTrack.Shared.Validation
┌─────────────────────────────────────────────────┐
│  Interfaces                                     │
│  IJobApplicationValidatable                     │
│  IJobApplicationDefaultsValidatable             │
│  IAiSettingsValidatable                         │
│  IPromptParameterValidatable                    │
│  IUserCredentialsValidatable                    │
│  IRegistrationValidatable ──→ IUserCredentials  │
│                                                 │
│  Base Validators                                │
│  JobApplicationBaseValidator<T>                 │
│  JobApplicationDefaultsBaseValidator<T>         │
│  AiSettingsBaseValidator<T>                     │
│  PromptParameterBaseValidator<T>                │
│  UserCredentialsBaseValidator<T>                │
│  RegistrationBaseValidator<T> ──→ UserCreds     │
└─────────────────────────────────────────────────┘
         ↑                            ↑
AppTrack.Application          AppTrack.Frontend.Models
AppTrack.Identity
┌──────────────────────┐      ┌──────────────────────────┐
│ Commands/DTOs        │      │ Models                   │
│ implement            │      │ implement                │
│ interfaces           │      │ interfaces               │
│                      │      │                          │
│ Validators           │      │ Validators               │
│ extend base          │      │ extend base              │
│ validators           │      │ validators               │
│ + DB checks          │      │ no DB checks             │
└──────────────────────┘      └──────────────────────────┘
                                        ↓
                              IModelValidator<T>
                              ModelValidator<T>
                              (wraps IValidator<T>)
```
