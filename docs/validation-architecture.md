# Validation Architecture

## Overview

AppTrack uses a **shared validation architecture** built on FluentValidation. The goal is to define validation rules once and reuse them across both the backend (ASP.NET Core API) and any frontend, avoiding duplication while keeping DB-dependent logic strictly on the backend.

The architecture consists of three layers:

```
AppTrack.Shared.Validation      ← shared interfaces + abstract base validators
        ↑                   ↑
AppTrack.Application            ← concrete backend validators (+ DB checks)
AppTrack.Identity               AppTrack.Frontend.Models  ← concrete frontend validators
```

`AppTrack.Shared.Validation` has no dependencies on any other project in the solution.

---

## Layer 1 — Shared Interface

For each domain concept that requires validation, a dedicated interface is defined in `AppTrack.Shared.Validation/Interfaces/`. The interface declares only the properties to be validated — nothing more.

Both the backend command/DTO and the frontend model implement this interface, establishing a shared contract.

```csharp
// AppTrack.Shared.Validation/Interfaces/IJobApplicationValidatable.cs
public interface IJobApplicationValidatable
{
    string Name { get; }
    string Position { get; }
    string URL { get; }
    // ...
}

// Backend: AppTrack.Application
public class CreateJobApplicationCommand : IJobApplicationValidatable { ... }

// Frontend
public class JobApplicationModel : IJobApplicationValidatable { ... }
```

If a property type differs between backend and frontend (e.g. `DateTime` vs `DateOnly`), it is excluded from the shared interface and validated individually in each layer.

---

## Layer 2 — Abstract Base Validator

For each interface, a corresponding abstract generic base validator is defined in `AppTrack.Shared.Validation/Validators/`. It contains all rules that apply to both backend and frontend.

```csharp
// AppTrack.Shared.Validation/Validators/JobApplicationBaseValidator.cs
public abstract class JobApplicationBaseValidator<T> : AbstractValidator<T>
    where T : IJobApplicationValidatable
{
    protected JobApplicationBaseValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.URL).Must(BeAValidUrl).WithMessage("...");
        // ...
    }
}
```

Base validators must not contain any database access or infrastructure dependencies.

---

## Layer 3a — Concrete Backend Validator

Backend validators inherit from the base validator and add backend-specific rules such as DB existence checks or ownership verification. They are injected into their corresponding command/query handler via constructor injection.

```csharp
// AppTrack.Application/.../CreateJobApplicationCommandValidator.cs
public class CreateJobApplicationCommandValidator
    : JobApplicationBaseValidator<CreateJobApplicationCommand>
{
    public CreateJobApplicationCommandValidator()
    {
        // backend-only fields not covered by the shared interface
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
    }
}

// AppTrack.Application/.../UpdateJobApplicationCommandValidator.cs
public class UpdateJobApplicationCommandValidator
    : JobApplicationBaseValidator<UpdateJobApplicationCommand>
{
    public UpdateJobApplicationCommandValidator(IJobApplicationRepository repository)
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();

        // DB check — backend only
        RuleFor(x => x).MustAsync(async (cmd, _) =>
        {
            var entity = await repository.GetByIdAsync(cmd.Id);
            return entity != null && entity.UserId == cmd.UserId;
        }).WithMessage("Job application not found for this user.");
    }
}
```

### Validation Flow

There is no MediatR pipeline behaviour for validation. Each handler calls `ValidateAsync` directly and throws a `BadRequestException` on failure. The `ExceptionMiddleware` converts this into an HTTP 400 response.

```
HTTP Request
    → Controller
    → IMediator.Send(command)
    → CommandHandler
        → validator.ValidateAsync(request)
        → errors? → throw BadRequestException(validationResult)
    → ExceptionMiddleware
        → HTTP 400 { Errors: { "PropertyName": ["message"] } }
```

---

## Layer 3b — Concrete Frontend Validator

Frontend validators inherit from the same base validator and add frontend-specific rules (e.g. collection-level duplicate checks within the UI model). They contain no DB access.

```csharp
// Frontend/Validators/JobApplicationModelValidator.cs
public class JobApplicationModelValidator
    : JobApplicationBaseValidator<JobApplicationModel>
{
    public JobApplicationModelValidator()
    {
        // frontend-only: DateOnly instead of DateTime
        RuleFor(x => x.StartDate).NotEqual(DateOnly.MinValue);
        RuleFor(x => x.Status).NotNull();
    }
}
```

Frontend validators are registered in the frontend's DI container and consumed through the `IModelValidator<T>` abstraction, which wraps `IValidator<T>` and exposes a property-keyed error dictionary consistent with the backend error format.

```csharp
// DI registration
services.AddTransient<IValidator<JobApplicationModel>, JobApplicationModelValidator>();
services.AddTransient(typeof(IModelValidator<>), typeof(ModelValidator<>));
```

---

## Adding a New Validated Concept

To add validation for a new domain concept, follow these steps:

1. **Define an interface** in `AppTrack.Shared.Validation/Interfaces/` with the properties to validate.
2. **Create a base validator** in `AppTrack.Shared.Validation/Validators/` inheriting from `AbstractValidator<T>` with all shared rules.
3. **Implement the interface** on the backend command/DTO and create a concrete validator inheriting from the base validator. Add any DB checks here.
4. **Implement the interface** on the frontend model and create a concrete validator inheriting from the base validator. Add any UI-specific rules here.
5. **Register the frontend validator** in the frontend's DI container.
