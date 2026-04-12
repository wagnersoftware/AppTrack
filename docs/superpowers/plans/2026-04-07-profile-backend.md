# Profile Backend Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the full backend stack for the freelancer profile feature — Domain entity, Application CQRS, Persistence, API controller, NSwag client regen, ApiService, frontend model/validator update, Blazor form validation wiring and Save button integration.

**Architecture:** Clean Architecture (Domain → Application → Persistence → API). The Blazor UI form already exists; this plan wires it to the backend via a new `PUT /api/profile` upsert endpoint and a `GET /api/profile` read endpoint, both scoped to the JWT user.

**Tech Stack:** .NET 10, EF Core 10 (SQL Server), FluentValidation 12, MudBlazor 9.1, NSwag 14, xUnit + Moq + Shouldly, Blazor WASM

**Spec:** `docs/superpowers/specs/2026-04-07-profile-backend-design.md`

> **Working directory:** All commands assume `C:\Users\danie\source\repos\AppTrack` as root.

> **TDD note:** Chunks 1–2 build foundation types. Chunk 3 writes tests that fail at runtime (handlers return empty stubs). Chunk 3 also implements handlers to make tests pass. Persistence/API/Frontend chunks use build verification only.

---

## Chunk 1: Shared Validation + Domain

### Task 1: `IFreelancerProfileValidatable` interface

**Files:**
- Create: `AppTrack.Shared.Validation/Interfaces/IFreelancerProfileValidatable.cs`

- [ ] **Step 1: Create the file**

```csharp
namespace AppTrack.Shared.Validation.Interfaces;

public interface IFreelancerProfileValidatable
{
    string FirstName { get; }
    string LastName { get; }
    decimal? HourlyRate { get; }
    decimal? DailyRate { get; }
    string? Skills { get; }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Shared.Validation/Interfaces/IFreelancerProfileValidatable.cs
git commit -m "feat: add IFreelancerProfileValidatable interface to shared validation"
```

---

### Task 2: `FreelancerProfileBaseValidator<T>`

**Files:**
- Create: `AppTrack.Shared.Validation/Validators/FreelancerProfileBaseValidator.cs`

- [ ] **Step 1: Create the file**

```csharp
using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class FreelancerProfileBaseValidator<T> : AbstractValidator<T>
    where T : IFreelancerProfileValidatable
{
    protected FreelancerProfileBaseValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

        RuleFor(x => x.HourlyRate)
            .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.")
            .When(x => x.HourlyRate.HasValue);

        RuleFor(x => x.DailyRate)
            .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.")
            .When(x => x.DailyRate.HasValue);

        RuleFor(x => x.Skills)
            .MaximumLength(1000).WithMessage("{PropertyName} must not exceed 1000 characters.")
            .When(x => x.Skills != null);
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Shared.Validation/Validators/FreelancerProfileBaseValidator.cs
git commit -m "feat: add FreelancerProfileBaseValidator to shared validation"
```

---

### Task 3: Domain enums

**Files:**
- Create: `AppTrack.Domain/Enums/RemotePreference.cs`
- Create: `AppTrack.Domain/Enums/ApplicationLanguage.cs`

These are **new Domain-layer types**, separate from the identically-named enums in `AppTrack.Frontend.Models`. Integer values must match so cast-based conversion works.

- [ ] **Step 1: Create `RemotePreference.cs`**

```csharp
namespace AppTrack.Domain.Enums;

public enum RemotePreference
{
    Remote = 0,
    Hybrid = 1,
    OnSite = 2
}
```

- [ ] **Step 2: Create `ApplicationLanguage.cs`**

```csharp
namespace AppTrack.Domain.Enums;

public enum ApplicationLanguage
{
    German = 0,
    English = 1
}
```

- [ ] **Step 3: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Domain/Enums/RemotePreference.cs AppTrack.Domain/Enums/ApplicationLanguage.cs
git commit -m "feat: add RemotePreference and ApplicationLanguage enums to Domain"
```

---

### Task 4: `FreelancerProfile` entity

**Files:**
- Create: `AppTrack.Domain/FreelancerProfile.cs`

- [ ] **Step 1: Create the file**

```csharp
using AppTrack.Domain.Common;
using AppTrack.Domain.Enums;

namespace AppTrack.Domain;

public class FreelancerProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal? HourlyRate { get; set; }
    public decimal? DailyRate { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public RemotePreference? WorkMode { get; set; }
    public string? Skills { get; set; }
    public ApplicationLanguage? Language { get; set; }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Domain/FreelancerProfile.cs
git commit -m "feat: add FreelancerProfile entity to Domain"
```

---

## Chunk 2: Application Layer

### Task 5: Repository contract + DTO

**Files:**
- Create: `AppTrack.Application/Contracts/Persistance/IFreelancerProfileRepository.cs`
- Create: `AppTrack.Application/Features/FreelancerProfile/Dto/FreelancerProfileDto.cs`

- [ ] **Step 1: Create `IFreelancerProfileRepository.cs`**

```csharp
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.Persistance;

public interface IFreelancerProfileRepository : IGenericRepository<FreelancerProfile>
{
    Task<FreelancerProfile?> GetByUserIdAsync(string userId);
    Task UpsertAsync(FreelancerProfile profile);
}
```

- [ ] **Step 2: Create `FreelancerProfileDto.cs`**

```csharp
using AppTrack.Domain.Enums;

namespace AppTrack.Application.Features.FreelancerProfile.Dto;

public class FreelancerProfileDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal? HourlyRate { get; set; }
    public decimal? DailyRate { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public RemotePreference? WorkMode { get; set; }
    public string? Skills { get; set; }
    public ApplicationLanguage? Language { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Application/Contracts/Persistance/IFreelancerProfileRepository.cs \
        AppTrack.Application/Features/FreelancerProfile/Dto/FreelancerProfileDto.cs
git commit -m "feat: add IFreelancerProfileRepository and FreelancerProfileDto"
```

---

### Task 6: `UpsertFreelancerProfileCommand` + validator

**Files:**
- Create: `AppTrack.Application/Features/FreelancerProfile/Commands/UpsertFreelancerProfile/UpsertFreelancerProfileCommand.cs`
- Create: `AppTrack.Application/Features/FreelancerProfile/Commands/UpsertFreelancerProfile/UpsertFreelancerProfileCommandValidator.cs`

- [ ] **Step 1: Create `UpsertFreelancerProfileCommand.cs`**

```csharp
using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Domain.Enums;
using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;

public class UpsertFreelancerProfileCommand : IRequest<FreelancerProfileDto>, IUserScopedRequest, IFreelancerProfileValidatable
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal? HourlyRate { get; set; }
    public decimal? DailyRate { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public RemotePreference? WorkMode { get; set; }
    public string? Skills { get; set; }
    public ApplicationLanguage? Language { get; set; }
}
```

- [ ] **Step 2: Create `UpsertFreelancerProfileCommandValidator.cs`**

```csharp
using AppTrack.Shared.Validation.Validators;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;

public class UpsertFreelancerProfileCommandValidator : FreelancerProfileBaseValidator<UpsertFreelancerProfileCommand>
{
    public UpsertFreelancerProfileCommandValidator()
    {
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Application/Features/FreelancerProfile/Commands/UpsertFreelancerProfile/
git commit -m "feat: add UpsertFreelancerProfileCommand and validator"
```

---

### Task 7: `GetFreelancerProfileQuery` + validator

**Files:**
- Create: `AppTrack.Application/Features/FreelancerProfile/Queries/GetFreelancerProfile/GetFreelancerProfileQuery.cs`
- Create: `AppTrack.Application/Features/FreelancerProfile/Queries/GetFreelancerProfile/GetFreelancerProfileQueryValidator.cs`

- [ ] **Step 1: Create `GetFreelancerProfileQuery.cs`**

```csharp
using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.FreelancerProfile.Dto;

namespace AppTrack.Application.Features.FreelancerProfile.Queries.GetFreelancerProfile;

public class GetFreelancerProfileQuery : IRequest<FreelancerProfileDto>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Create `GetFreelancerProfileQueryValidator.cs`**

```csharp
using FluentValidation;

namespace AppTrack.Application.Features.FreelancerProfile.Queries.GetFreelancerProfile;

public class GetFreelancerProfileQueryValidator : AbstractValidator<GetFreelancerProfileQuery>
{
    public GetFreelancerProfileQueryValidator()
    {
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Application/Features/FreelancerProfile/Queries/GetFreelancerProfile/
git commit -m "feat: add GetFreelancerProfileQuery and validator"
```

---

### Task 8: Application Mappings

**Files:**
- Create: `AppTrack.Application/Mappings/FreelancerProfileMappings.cs`

- [ ] **Step 1: Create the file**

```csharp
using AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;
using AppTrack.Application.Features.FreelancerProfile.Dto;

namespace AppTrack.Application.Mappings;

internal static class FreelancerProfileMappings
{
    internal static Domain.FreelancerProfile ToNewDomain(this UpsertFreelancerProfileCommand command) => new()
    {
        UserId = command.UserId,
        FirstName = command.FirstName,
        LastName = command.LastName,
        HourlyRate = command.HourlyRate,
        DailyRate = command.DailyRate,
        AvailableFrom = command.AvailableFrom,
        WorkMode = command.WorkMode,
        Skills = command.Skills,
        Language = command.Language,
    };

    internal static void ApplyTo(this UpsertFreelancerProfileCommand command, Domain.FreelancerProfile entity)
    {
        entity.UserId = command.UserId;
        entity.FirstName = command.FirstName;
        entity.LastName = command.LastName;
        entity.HourlyRate = command.HourlyRate;
        entity.DailyRate = command.DailyRate;
        entity.AvailableFrom = command.AvailableFrom;
        entity.WorkMode = command.WorkMode;
        entity.Skills = command.Skills;
        entity.Language = command.Language;
    }

    internal static FreelancerProfileDto ToDto(this Domain.FreelancerProfile entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        HourlyRate = entity.HourlyRate,
        DailyRate = entity.DailyRate,
        AvailableFrom = entity.AvailableFrom,
        WorkMode = entity.WorkMode,
        Skills = entity.Skills,
        Language = entity.Language,
        CreationDate = entity.CreationDate ?? default,
        ModifiedDate = entity.ModifiedDate ?? default,
    };
}
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Application/Mappings/FreelancerProfileMappings.cs
git commit -m "feat: add FreelancerProfile application mappings"
```

---

### Task 9: Command handler + Query handler

**Files:**
- Create: `AppTrack.Application/Features/FreelancerProfile/Commands/UpsertFreelancerProfile/UpsertFreelancerProfileCommandHandler.cs`
- Create: `AppTrack.Application/Features/FreelancerProfile/Queries/GetFreelancerProfile/GetFreelancerProfileQueryHandler.cs`

- [ ] **Step 1: Create `UpsertFreelancerProfileCommandHandler.cs`**

```csharp
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.Mappings;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;

public class UpsertFreelancerProfileCommandHandler : IRequestHandler<UpsertFreelancerProfileCommand, FreelancerProfileDto>
{
    private readonly IFreelancerProfileRepository _repository;

    public UpsertFreelancerProfileCommandHandler(IFreelancerProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<FreelancerProfileDto> Handle(UpsertFreelancerProfileCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpsertFreelancerProfileCommandValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException("Invalid freelancer profile request", validationResult);
        }

        var existing = await _repository.GetByUserIdAsync(request.UserId);

        if (existing == null)
        {
            var newProfile = request.ToNewDomain();
            await _repository.UpsertAsync(newProfile);
            return newProfile.ToDto();
        }

        request.ApplyTo(existing);
        await _repository.UpsertAsync(existing);
        return existing.ToDto();
    }
}
```

- [ ] **Step 2: Create `GetFreelancerProfileQueryHandler.cs`**

```csharp
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.Mappings;
using AppTrack.Domain;

namespace AppTrack.Application.Features.FreelancerProfile.Queries.GetFreelancerProfile;

public class GetFreelancerProfileQueryHandler : IRequestHandler<GetFreelancerProfileQuery, FreelancerProfileDto>
{
    private readonly IFreelancerProfileRepository _repository;

    public GetFreelancerProfileQueryHandler(IFreelancerProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<FreelancerProfileDto> Handle(GetFreelancerProfileQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetFreelancerProfileQueryValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException("Invalid get profile request", validationResult);
        }

        var profile = await _repository.GetByUserIdAsync(request.UserId);

        if (profile == null)
        {
            throw new NotFoundException(nameof(FreelancerProfile), request.UserId);
        }

        return profile.ToDto();
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Application/Features/FreelancerProfile/Commands/UpsertFreelancerProfile/UpsertFreelancerProfileCommandHandler.cs \
        AppTrack.Application/Features/FreelancerProfile/Queries/GetFreelancerProfile/GetFreelancerProfileQueryHandler.cs
git commit -m "feat: add UpsertFreelancerProfile and GetFreelancerProfile handlers"
```

---

## Chunk 3: Unit Tests

### Task 10: Mock repository

**Files:**
- Create: `AppTrack.Application.UnitTests/Mocks/MockFreelancerProfileRepository.cs`

- [ ] **Step 1: Create the file**

```csharp
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;

namespace AppTrack.Application.UnitTests.Mocks;

public static class MockFreelancerProfileRepository
{
    public const string ExistingUserId = "user-1";
    public const int ExistingId = 42;

    public static Mock<IFreelancerProfileRepository> GetMock()
    {
        var mockRepo = new Mock<IFreelancerProfileRepository>();

        var existingProfile = new FreelancerProfile
        {
            Id = ExistingId,
            UserId = ExistingUserId,
            FirstName = "Anna",
            LastName = "Müller",
            HourlyRate = 100m,
            DailyRate = null,
            WorkMode = RemotePreference.Remote,
            Skills = "C#, .NET",
        };

        mockRepo
            .Setup(r => r.GetByUserIdAsync(ExistingUserId))
            .ReturnsAsync(existingProfile);

        mockRepo
            .Setup(r => r.GetByUserIdAsync(It.Is<string>(id => id != ExistingUserId)))
            .ReturnsAsync((FreelancerProfile?)null);

        mockRepo
            .Setup(r => r.UpsertAsync(It.IsAny<FreelancerProfile>()))
            .Returns(Task.CompletedTask);

        return mockRepo;
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Application.UnitTests/Mocks/MockFreelancerProfileRepository.cs
git commit -m "test: add MockFreelancerProfileRepository"
```

---

### Task 11: `UpsertFreelancerProfileCommandValidatorTests`

**Files:**
- Create: `AppTrack.Application.UnitTests/Features/FreelancerProfile/Commands/UpsertFreelancerProfileCommandValidatorTests.cs`

- [ ] **Step 1: Create the test file**

```csharp
using AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Commands;

public class UpsertFreelancerProfileCommandValidatorTests
{
    private readonly UpsertFreelancerProfileCommandValidator _validator = new();

    private static UpsertFreelancerProfileCommand ValidCommand() => new()
    {
        UserId = "user-1",
        FirstName = "Anna",
        LastName = "Müller",
        HourlyRate = null,
        DailyRate = null,
        Skills = null,
    };

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var result = await _validator.TestValidateAsync(ValidCommand());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenFirstNameIsEmpty()
    {
        var command = ValidCommand();
        command.FirstName = string.Empty;
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenLastNameIsEmpty()
    {
        var command = ValidCommand();
        command.LastName = string.Empty;
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenHourlyRateIsZero()
    {
        var command = ValidCommand();
        command.HourlyRate = 0;
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.HourlyRate);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenHourlyRateIsNegative()
    {
        var command = ValidCommand();
        command.HourlyRate = -1;
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.HourlyRate);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenHourlyRateIsNull()
    {
        var command = ValidCommand();
        command.HourlyRate = null;
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.HourlyRate);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenSkillsExceedMaxLength()
    {
        var command = ValidCommand();
        command.Skills = new string('x', 1001);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Skills);
    }
}
```

- [ ] **Step 2: Run the tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release --filter "FullyQualifiedName~UpsertFreelancerProfileCommandValidatorTests"
```

Expected: all 7 tests **PASS** (validator already has all rules from the base).

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Application.UnitTests/Features/FreelancerProfile/Commands/UpsertFreelancerProfileCommandValidatorTests.cs
git commit -m "test: add UpsertFreelancerProfileCommandValidatorTests"
```

---

### Task 12: `UpsertFreelancerProfileCommandHandlerTests`

**Files:**
- Create: `AppTrack.Application.UnitTests/Features/FreelancerProfile/Commands/UpsertFreelancerProfileCommandHandlerTests.cs`

- [ ] **Step 1: Create the test file**

```csharp
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.UnitTests.Mocks;
using AppTrack.Domain;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Commands;

public class UpsertFreelancerProfileCommandHandlerTests
{
    private readonly Mock<IFreelancerProfileRepository> _mockRepo;
    private readonly UpsertFreelancerProfileCommandHandler _handler;

    public UpsertFreelancerProfileCommandHandlerTests()
    {
        _mockRepo = MockFreelancerProfileRepository.GetMock();
        _handler = new UpsertFreelancerProfileCommandHandler(_mockRepo.Object);
    }

    private static UpsertFreelancerProfileCommand ValidCommand(string userId = MockFreelancerProfileRepository.ExistingUserId) => new()
    {
        UserId = userId,
        FirstName = "Anna",
        LastName = "Müller",
    };

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenCommandIsValid()
    {
        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<FreelancerProfileDto>();
        result.FirstName.ShouldBe("Anna");
        result.LastName.ShouldBe("Müller");
    }

    [Fact]
    public async Task Handle_ShouldCreate_WhenNoExistingProfile()
    {
        var result = await _handler.Handle(ValidCommand(userId: "new-user"), CancellationToken.None);

        result.ShouldNotBeNull();
        _mockRepo.Verify(r => r.UpsertAsync(It.Is<FreelancerProfile>(p => p.Id == 0)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdate_WhenProfileExists()
    {
        var command = ValidCommand(userId: MockFreelancerProfileRepository.ExistingUserId);
        command.FirstName = "Updated";

        var result = await _handler.Handle(command, CancellationToken.None);

        result.FirstName.ShouldBe("Updated");
        _mockRepo.Verify(r => r.UpsertAsync(It.Is<FreelancerProfile>(p => p.Id == MockFreelancerProfileRepository.ExistingId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenFirstNameIsEmpty()
    {
        var command = ValidCommand();
        command.FirstName = string.Empty;

        var ex = await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        ex.ValidationErrors.ShouldContainKey("FirstName");
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenLastNameIsEmpty()
    {
        var command = ValidCommand();
        command.LastName = string.Empty;

        var ex = await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        ex.ValidationErrors.ShouldContainKey("LastName");
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenHourlyRateIsNegative()
    {
        var command = ValidCommand();
        command.HourlyRate = -10;

        var ex = await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        ex.ValidationErrors.ShouldContainKey("HourlyRate");
    }

    [Fact]
    public async Task Handle_ShouldNotCallUpsertAsync_WhenValidationFails()
    {
        var command = ValidCommand();
        command.FirstName = string.Empty;

        await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _mockRepo.Verify(r => r.UpsertAsync(It.IsAny<FreelancerProfile>()), Times.Never);
    }
}
```

- [ ] **Step 2: Run the tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release --filter "FullyQualifiedName~UpsertFreelancerProfileCommandHandlerTests"
```

Expected: all 7 tests **PASS**.

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Application.UnitTests/Features/FreelancerProfile/Commands/UpsertFreelancerProfileCommandHandlerTests.cs
git commit -m "test: add UpsertFreelancerProfileCommandHandlerTests"
```

---

### Task 13: `GetFreelancerProfileQueryValidatorTests` + `GetFreelancerProfileQueryHandlerTests`

**Files:**
- Create: `AppTrack.Application.UnitTests/Features/FreelancerProfile/Queries/GetFreelancerProfileQueryValidatorTests.cs`
- Create: `AppTrack.Application.UnitTests/Features/FreelancerProfile/Queries/GetFreelancerProfileQueryHandlerTests.cs`

- [ ] **Step 1: Create `GetFreelancerProfileQueryValidatorTests.cs`**

```csharp
using AppTrack.Application.Features.FreelancerProfile.Queries.GetFreelancerProfile;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Queries;

public class GetFreelancerProfileQueryValidatorTests
{
    private readonly GetFreelancerProfileQueryValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldPass_ForAnyQuery()
    {
        var query = new GetFreelancerProfileQuery { UserId = "user-1" };
        var result = await _validator.TestValidateAsync(query);
        result.IsValid.ShouldBeTrue();
    }
}
```

- [ ] **Step 2: Create `GetFreelancerProfileQueryHandlerTests.cs`**

```csharp
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.Features.FreelancerProfile.Queries.GetFreelancerProfile;
using AppTrack.Application.UnitTests.Mocks;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Queries;

public class GetFreelancerProfileQueryHandlerTests
{
    private readonly Mock<IFreelancerProfileRepository> _mockRepo;
    private readonly GetFreelancerProfileQueryHandler _handler;

    public GetFreelancerProfileQueryHandlerTests()
    {
        _mockRepo = MockFreelancerProfileRepository.GetMock();
        _handler = new GetFreelancerProfileQueryHandler(_mockRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenProfileExists()
    {
        var query = new GetFreelancerProfileQuery { UserId = MockFreelancerProfileRepository.ExistingUserId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<FreelancerProfileDto>();
        result.UserId.ShouldBe(MockFreelancerProfileRepository.ExistingUserId);
        result.FirstName.ShouldBe("Anna");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProfileDoesNotExist()
    {
        var query = new GetFreelancerProfileQuery { UserId = "unknown-user" };

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
    }
}
```

- [ ] **Step 3: Run all FreelancerProfile tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release --filter "FullyQualifiedName~FreelancerProfile"
```

Expected: all tests **PASS** (17 total across the 4 test classes).

- [ ] **Step 4: Commit**

```bash
git add AppTrack.Application.UnitTests/Features/FreelancerProfile/
git commit -m "test: add GetFreelancerProfile query validator and handler tests"
```

---

## Chunk 4: Persistence + API

### Task 14: EF Entity Configuration + Repository

**Files:**
- Create: `AppTrack.Persistance/Configurations/FreelancerProfileConfiguration.cs`
- Create: `AppTrack.Persistance/Repositories/FreelancerProfileRepository.cs`
- Modify: `AppTrack.Persistance/DatabaseContext/AppTrackDatabaseContext.cs`
- Modify: `AppTrack.Persistance/PersistanceServiceRegistration.cs`

- [ ] **Step 1: Create `FreelancerProfileConfiguration.cs`**

```csharp
using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class FreelancerProfileConfiguration : IEntityTypeConfiguration<FreelancerProfile>
{
    public void Configure(EntityTypeBuilder<FreelancerProfile> builder)
    {
        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.HourlyRate)
            .HasPrecision(18, 2);

        builder.Property(x => x.DailyRate)
            .HasPrecision(18, 2);

        builder.Property(x => x.Skills)
            .HasMaxLength(1000);
    }
}
```

- [ ] **Step 2: Create `FreelancerProfileRepository.cs`**

```csharp
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class FreelancerProfileRepository : GenericRepository<FreelancerProfile>, IFreelancerProfileRepository
{
    public FreelancerProfileRepository(AppTrackDatabaseContext dbContext) : base(dbContext)
    {
    }

    public async Task<FreelancerProfile?> GetByUserIdAsync(string userId)
    {
        return await _context.FreelancerProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task UpsertAsync(FreelancerProfile profile)
    {
        if (profile.Id == 0)
        {
            await CreateAsync(profile);
        }
        else
        {
            await UpdateAsync(profile);
        }
    }
}
```

- [ ] **Step 3: Add `DbSet` to `AppTrackDatabaseContext.cs`**

Open `AppTrack.Persistance/DatabaseContext/AppTrackDatabaseContext.cs` and add after the existing `DbSet` declarations:

```csharp
public DbSet<FreelancerProfile> FreelancerProfiles { get; set; }
```

Also add the `using` for the domain:
```csharp
// Already present: using AppTrack.Domain;
```

- [ ] **Step 4: Register repository in `PersistanceServiceRegistration.cs`**

In `AppTrack.Persistance/PersistanceServiceRegistration.cs`, add after the last `services.AddScoped` call:

```csharp
services.AddScoped<IFreelancerProfileRepository, FreelancerProfileRepository>();
```

- [ ] **Step 5: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 6: Commit**

```bash
git add AppTrack.Persistance/Configurations/FreelancerProfileConfiguration.cs \
        AppTrack.Persistance/Repositories/FreelancerProfileRepository.cs \
        AppTrack.Persistance/DatabaseContext/AppTrackDatabaseContext.cs \
        AppTrack.Persistance/PersistanceServiceRegistration.cs
git commit -m "feat: add FreelancerProfile persistence (config, repository, DbContext, DI)"
```

---

### Task 15: EF Migration

**Files:**
- Generated: `AppTrack.Persistance/Migrations/` (auto-generated by `dotnet ef`)

- [ ] **Step 1: Generate migration**

```bash
dotnet ef migrations add AddFreelancerProfileTable \
  --project AppTrack.Persistance \
  --startup-project AppTrack.Api
```

Expected: migration files created in `AppTrack.Persistance/Migrations/`.

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Persistance/Migrations/
git commit -m "feat: add EF migration for FreelancerProfile table"
```

---

### Task 16: `ProfileController`

**Files:**
- Create: `AppTrack.Api/Controllers/ProfileController.cs`

- [ ] **Step 1: Create the file**

```csharp
using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.Features.FreelancerProfile.Queries.GetFreelancerProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/profile")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET api/profile
    [HttpGet]
    [ProducesResponseType(typeof(FreelancerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FreelancerProfileDto>> Get()
    {
        var dto = await _mediator.Send(new GetFreelancerProfileQuery());
        return Ok(dto);
    }

    // PUT api/profile
    [HttpPut]
    [ProducesResponseType(typeof(FreelancerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FreelancerProfileDto>> Put([FromBody] UpsertFreelancerProfileCommand command)
    {
        var dto = await _mediator.Send(command);
        return Ok(dto);
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 3: Commit**

```bash
git add AppTrack.Api/Controllers/ProfileController.cs
git commit -m "feat: add ProfileController with GET and PUT api/profile endpoints"
```

---

## Chunk 5: ApiService + Frontend Models + Blazor Wiring

### Task 17: Regenerate NSwag client

**Files:**
- Modify: `ApiService/Base/ServiceClient.cs` (auto-generated)

The NSwag client reads from the running API's Swagger endpoint. The API must be running locally for this step.

- [ ] **Step 1: Start the API**

Run `AppTrack.Api` locally (F5 in Visual Studio or `dotnet run --project AppTrack.Api`). Confirm it starts at `https://localhost:7273`.

- [ ] **Step 2: Regenerate the client**

Open `ApiService/Base/clientsettings.nswag` in Visual Studio and click **Generate Files**, or run via NSwag CLI:

```bash
cd ApiService/Base
nswag run clientsettings.nswag
```

- [ ] **Step 3: Verify new methods exist in `ServiceClient.cs`**

Open `ApiService/Base/ServiceClient.cs` and verify it contains:
- A `ProfileGETAsync()` method returning `Task<FreelancerProfileDto>`
- A `ProfilePUTAsync(UpsertFreelancerProfileCommand body)` method returning `Task<FreelancerProfileDto>`
- Standalone enum types `RemotePreference` and `ApplicationLanguage` in the `AppTrack.Frontend.ApiService.Base` namespace

- [ ] **Step 4: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 5: Commit**

```bash
git add ApiService/Base/ServiceClient.cs \
        ApiService/Base/clientsettings.nswag
git commit -m "feat: regenerate NSwag client with profile endpoints"
```

---

### Task 18: `FreelancerProfileModel` update + `IFreelancerProfileService` + `FreelancerProfileService` + Mappings

**Files:**
- Modify: `Models/FreelancerProfileModel.cs`
- Create: `ApiService/Contracts/IFreelancerProfileService.cs`
- Create: `ApiService/Services/FreelancerProfileService.cs`
- Create: `ApiService/Mappings/FreelancerProfileMappings.cs`
- Modify: `ApiService/ApiServiceRegistration.cs`

> **Note:** `FreelancerProfileModel` must extend `ModelBase` before the mappings build step, because `FreelancerProfileMappings.ToModel()` assigns `Id`, `CreationDate`, and `ModifiedDate` which come from `ModelBase`. That is why this task begins with the model update.

- [ ] **Step 1: Update `FreelancerProfileModel.cs`**

Replace the class declaration to extend `ModelBase` and implement `IFreelancerProfileValidatable`:

```csharp
using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Frontend.Models;

public class FreelancerProfileModel : ModelBase, IFreelancerProfileValidatable
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public RateKind? SelectedRateType { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? DailyRate { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public RemotePreference? WorkMode { get; set; }
    public string? Skills { get; set; }
    public ApplicationLanguage? Language { get; set; }
}
```

- [ ] **Step 2: Create `IFreelancerProfileService.cs`**

```csharp
using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IFreelancerProfileService
{
    Task<Response<FreelancerProfileDto>> GetProfileAsync();
    Task<Response<FreelancerProfileDto>> UpsertProfileAsync(FreelancerProfileModel model);
}
```

- [ ] **Step 3: Create `FreelancerProfileMappings.cs`**

```csharp
using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Mappings;

public static class FreelancerProfileMappings
{
    public static UpsertFreelancerProfileCommand ToUpsertCommand(this FreelancerProfileModel model) => new()
    {
        FirstName = model.FirstName,
        LastName = model.LastName,
        HourlyRate = (double?)model.HourlyRate,
        DailyRate = (double?)model.DailyRate,
        AvailableFrom = model.AvailableFrom.HasValue
            ? model.AvailableFrom.Value.ToDateTime(TimeOnly.MinValue)
            : (DateTime?)null,
        WorkMode = (AppTrack.Frontend.ApiService.Base.RemotePreference?)(int?)model.WorkMode,
        Skills = model.Skills,
        Language = (AppTrack.Frontend.ApiService.Base.ApplicationLanguage?)(int?)model.Language,
        // SelectedRateType is UI-only and intentionally not mapped
    };

    public static FreelancerProfileModel ToModel(this FreelancerProfileDto dto) => new()
    {
        Id = dto.Id,
        FirstName = dto.FirstName ?? string.Empty,
        LastName = dto.LastName ?? string.Empty,
        HourlyRate = (decimal?)dto.HourlyRate,
        DailyRate = (decimal?)dto.DailyRate,
        AvailableFrom = dto.AvailableFrom.HasValue
            ? DateOnly.FromDateTime(dto.AvailableFrom.Value)
            : (DateOnly?)null,
        WorkMode = (AppTrack.Frontend.Models.RemotePreference?)(int?)dto.WorkMode,
        Skills = dto.Skills,
        Language = (AppTrack.Frontend.Models.ApplicationLanguage?)(int?)dto.Language,
        SelectedRateType = dto.HourlyRate.HasValue ? AppTrack.Frontend.Models.RateKind.Hourly
                         : dto.DailyRate.HasValue  ? AppTrack.Frontend.Models.RateKind.Daily
                         : (AppTrack.Frontend.Models.RateKind?)null,
        CreationDate = dto.CreationDate,
        ModifiedDate = dto.ModifiedDate,
    };
}
```

- [ ] **Step 4: Create `FreelancerProfileService.cs`**

```csharp
using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Mappings;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services;

public class FreelancerProfileService : BaseHttpService, IFreelancerProfileService
{
    public FreelancerProfileService(IClient client) : base(client)
    {
    }

    public Task<Response<FreelancerProfileDto>> GetProfileAsync() =>
        TryExecuteAsync(async () =>
        {
            var dto = await _client.ProfileGETAsync();
            return dto;
        });

    public Task<Response<FreelancerProfileDto>> UpsertProfileAsync(FreelancerProfileModel model) =>
        TryExecuteAsync(async () =>
        {
            var command = model.ToUpsertCommand();
            var dto = await _client.ProfilePUTAsync(command);
            return dto;
        });
}
```

- [ ] **Step 5: Register in `ApiServiceRegistration.cs`**

Add after the last `services.AddScoped` line in `ApiServiceRegistration.cs`:

```csharp
services.AddScoped<IFreelancerProfileService, FreelancerProfileService>();
```

- [ ] **Step 6: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 7: Commit**

```bash
git add Models/FreelancerProfileModel.cs \
        ApiService/Contracts/IFreelancerProfileService.cs \
        ApiService/Services/FreelancerProfileService.cs \
        ApiService/Mappings/FreelancerProfileMappings.cs \
        ApiService/ApiServiceRegistration.cs
git commit -m "feat: extend FreelancerProfileModel with ModelBase and add FreelancerProfileService and mappings"
```

---

### Task 19: Frontend Validator + DI registration

> **Note:** `FreelancerProfileModel` was already extended with `ModelBase` in Task 18 Step 1. This task only adds the validator and registers it.

**Files:**
- Create: `Models/Validators/FreelancerProfileModelValidator.cs`
- Modify: `AppTrack.BlazorUi/Program.cs`

- [ ] **Step 1: Create `FreelancerProfileModelValidator.cs`**

```csharp
using AppTrack.Shared.Validation.Validators;

namespace AppTrack.Frontend.Models.Validators;

public class FreelancerProfileModelValidator : FreelancerProfileBaseValidator<FreelancerProfileModel>
{
    public FreelancerProfileModelValidator()
    {
    }
}
```

- [ ] **Step 2: Register in `Program.cs`**

In `AppTrack.BlazorUi/Program.cs`, add after the existing `AddTransient<IValidator<...>>` registrations:

```csharp
builder.Services.AddTransient<IValidator<FreelancerProfileModel>, FreelancerProfileModelValidator>();
```

- [ ] **Step 3: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Commit**

```bash
git add Models/Validators/FreelancerProfileModelValidator.cs \
        AppTrack.BlazorUi/Program.cs
git commit -m "feat: add FreelancerProfileModelValidator and register in DI"
```

---

### Task 20: `FreelancerProfileForm` refactor (Model parameter + validation wiring)

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor.cs`
- Modify: `AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor`

- [ ] **Step 1: Rewrite `FreelancerProfileForm.razor.cs`**

```csharp
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;

namespace AppTrack.BlazorUi.Components.Profile;

public partial class FreelancerProfileForm
{
    [Parameter] public FreelancerProfileModel Model { get; set; } = new();
    [Inject] private IModelValidator<FreelancerProfileModel> ModelValidator { get; set; } = null!;

    private string _selectedType = "Freelancer";
    private DateTime? _availableFrom;

    protected override void OnParametersSet()
    {
        _availableFrom = Model.AvailableFrom.HasValue
            ? Model.AvailableFrom.Value.ToDateTime(TimeOnly.MinValue)
            : null;
    }

    public bool Validate() => ModelValidator.Validate(Model);

    private string GetFirstError(string propertyName) =>
        ModelValidator.Errors.GetValueOrDefault(propertyName)?.FirstOrDefault() ?? string.Empty;

    private void SelectFreelancer() => _selectedType = "Freelancer";

    private void OnFirstNameChanged(string value)
    {
        Model.FirstName = value;
        ModelValidator.ResetErrors(nameof(Model.FirstName));
    }

    private void OnLastNameChanged(string value)
    {
        Model.LastName = value;
        ModelValidator.ResetErrors(nameof(Model.LastName));
    }

    private void OnAvailableFromChanged(DateTime? date)
    {
        _availableFrom = date;
        Model.AvailableFrom = date.HasValue ? DateOnly.FromDateTime(date.Value) : null;
    }

    private void OnRateKindChanged(RateKind? newKind)
    {
        if (Model.SelectedRateType == newKind) return;
        Model.SelectedRateType = newKind;
        if (newKind == RateKind.Hourly)
        {
            Model.DailyRate = null;
            ModelValidator.ResetErrors(nameof(Model.DailyRate));
        }
        else if (newKind == RateKind.Daily)
        {
            Model.HourlyRate = null;
            ModelValidator.ResetErrors(nameof(Model.HourlyRate));
        }
        else
        {
            Model.HourlyRate = null;
            Model.DailyRate = null;
            ModelValidator.ResetErrors(nameof(Model.HourlyRate));
            ModelValidator.ResetErrors(nameof(Model.DailyRate));
        }
    }

    private void OnRateValueChanged(decimal? value)
    {
        if (Model.SelectedRateType == RateKind.Hourly)
        {
            Model.HourlyRate = value;
            ModelValidator.ResetErrors(nameof(Model.HourlyRate));
        }
        else if (Model.SelectedRateType == RateKind.Daily)
        {
            Model.DailyRate = value;
            ModelValidator.ResetErrors(nameof(Model.DailyRate));
        }
    }

    private void OnSkillsChanged(string? value)
    {
        Model.Skills = value;
        ModelValidator.ResetErrors(nameof(Model.Skills));
    }

    private string GetCardStyle(string type) =>
        _selectedType == type
            ? "cursor: pointer; border: 2px solid var(--mud-palette-primary);"
            : "cursor: pointer;";
}
```

- [ ] **Step 2: Rewrite `FreelancerProfileForm.razor`**

Replace the entire file with the following (all `_model` references changed to `Model`, field `Error`/`ErrorText` added for validated fields, `@bind-Value` replaced with `Value=`/`ValueChanged=` for validated fields):

```razor
<MudStack Spacing="4">

    <!-- ── Section 1: Profile Type ─────────────────────────────── -->
    <div>
        <MudText Typo="Typo.subtitle1" Class="mb-2">Profile Type</MudText>
        <MudGrid Spacing="2">
            <MudItem xs="12" sm="6">
                <MudCard @onclick="SelectFreelancer"
                         Style="@GetCardStyle("Freelancer")"
                         Elevation="2">
                    <MudCardContent>
                        <MudStack AlignItems="AlignItems.Center" Spacing="1">
                            <MudIcon Icon="@Icons.Material.Filled.WorkspacePremium"
                                     Size="Size.Large"
                                     Color="Color.Primary" />
                            <MudText Typo="Typo.h6">Freelancer</MudText>
                        </MudStack>
                    </MudCardContent>
                </MudCard>
            </MudItem>
            <MudItem xs="12" sm="6">
                <MudTooltip Text="Coming soon">
                    <MudCard Style="pointer-events: none; opacity: 0.5;" Elevation="1">
                        <MudCardContent>
                            <MudStack AlignItems="AlignItems.Center" Spacing="1">
                                <MudIcon Icon="@Icons.Material.Filled.BusinessCenter"
                                         Size="Size.Large"
                                         Color="Color.Secondary" />
                                <MudText Typo="Typo.h6">Employee</MudText>
                            </MudStack>
                        </MudCardContent>
                    </MudCard>
                </MudTooltip>
            </MudItem>
        </MudGrid>
    </div>

    <!-- ── Section 2: Personal Details ─────────────────────────── -->
    <div>
        <MudText Typo="Typo.subtitle1" Class="mb-2">Personal Details</MudText>
        <MudGrid Spacing="2">

            <!-- First Name -->
            <MudItem xs="12" sm="6">
                <MudTextField T="string"
                              Label="First Name"
                              Value="@Model.FirstName"
                              ValueChanged="@((string v) => OnFirstNameChanged(v))"
                              Error="@ModelValidator.Errors.ContainsKey(nameof(Model.FirstName))"
                              ErrorText="@GetFirstError(nameof(Model.FirstName))"
                              Variant="Variant.Outlined"
                              Adornment="Adornment.Start"
                              AdornmentIcon="@Icons.Material.Outlined.Person" />
            </MudItem>

            <!-- Last Name -->
            <MudItem xs="12" sm="6">
                <MudTextField T="string"
                              Label="Last Name"
                              Value="@Model.LastName"
                              ValueChanged="@((string v) => OnLastNameChanged(v))"
                              Error="@ModelValidator.Errors.ContainsKey(nameof(Model.LastName))"
                              ErrorText="@GetFirstError(nameof(Model.LastName))"
                              Variant="Variant.Outlined"
                              Adornment="Adornment.Start"
                              AdornmentIcon="@Icons.Material.Outlined.Person" />
            </MudItem>

            <!-- Rate Type Toggle -->
            <MudItem xs="12">
                <MudToggleGroup T="RateKind?" Value="Model.SelectedRateType"
                                ValueChanged="OnRateKindChanged"
                                SelectionMode="SelectionMode.ToggleSelection"
                                Color="Color.Primary" Outlined="true">
                    <MudToggleItem Value="@((RateKind?)RateKind.Hourly)">Hourly Rate</MudToggleItem>
                    <MudToggleItem Value="@((RateKind?)RateKind.Daily)">Daily Rate</MudToggleItem>
                </MudToggleGroup>
            </MudItem>

            <!-- Active Rate Field -->
            @if (Model.SelectedRateType != null)
            {
                <MudItem @key="Model.SelectedRateType" xs="12" sm="6">
                    <MudNumericField T="decimal?"
                                     Label="@(Model.SelectedRateType == RateKind.Hourly ? "Hourly Rate (€)" : "Daily Rate (€)")"
                                     Value="@(Model.SelectedRateType == RateKind.Hourly ? Model.HourlyRate : Model.DailyRate)"
                                     ValueChanged="OnRateValueChanged"
                                     Error="@(Model.SelectedRateType == RateKind.Hourly
                                               ? ModelValidator.Errors.ContainsKey(nameof(Model.HourlyRate))
                                               : ModelValidator.Errors.ContainsKey(nameof(Model.DailyRate)))"
                                     ErrorText="@(Model.SelectedRateType == RateKind.Hourly
                                                  ? GetFirstError(nameof(Model.HourlyRate))
                                                  : GetFirstError(nameof(Model.DailyRate)))"
                                     Variant="Variant.Outlined"
                                     Adornment="Adornment.Start"
                                     AdornmentIcon="@Icons.Material.Outlined.Euro" />
                </MudItem>
            }

            <!-- Available From -->
            <MudItem xs="12">
                <MudDatePicker Label="Available From"
                               Date="_availableFrom"
                               DateChanged="OnAvailableFromChanged"
                               Variant="Variant.Outlined"
                               DateFormat="dd.MM.yyyy"
                               Editable="true" />
            </MudItem>

            <!-- Remote Preference -->
            <MudItem xs="12">
                <MudSelect T="RemotePreference?"
                           Label="Remote Preference"
                           Value="Model.WorkMode"
                           ValueChanged="@((RemotePreference? v) => Model.WorkMode = v)"
                           Variant="Variant.Outlined"
                           Clearable="true"
                           AdornmentIcon="@Icons.Material.Outlined.LocationOn">
                    <MudSelectItem T="RemotePreference?" Value="@((RemotePreference?)RemotePreference.Remote)">Remote</MudSelectItem>
                    <MudSelectItem T="RemotePreference?" Value="@((RemotePreference?)RemotePreference.Hybrid)">Hybrid</MudSelectItem>
                    <MudSelectItem T="RemotePreference?" Value="@((RemotePreference?)RemotePreference.OnSite)">On-Site</MudSelectItem>
                </MudSelect>
            </MudItem>

            <!-- Skills -->
            <MudItem xs="12">
                <MudTextField T="string"
                              Label="Skills"
                              Value="@(Model.Skills ?? string.Empty)"
                              ValueChanged="@((string v) => OnSkillsChanged(v))"
                              Error="@ModelValidator.Errors.ContainsKey(nameof(Model.Skills))"
                              ErrorText="@GetFirstError(nameof(Model.Skills))"
                              Variant="Variant.Outlined"
                              Lines="3"
                              Placeholder="e.g. C#, .NET, Azure, SQL" />
            </MudItem>

            <!-- Application Language -->
            <MudItem xs="12">
                <MudSelect T="ApplicationLanguage?"
                           Label="Application Language"
                           Value="Model.Language"
                           ValueChanged="@((ApplicationLanguage? v) => Model.Language = v)"
                           Variant="Variant.Outlined"
                           Clearable="true"
                           HelperText="Language for AI-generated application text"
                           AdornmentIcon="@Icons.Material.Outlined.Translate">
                    <MudSelectItem T="ApplicationLanguage?" Value="@((ApplicationLanguage?)ApplicationLanguage.German)">German</MudSelectItem>
                    <MudSelectItem T="ApplicationLanguage?" Value="@((ApplicationLanguage?)ApplicationLanguage.English)">English</MudSelectItem>
                </MudSelect>
            </MudItem>

            <!-- CV Upload (decorative) -->
            <MudItem xs="12">
                <MudFileUpload T="IBrowserFile" Accept=".pdf,.doc,.docx">
                    <CustomContent>
                        <MudButton Variant="Variant.Outlined"
                                   StartIcon="@Icons.Material.Filled.AttachFile"
                                   Color="Color.Default"
                                   OnClick="@context.OpenFilePickerAsync">
                            Upload CV (PDF)
                        </MudButton>
                    </CustomContent>
                </MudFileUpload>
            </MudItem>

        </MudGrid>
    </div>

</MudStack>
```

- [ ] **Step 3: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Commit**

```bash
git add AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor.cs \
        AppTrack.BlazorUi/Components/Profile/FreelancerProfileForm.razor
git commit -m "feat: refactor FreelancerProfileForm to accept Model parameter and add validation wiring"
```

---

### Task 21: Wire up `ProfileSetup` page

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Pages/ProfileSetup.razor.cs`
- Modify: `AppTrack.BlazorUi/Components/Pages/ProfileSetup.razor`

- [ ] **Step 1: Rewrite `ProfileSetup.razor.cs`**

```csharp
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Mappings;
using AppTrack.Frontend.Models;
using AppTrack.BlazorUi.Components.Profile;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Pages;

public partial class ProfileSetup
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IFreelancerProfileService ProfileService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private FreelancerProfileModel _model = new();
    private FreelancerProfileForm _form = null!;
    private bool _isBusy;

    protected override async Task OnInitializedAsync()
    {
        var response = await ProfileService.GetProfileAsync();
        if (response.Success && response.Data is not null)
        {
            _model = response.Data.ToModel();
        }
    }

    private async Task Save()
    {
        if (!_form.Validate()) return;

        _isBusy = true;
        var response = await ProfileService.UpsertProfileAsync(_model);
        _isBusy = false;

        if (response.Success)
        {
            Snackbar.Add("Profile saved", Severity.Success);
            Navigation.NavigateTo("/");
        }
        else
        {
            Snackbar.Add(response.ErrorMessage ?? "Failed to save profile", Severity.Error);
        }
    }
}
```

- [ ] **Step 2: Update `ProfileSetup.razor`**

Replace `<FreelancerProfileForm />` with `<FreelancerProfileForm @ref="_form" Model="_model" />` and update the Save button to be async and show a spinner when busy:

```razor
@page "/profile/setup"

<PageTitle>My Profile - AppTrack</PageTitle>

<MudContainer MaxWidth="MaxWidth.Small" Class="mt-6">
    <MudStack Row="true" AlignItems="AlignItems.Center" Class="mb-4">
        <MudIcon Icon="@Icons.Material.Filled.AccountCircle" Class="mr-2" Color="Color.Primary" />
        <MudText Typo="Typo.h5">My Profile</MudText>
    </MudStack>

    <FreelancerProfileForm @ref="_form" Model="_model" />

    <MudStack Row="true" Justify="Justify.FlexEnd" Class="mt-4">
        <MudButton Variant="Variant.Filled"
                   Color="Color.Primary"
                   StartIcon="@Icons.Material.Filled.Save"
                   OnClick="Save"
                   Disabled="_isBusy">
            @if (_isBusy)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
                <span>Saving…</span>
            }
            else
            {
                <span>Save</span>
            }
        </MudButton>
    </MudStack>
</MudContainer>
```

- [ ] **Step 3: Build**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Commit**

```bash
git add AppTrack.BlazorUi/Components/Pages/ProfileSetup.razor.cs \
        AppTrack.BlazorUi/Components/Pages/ProfileSetup.razor
git commit -m "feat: wire up ProfileSetup page to API with validation and snackbar feedback"
```

---

### Task 22: Wire up `ProfileSetupDialog`

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Dialogs/ProfileSetupDialog.razor.cs`
- Modify: `AppTrack.BlazorUi/Components/Dialogs/ProfileSetupDialog.razor`

- [ ] **Step 1: Rewrite `ProfileSetupDialog.razor.cs`**

```csharp
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Mappings;
using AppTrack.Frontend.Models;
using AppTrack.BlazorUi.Components.Profile;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class ProfileSetupDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Inject] private IFreelancerProfileService ProfileService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private FreelancerProfileModel _model = new();
    private FreelancerProfileForm _form = null!;
    private bool _isBusy;

    protected override async Task OnInitializedAsync()
    {
        var response = await ProfileService.GetProfileAsync();
        if (response.Success && response.Data is not null)
        {
            _model = response.Data.ToModel();
        }
    }

    private async Task Save()
    {
        if (!_form.Validate()) return;

        _isBusy = true;
        var response = await ProfileService.UpsertProfileAsync(_model);
        _isBusy = false;

        if (response.Success)
        {
            MudDialog.Close();
        }
        else
        {
            Snackbar.Add(response.ErrorMessage ?? "Failed to save profile", Severity.Error);
        }
    }

    private void Skip() => MudDialog.Cancel();
}
```

- [ ] **Step 2: Update `ProfileSetupDialog.razor`**

Replace `<FreelancerProfileForm />` with `<FreelancerProfileForm @ref="_form" Model="_model" />` and update action buttons:

```razor
<MudDialog Class="apptrack-dialog">
    <TitleContent>
        <MudText Typo="Typo.h6">
            <MudIcon Icon="@Icons.Material.Filled.AccountCircle" Class="mr-2" />
            My Profile
        </MudText>
    </TitleContent>
    <DialogContent>
        <FreelancerProfileForm @ref="_form" Model="_model" />
    </DialogContent>
    <DialogActions>
        <MudButton Variant="Variant.Text" OnClick="Skip" Disabled="_isBusy">Skip</MudButton>
        <MudSpacer />
        <MudButton Variant="Variant.Filled"
                   Color="Color.Primary"
                   OnClick="Save"
                   Disabled="_isBusy">
            @if (_isBusy)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
                <span>Saving…</span>
            }
            else
            {
                <span>Save</span>
            }
        </MudButton>
    </DialogActions>
</MudDialog>
```

- [ ] **Step 3: Final build of the full solution**

```bash
dotnet build AppTrack.sln --configuration Release --no-restore
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 4: Run all unit tests**

```bash
dotnet test AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

Expected: all tests **PASS**, 0 failed.

- [ ] **Step 5: Commit**

```bash
git add AppTrack.BlazorUi/Components/Dialogs/ProfileSetupDialog.razor.cs \
        AppTrack.BlazorUi/Components/Dialogs/ProfileSetupDialog.razor
git commit -m "feat: wire up ProfileSetupDialog to API with validation and snackbar feedback"
```
