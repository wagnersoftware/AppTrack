# Send Prompt Selection Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a prompt selection dropdown to the Blazor AI text generation dialog and rename the trigger button to "Send Prompt".

**Architecture:** New `GetPromptNamesQuery` returns the user's saved prompt names from the backend. `GeneratePromptQuery` is extended with a required `PromptName` field. The Blazor `GenerateTextDialog` receives prompt names as a parameter, auto-selects the first, and reloads the generated prompt when the user changes selection.

**Tech Stack:** .NET 10, ASP.NET Core, MediatR, FluentValidation, EF Core, MudBlazor, NSwag-generated client.

**Spec:** `docs/superpowers/specs/2026-03-22-send-prompt-selection-design.md`

---

## Chunk 1: New GetPromptNamesQuery (Backend)

**Files:**
- Create: `AppTrack.Application/Features/ApplicationText/Dto/GetPromptNamesDto.cs`
- Create: `AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/GetPromptNamesQuery.cs`
- Create: `AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/GetPromptNamesQueryHandler.cs`
- Create: `AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/GetPromptNamesQueryValidator.cs`
- Create: `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GetPromptNamesQueryHandlerTests.cs`
- Create: `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GetPromptNamesQueryValidatorTests.cs`
- Modify: `AppTrack.Api/Controllers/AiController.cs`

---

### Task 1: GetPromptNamesDto

- [ ] **Create `AppTrack.Application/Features/ApplicationText/Dto/GetPromptNamesDto.cs`:**

```csharp
namespace AppTrack.Application.Features.ApplicationText.Dto;

public class GetPromptNamesDto
{
    public List<string> Names { get; set; } = [];
}
```

---

### Task 2: GetPromptNamesQuery, Handler, Validator

- [ ] **Create `AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/GetPromptNamesQuery.cs`:**

```csharp
using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ApplicationText.Dto;

namespace AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;

public class GetPromptNamesQuery : IRequest<GetPromptNamesDto>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
```

- [ ] **Create `AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/GetPromptNamesQueryValidator.cs`:**

```csharp
using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;

public class GetPromptNamesQueryValidator : AbstractValidator<GetPromptNamesQuery>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public GetPromptNamesQueryValidator(IAiSettingsRepository aiSettingsRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;

        RuleFor(x => x)
            .CustomAsync(ValidateAiSettings);
    }

    private async Task ValidateAiSettings(GetPromptNamesQuery query, ValidationContext<GetPromptNamesQuery> context, CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(query.UserId);

        if (aiSettings == null)
            context.AddFailure("AI settings not found for this user.");
    }
}
```

- [ ] **Create `AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/GetPromptNamesQueryHandler.cs`:**

```csharp
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;

namespace AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;

public class GetPromptNamesQueryHandler : IRequestHandler<GetPromptNamesQuery, GetPromptNamesDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public GetPromptNamesQueryHandler(IAiSettingsRepository aiSettingsRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;
    }

    public async Task<GetPromptNamesDto> Handle(GetPromptNamesQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetPromptNamesQueryValidator(_aiSettingsRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid request.", validationResult);

        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId);
        var names = aiSettings!.Prompts.Select(p => p.Name).ToList();

        return new GetPromptNamesDto { Names = names };
    }
}
```

---

### Task 3: Write failing tests for GetPromptNamesQuery

- [ ] **Create `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GetPromptNamesQueryHandlerTests.cs`:**

```csharp
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.ApplicationText.Queries;

public class GetPromptNamesQueryHandlerTests
{
    private const string UserId = "user-1";
    private readonly Mock<IAiSettingsRepository> _mockAiSettingsRepo = new();

    private GetPromptNamesQueryHandler CreateHandler() =>
        new(_mockAiSettingsRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnNamesInInsertionOrder_WhenAiSettingsHaveMultiplePrompts()
    {
        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(AppTrack.Domain.Prompt.Create("Cover Letter", "template A"));
        aiSettings.Prompts.Add(AppTrack.Domain.Prompt.Create("LinkedIn Message", "template B"));
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(aiSettings);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBe(["Cover Letter", "LinkedIn Message"]);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAiSettingsHaveNoPrompts()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId });

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAiSettingsNotFound()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync((DomainAiSettings?)null);

        await Should.ThrowAsync<BadRequestException>(() =>
            CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None));
    }
}
```

- [ ] **Create `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GetPromptNamesQueryValidatorTests.cs`:**

```csharp
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;
using FluentValidation.TestHelper;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.ApplicationText.Queries;

public class GetPromptNamesQueryValidatorTests
{
    private const string UserId = "user-1";
    private readonly Mock<IAiSettingsRepository> _aiSettingsRepo = new();
    private readonly GetPromptNamesQueryValidator _validator;

    public GetPromptNamesQueryValidatorTests()
    {
        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId });
        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(It.Is<string>(id => id != UserId)))
            .ReturnsAsync((DomainAiSettings?)null);

        _validator = new GetPromptNamesQueryValidator(_aiSettingsRepo.Object);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenAiSettingsExist()
    {
        var result = await _validator.TestValidateAsync(new GetPromptNamesQuery { UserId = UserId });
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenAiSettingsNotFound()
    {
        var result = await _validator.TestValidateAsync(new GetPromptNamesQuery { UserId = "unknown-user" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "AI settings not found for this user.");
    }
}
```

- [ ] **Run tests — expect FAIL** (handler/validator don't exist yet):

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release --filter "FullyQualifiedName~GetPromptNamesQuery"
```

---

### Task 4: Add controller endpoint

- [ ] **Modify `AppTrack.Api/Controllers/AiController.cs`** — add the new endpoint and required usings:

```csharp
using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;
using AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/ai")]
[ApiController]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    // POST /api/ai/generate-prompt
    [HttpPost("generate-prompt")]
    [ProducesResponseType(typeof(GeneratedPromptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GeneratedPromptDto>> GeneratePrompt([FromBody] GeneratePromptQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // GET /api/ai/prompt-names
    [HttpGet("prompt-names")]
    [ProducesResponseType(typeof(GetPromptNamesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetPromptNamesDto>> GetPromptNames()
    {
        var result = await _mediator.Send(new GetPromptNamesQuery());
        return Ok(result);
    }
}
```

---

### Task 5: Build and run tests — expect PASS

- [ ] **Build:**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: 0 errors, 0 warnings.

- [ ] **Run new tests:**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release --filter "FullyQualifiedName~GetPromptNamesQuery"
```

Expected: 4 tests pass.

- [ ] **Commit:**

```bash
git add AppTrack.Application/Features/ApplicationText/Dto/GetPromptNamesDto.cs \
        AppTrack.Application/Features/ApplicationText/Query/GetPromptNamesQuery/ \
        AppTrack.Api/Controllers/AiController.cs \
        AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GetPromptNamesQueryHandlerTests.cs \
        AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GetPromptNamesQueryValidatorTests.cs
git commit -m "feat: add GetPromptNamesQuery and GET /api/ai/prompt-names endpoint"
```

---

## Chunk 2: Extend GeneratePromptQuery (Backend)

**Files:**
- Modify: `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQuery.cs`
- Modify: `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryValidator.cs`
- Modify: `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryHandler.cs`
- Modify: `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryValidatorTests.cs`
- Modify: `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryHandlerTests.cs`

---

### Task 6: Add PromptName to GeneratePromptQuery

- [ ] **Modify `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQuery.cs`** — add `PromptName`:

```csharp
using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ApplicationText.Dto;

namespace AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;

public class GeneratePromptQuery : IRequest<GeneratedPromptDto>, IUserScopedRequest
{
    public int JobApplicationId { get; set; }
    public string PromptName { get; set; } = string.Empty;
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
```

---

### Task 7: Update GeneratePromptQueryValidator

- [ ] **Modify `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryValidator.cs`:**

```csharp
using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;

public class GeneratePromptQueryValidator : AbstractValidator<GeneratePromptQuery>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public GeneratePromptQueryValidator(IJobApplicationRepository jobApplicationRepository, IAiSettingsRepository aiSettingsRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _aiSettingsRepository = aiSettingsRepository;

        RuleFor(x => x.JobApplicationId)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.PromptName)
            .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(x => x)
            .MustAsync(JobApplicationExists)
            .WithMessage("Job application doesn't exist");

        RuleFor(x => x)
            .CustomAsync(ValidateAiSettings);
    }

    private async Task<bool> JobApplicationExists(GeneratePromptQuery query, CancellationToken token)
    {
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(query.JobApplicationId);
        return jobApplication != null;
    }

    private async Task ValidateAiSettings(GeneratePromptQuery query, ValidationContext<GeneratePromptQuery> context, CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(query.UserId);

        if (aiSettings == null)
        {
            context.AddFailure("AI settings not found for this user.");
            return;
        }

        var prompt = aiSettings.Prompts.FirstOrDefault(p => p.Name == query.PromptName);

        if (prompt == null)
        {
            context.AddFailure("Prompt not found in AI settings.");
            return;
        }

        if (string.IsNullOrWhiteSpace(prompt.PromptTemplate))
            context.AddFailure("Prompt template is empty.");
    }
}
```

---

### Task 8: Update GeneratePromptQueryHandler

- [ ] **Modify `AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/GeneratePromptQueryHandler.cs`** — change line 43 only (prompt template lookup):

```csharp
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Domain.Contracts;
using AppTrack.Domain.Extensions;

namespace AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;

public class GeneratePromptQueryHandler : IRequestHandler<GeneratePromptQuery, GeneratedPromptDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IPromptBuilder _promptBuilder;

    public GeneratePromptQueryHandler(IAiSettingsRepository aiSettingsRepository, IJobApplicationRepository jobApplicationRepository, IPromptBuilder promptBuilder)
    {
        this._aiSettingsRepository = aiSettingsRepository;
        this._jobApplicationRepository = jobApplicationRepository;
        this._promptBuilder = promptBuilder;
    }

    public async Task<GeneratedPromptDto> Handle(GeneratePromptQuery request, CancellationToken cancellationToken)
    {
        var validator = new GeneratePromptQueryValidator(_jobApplicationRepository, _aiSettingsRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid Ai setting.", validationResult);
        }

        //get Ai settings
        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId);

        //get job application
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId);

        //build prompt
        var applicantParameter = aiSettings!.PromptParameter.ToList();
        var jobApplicationParameter = jobApplication!.ToPromptParameters().ToList();
        var promptParameter = jobApplicationParameter.Union(applicantParameter).ToList();
        var promptTemplate = aiSettings!.Prompts.First(p => p.Name == request.PromptName).PromptTemplate;
        var (prompt, unusedKeys) = _promptBuilder.BuildPrompt(promptParameter, promptTemplate);

        return new GeneratedPromptDto() { Prompt = prompt, UnusedKeys = unusedKeys };
    }
}
```

---

### Task 9: Update existing unit tests

The existing `GeneratePromptQueryValidatorTests` and `GeneratePromptQueryHandlerTests` must be updated because `PromptName` is now required and the old "no prompts configured" tests are obsolete.

- [ ] **Modify `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryValidatorTests.cs`:**

```csharp
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;
using FluentValidation.TestHelper;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;
using DomainJobApplication = AppTrack.Domain.JobApplication;
using DomainPrompt = AppTrack.Domain.Prompt;

namespace AppTrack.Application.UnitTests.Features.ApplicationText.Queries;

public class GeneratePromptQueryValidatorTests
{
    private const string UserId = "user-1";
    private const string PromptName = "Default";
    private const int ExistingJobApplicationId = 42;

    private readonly Mock<IJobApplicationRepository> _jobAppRepo;
    private readonly Mock<IAiSettingsRepository> _aiSettingsRepo;
    private readonly GeneratePromptQueryValidator _validator;

    public GeneratePromptQueryValidatorTests()
    {
        _jobAppRepo = new Mock<IJobApplicationRepository>();
        _aiSettingsRepo = new Mock<IAiSettingsRepository>();

        var jobApplication = new DomainJobApplication { Id = ExistingJobApplicationId, UserId = UserId };
        _jobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(jobApplication);
        _jobAppRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingJobApplicationId)))
            .ReturnsAsync((DomainJobApplication?)null);

        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(DomainPrompt.Create(PromptName, "Write a cover letter for {position}"));

        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(aiSettings);
        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(It.Is<string>(id => id != UserId)))
            .ReturnsAsync((DomainAiSettings?)null);

        _validator = new GeneratePromptQueryValidator(_jobAppRepo.Object, _aiSettingsRepo.Object);
    }

    private static GeneratePromptQuery BuildValidQuery(
        string userId = UserId,
        int jobApplicationId = ExistingJobApplicationId,
        string promptName = PromptName) => new()
    {
        UserId = userId,
        JobApplicationId = jobApplicationId,
        PromptName = promptName
    };

    [Fact]
    public async Task Validate_ShouldPass_WhenQueryIsValid()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationIdIsZero()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery(jobApplicationId: 0));
        result.ShouldHaveValidationErrorFor(x => x.JobApplicationId);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationDoesNotExist()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery(jobApplicationId: 9999));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Job application doesn't exist");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenAiSettingsNotFound()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery(userId: "unknown-user"));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "AI settings not found for this user.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenPromptNameIsEmpty()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery(promptName: ""));
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(x => x.PromptName);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenNamedPromptNotFound()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery(promptName: "NonExistentPrompt"));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Prompt not found in AI settings.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenNamedPromptTemplateIsEmpty()
    {
        const string emptyTemplateUser = "user-empty-template";
        var aiSettingsWithEmptyTemplate = new DomainAiSettings { Id = 3, UserId = emptyTemplateUser };
        aiSettingsWithEmptyTemplate.Prompts.Add(DomainPrompt.Create(PromptName, " "));

        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(emptyTemplateUser))
            .ReturnsAsync(aiSettingsWithEmptyTemplate);

        _jobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(new DomainJobApplication { Id = ExistingJobApplicationId, UserId = emptyTemplateUser });

        var result = await _validator.TestValidateAsync(BuildValidQuery(userId: emptyTemplateUser));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Prompt template is empty.");
    }
}
```

- [ ] **Modify `AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryHandlerTests.cs`:**

Update the constructor to add `PromptName = "Default"` to the default AiSettings setup. Replace `Handle_ShouldThrowBadRequestException_WhenNoPromptsConfigured` with `Handle_ShouldBuildPromptFromNamedTemplate`. Update all existing queries to include `PromptName`.

```csharp
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;
using AppTrack.Domain;
using AppTrack.Domain.Contracts;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.ApplicationText.Queries;

public class GeneratePromptQueryHandlerTests
{
    private const string UserId = "user-1";
    private const int JobApplicationId = 5;
    private const string PromptName = "Default";

    private readonly Mock<IAiSettingsRepository> _mockAiSettingsRepo;
    private readonly Mock<IJobApplicationRepository> _mockJobApplicationRepo;
    private readonly Mock<IPromptBuilder> _mockPromptBuilder;

    public GeneratePromptQueryHandlerTests()
    {
        _mockAiSettingsRepo = new Mock<IAiSettingsRepository>();
        _mockJobApplicationRepo = new Mock<IJobApplicationRepository>();
        _mockPromptBuilder = new Mock<IPromptBuilder>();

        var existingJobApplication = new JobApplication
        {
            Id = JobApplicationId,
            UserId = UserId,
            Name = "Test Company",
            Position = "Developer",
            URL = "https://company.com",
            JobDescription = "Job desc",
            Location = "Remote",
            ContactPerson = "Jane",
            Status = JobApplicationStatus.New,
            StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var existingAiSettings = new DomainAiSettings
        {
            Id = 1,
            UserId = UserId,
            Prompts = new List<AppTrack.Domain.Prompt> { AppTrack.Domain.Prompt.Create(PromptName, "Hello {Name}") },
            PromptParameter = new List<PromptParameter>()
        };

        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(JobApplicationId))
            .ReturnsAsync(existingJobApplication);

        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != JobApplicationId)))
            .ReturnsAsync((JobApplication?)null);

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(existingAiSettings);

        _mockPromptBuilder
            .Setup(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), It.IsAny<string>()))
            .Returns(("Hello Test Company", new List<string>()));
    }

    private GeneratePromptQueryHandler CreateHandler() =>
        new(_mockAiSettingsRepo.Object, _mockJobApplicationRepo.Object, _mockPromptBuilder.Object);

    [Fact]
    public async Task Handle_ShouldReturnGeneratedPromptDto_WhenQueryIsValid()
    {
        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptName = PromptName };

        var result = await CreateHandler().Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<GeneratedPromptDto>();
        result.Prompt.ShouldBe("Hello Test Company");
        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), "Hello {Name}"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldBuildPromptFromNamedTemplate()
    {
        // Verify handler selects the correct template by name, not first
        const string secondPromptName = "LinkedIn";
        const string secondTemplate = "LinkedIn template for {Name}";

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings
            {
                Id = 1,
                UserId = UserId,
                Prompts = new List<AppTrack.Domain.Prompt>
                {
                    AppTrack.Domain.Prompt.Create(PromptName, "Hello {Name}"),
                    AppTrack.Domain.Prompt.Create(secondPromptName, secondTemplate)
                },
                PromptParameter = new List<PromptParameter>()
            });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptName = secondPromptName };

        await CreateHandler().Handle(query, CancellationToken.None);

        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), secondTemplate), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationDoesNotExist()
    {
        var query = new GeneratePromptQuery { JobApplicationId = 9999, UserId = UserId, PromptName = PromptName };
        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAiSettingsDoNotExistForUser()
    {
        const string noSettingsUser = "user-no-settings";

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(noSettingsUser))
            .ReturnsAsync((DomainAiSettings?)null);

        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(JobApplicationId))
            .ReturnsAsync(new JobApplication { Id = JobApplicationId, UserId = noSettingsUser });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = noSettingsUser, PromptName = PromptName };
        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(query, CancellationToken.None));
    }
}
```

---

### Task 10: Build and run tests — expect PASS

- [ ] **Build:**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: 0 errors, 0 warnings.

- [ ] **Run all unit tests:**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

Expected: all tests pass.

- [ ] **Commit:**

```bash
git add AppTrack.Application/Features/ApplicationText/Query/GeneratePromptQuery/ \
        AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryValidatorTests.cs \
        AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryHandlerTests.cs
git commit -m "feat: add PromptName to GeneratePromptQuery — validator and handler updated"
```

---

## Chunk 3: NSwag Regeneration + Frontend Service

**Files:**
- Modify (auto-generated): `ApiService/Base/ServiceClient.cs`
- Modify (auto-generated): `ApiService/Base/clientsettings.nswag`
- Modify: `ApiService/Contracts/IApplicationTextService.cs`
- Modify: `ApiService/Services/ApplicationTextService.cs`

---

### Task 11: Regenerate NSwag client

The `ServiceClient.cs` is auto-generated from the running API's swagger doc. After the backend changes in Chunks 1 and 2, the generated client is out of date. Regenerate it now.

- [ ] **Start the API** (in a separate terminal, from the repo root):

```bash
dotnet run --project AppTrack.Api/AppTrack.Api.csproj
```

Wait for `Application started` in the output. The API runs on `https://localhost:7273` by default.

- [ ] **Regenerate the client** (in another terminal, from the repo root):

```bash
cd ApiService/Base && nswag run clientsettings.nswag
```

If `nswag` is not found, install it: `dotnet tool install -g NSwag.ConsoleCore`.

After this command, `ServiceClient.cs` is updated. It will now contain:
- `GetPromptNamesAsync()` method on `IClient` / `Client`
- `GetPromptNamesDto` class with `ICollection<string> Names`
- Updated `GeneratePromptQuery` class with `PromptName` property

- [ ] **Stop the API** (Ctrl+C in the API terminal).

- [ ] **Build to verify the new client compiles:**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: 0 errors. (The frontend service still uses the old `GeneratePrompt(int)` signature — that will cause a compile error until Task 12.)

---

### Task 12: Update IApplicationTextService and ApplicationTextService

- [ ] **Modify `ApiService/Contracts/IApplicationTextService.cs`:**

```csharp
using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IApplicationTextService
{
    Task<Response<ApplicationTextModel>> GenerateApplicationText(string prompt, int jobApplicationId, CancellationToken token);

    Task<Response<GeneratedPromptModel>> GeneratePrompt(int jobApplicationId, string promptName);

    Task<Response<List<string>>> GetPromptNames();
}
```

- [ ] **Modify `ApiService/Services/ApplicationTextService.cs`:**

```csharp
using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services
{
    public class ApplicationTextService : BaseHttpService, IApplicationTextService
    {
        public ApplicationTextService(IClient client) : base(client)
        {
        }

        public Task<Response<ApplicationTextModel>> GenerateApplicationText(string prompt, int jobApplicationId, CancellationToken token) =>
            TryExecuteAsync(async () =>
            {
                var command = new GenerateApplicationTextCommand() { Prompt = prompt, JobApplicationId = jobApplicationId };
                var generatedTextDto = await _client.GenerateApplicationTextAsync(command, token);
                return new ApplicationTextModel()
                {
                    Text = generatedTextDto.ApplicationText,
                    WindowTitle = "Generated application text",
                };
            });

        public Task<Response<GeneratedPromptModel>> GeneratePrompt(int jobApplicationId, string promptName) =>
            TryExecuteAsync(async () =>
            {
                var query = new GeneratePromptQuery() { JobApplicationId = jobApplicationId, PromptName = promptName };
                var generatedPromptDto = await _client.GeneratePromptAsync(query);
                return new GeneratedPromptModel()
                {
                    Text = generatedPromptDto.Prompt,
                    WindowTitle = "Generated prompt",
                    UnusedKeys = generatedPromptDto.UnusedKeys.ToList()
                };
            });

        public Task<Response<List<string>>> GetPromptNames() =>
            TryExecuteAsync(async () =>
            {
                var dto = await _client.GetPromptNamesAsync();
                return dto.Names.ToList();
            });
    }
}
```

---

### Task 13: Build — expect PASS

- [ ] **Build:**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: 0 errors, 0 warnings. The WPF project calls `GeneratePrompt(jobApplicationId)` — this signature changed. **Check for compile errors in `AppTrack.Wpf`.**

> **Note:** The spec explicitly excludes WPF changes. However, since the service interface changed, the WPF `ApplicationTextService` call will break at compile time. The WPF UI currently passes only `jobApplicationId` — update that one call to pass a hardcoded empty string or a first-prompt strategy is out of scope. Since this is out of scope, the simplest fix is to **add an overload** that preserves backward compatibility:
>
> Add to `IApplicationTextService`:
> ```csharp
> // Backward-compatible overload used by WPF — passes empty string, selects first prompt via backend
> Task<Response<GeneratedPromptModel>> GeneratePrompt(int jobApplicationId);
> ```
> And in `ApplicationTextService`:
> ```csharp
> public Task<Response<GeneratedPromptModel>> GeneratePrompt(int jobApplicationId) =>
>     GeneratePrompt(jobApplicationId, string.Empty);
> ```
> **Important:** This overload passes `PromptName = ""` which will now fail validation. Check whether the WPF app still needs to work. If WPF is fully excluded and its build is acceptable to break, skip the overload and suppress or exclude the WPF project from the build check. Confirm with the user before proceeding.

- [ ] **Commit:**

```bash
git add ApiService/Base/ServiceClient.cs \
        ApiService/Base/clientsettings.nswag \
        ApiService/Contracts/IApplicationTextService.cs \
        ApiService/Services/ApplicationTextService.cs
git commit -m "feat: regenerate NSwag client — add GetPromptNamesAsync and PromptName to GeneratePromptQuery"
```

---

## Chunk 4: Blazor UI

**Files:**
- Modify: `AppTrack.BlazorUi/Components/Pages/Home.razor` (line 159)
- Modify: `AppTrack.BlazorUi/Components/Pages/Home.razor.cs`
- Modify: `AppTrack.BlazorUi/Components/Dialogs/GenerateTextDialog.razor.cs`
- Modify: `AppTrack.BlazorUi/Components/Dialogs/GenerateTextDialog.razor`

---

### Task 14: Rename tooltip and update Home.razor.cs

- [ ] **Modify `AppTrack.BlazorUi/Components/Pages/Home.razor`** — change tooltip text on line 159:

```razor
<MudTooltip Text="Send Prompt">
```

(Only this one string changes. No other markup changes.)

- [ ] **Modify `AppTrack.BlazorUi/Components/Pages/Home.razor.cs`** — add `IApplicationTextService` injection and update `GenerateTextAsync`:

```csharp
using AppTrack.BlazorUi.Components.Dialogs;
using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Pages;

public partial class Home
{
    [Inject] private IJobApplicationService JobApplicationService { get; set; } = null!;
    [Inject] private IApplicationTextService ApplicationTextService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IErrorHandlingService ErrorHandlingService { get; set; } = null!;

    private static readonly DialogOptions _dialogOptions = new()
    {
        BackdropClick = false,
        MaxWidth = MaxWidth.Medium,
        FullWidth = true,
    };

    private static readonly DialogOptions _generateTextDialogOptions = new()
    {
        BackdropClick = false,
        MaxWidth = MaxWidth.Large,
        FullWidth = true,
    };

    private MudMessageBox? _deleteConfirmBox;
    private string _deleteConfirmMessage = string.Empty;

    private List<JobApplicationModel> _jobApplications = [];
    private JobApplicationModel.JobApplicationStatus? _selectedStatus;
    private string _searchText = string.Empty;
    private bool _isLoading;

    private IEnumerable<JobApplicationModel> _filteredJobApplications =>
        _jobApplications
            .Where(x => _selectedStatus is not { } s || x.Status == s)
            .Where(x => string.IsNullOrWhiteSpace(_searchText)
                        || x.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
                        || x.Position.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
                        || x.Location.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

    private int _filteredCount => _filteredJobApplications.Count();

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated == true)
            await LoadJobApplicationsAsync();
    }

    private async Task LoadJobApplicationsAsync()
    {
        _isLoading = true;
        try
        {
            var response = await JobApplicationService.GetJobApplicationsForUserAsync();
            _jobApplications = response.Success ? response.Data ?? [] : [];
        }
        finally
        {
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void OnStatusFilterChanged(JobApplicationModel.JobApplicationStatus? status) =>
        _selectedStatus = status;

    private void OnSearchChanged(string value) =>
        _searchText = value ?? string.Empty;

    private async Task CreateJobApplicationAsync()
    {
        var dialog = await DialogService.ShowAsync<CreateJobApplicationDialog>("", _dialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not JobApplicationModel newModel) return;

        _jobApplications.Add(newModel);
        await InvokeAsync(StateHasChanged);
    }

    private async Task EditJobApplicationAsync(JobApplicationModel model)
    {
        var parameters = new DialogParameters<EditJobApplicationDialog>
        {
            { x => x.JobApplication, model }
        };

        var dialog = await DialogService.ShowAsync<EditJobApplicationDialog>("", parameters, _dialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not JobApplicationModel updatedModel) return;

        var index = _jobApplications.IndexOf(model);
        if (index >= 0)
            _jobApplications[index] = updatedModel;

        await InvokeAsync(StateHasChanged);
    }

    private async Task GenerateTextAsync(JobApplicationModel model)
    {
        var namesResponse = await ApplicationTextService.GetPromptNames();

        if (!ErrorHandlingService.HandleResponse(namesResponse))
            return;

        if (namesResponse.Data is not { Count: > 0 })
        {
            ErrorHandlingService.ShowError("No prompt configured");
            return;
        }

        var parameters = new DialogParameters<GenerateTextDialog>
        {
            { x => x.JobApplication, model },
            { x => x.PromptNames, namesResponse.Data }
        };

        var dialog = await DialogService.ShowAsync<GenerateTextDialog>("", parameters, _generateTextDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not string generatedText) return;

        model.ApplicationText = generatedText;
        await InvokeAsync(StateHasChanged);
    }

    private async Task DeleteJobApplicationAsync(JobApplicationModel model)
    {
        _deleteConfirmMessage = $"Are you sure you want to delete '{model.Name}'?";

        var confirmed = await _deleteConfirmBox!.ShowAsync();
        if (confirmed != true)
            return;

        var response = await JobApplicationService.DeleteJobApplicationAsync(model.Id);

        if (!ErrorHandlingService.HandleResponse(response)) return;
        if (response.Data is null) return;

        _jobApplications.Remove(model);
        await InvokeAsync(StateHasChanged);
    }

    internal static Color GetStatusColor(JobApplicationModel.JobApplicationStatus status) => status switch
    {
        JobApplicationModel.JobApplicationStatus.New => Color.Primary,
        JobApplicationModel.JobApplicationStatus.WaitingForFeedback => Color.Warning,
        JobApplicationModel.JobApplicationStatus.Rejected => Color.Default,
        _ => Color.Default
    };

    internal static string GetStatusHexColor(JobApplicationModel.JobApplicationStatus status) => status switch
    {
        JobApplicationModel.JobApplicationStatus.New => "#0078D4",
        JobApplicationModel.JobApplicationStatus.WaitingForFeedback => "#EF6C00",
        JobApplicationModel.JobApplicationStatus.Rejected => "#546E7A",
        _ => "#9E9E9E"
    };

    internal static string GetStatusLabel(JobApplicationModel.JobApplicationStatus status) => status switch
    {
        JobApplicationModel.JobApplicationStatus.New => "New",
        JobApplicationModel.JobApplicationStatus.WaitingForFeedback => "Waiting",
        JobApplicationModel.JobApplicationStatus.Rejected => "Rejected",
        _ => status.ToString()
    };
}
```

---

### Task 15: Update GenerateTextDialog

- [ ] **Modify `AppTrack.BlazorUi/Components/Dialogs/GenerateTextDialog.razor.cs`:**

```csharp
using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class GenerateTextDialog : IDisposable
{
    [Inject] private IApplicationTextService ApplicationTextService { get; set; } = null!;
    [Inject] private IErrorHandlingService ErrorHandlingService { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public JobApplicationModel JobApplication { get; set; } = null!;
    [Parameter] public List<string> PromptNames { get; set; } = [];

    private enum Phase { LoadingPrompt, PromptReady, GeneratingText, TextReady }

    private Phase _phase = Phase.LoadingPrompt;
    private string _prompt = string.Empty;
    private string _generatedText = string.Empty;
    private List<string> _unusedKeys = [];
    private CancellationTokenSource _cts = new();
    private string _selectedPromptName = string.Empty;
    private bool _isReloadingPrompt;

    protected override async Task OnInitializedAsync()
    {
        _selectedPromptName = PromptNames.First();
        var response = await ApplicationTextService.GeneratePrompt(JobApplication.Id, _selectedPromptName);

        if (!ErrorHandlingService.HandleResponse(response) || response.Data is null)
        {
            MudDialog.Cancel();
            return;
        }

        _prompt = response.Data.Text;
        _unusedKeys = response.Data.UnusedKeys;
        _phase = Phase.PromptReady;
    }

    private async Task OnPromptNameChangedAsync(string newName)
    {
        _selectedPromptName = newName;
        _isReloadingPrompt = true;
        StateHasChanged();

        var response = await ApplicationTextService.GeneratePrompt(JobApplication.Id, newName);
        _isReloadingPrompt = false;

        if (!ErrorHandlingService.HandleResponse(response) || response.Data is null)
            return;

        _prompt = response.Data.Text;
        _unusedKeys = response.Data.UnusedKeys;
    }

    private async Task SendPromptAsync()
    {
        _phase = Phase.GeneratingText;
        _cts = new CancellationTokenSource();

        var response = await ApplicationTextService.GenerateApplicationText(_prompt, JobApplication.Id, _cts.Token);

        if (_cts.IsCancellationRequested)
        {
            _phase = Phase.PromptReady;
            return;
        }

        if (!ErrorHandlingService.HandleResponse(response) || response.Data is null)
        {
            _phase = Phase.PromptReady;
            return;
        }

        _generatedText = response.Data.Text;
        _phase = Phase.TextReady;
    }

    private void AbortGeneration() => _cts.Cancel();

    private async Task CopyTextAsync()
    {
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", _generatedText);
        ErrorHandlingService.ShowSuccess("Copied to clipboard.");
    }

    private async Task CopyAndCloseAsync()
    {
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", _generatedText);
        MudDialog.Close(DialogResult.Ok(_generatedText));
    }

    private void Cancel() => MudDialog.Cancel();

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
```

- [ ] **Modify `AppTrack.BlazorUi/Components/Dialogs/GenerateTextDialog.razor`** — add `MudSelect` in the `PromptReady` phase and `Disabled` attributes:

```razor
<MudDialog Class="apptrack-dialog" ContentClass="generate-text-content">
    <TitleContent>
        <MudText Typo="Typo.h6">
            <MudIcon Icon="@Icons.Material.Filled.AutoFixHigh" Class="mr-2" />
            Generate Application Text
        </MudText>
    </TitleContent>
    <DialogContent>
        @if (_phase == Phase.LoadingPrompt)
        {
            <MudStack AlignItems="AlignItems.Center" Class="py-8" Spacing="2">
                <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
                <MudText Typo="Typo.body2" Color="Color.Secondary">Generating prompt…</MudText>
            </MudStack>
        }
        else if (_phase == Phase.PromptReady)
        {
            <MudSelect T="string"
                       Label="Prompt"
                       Value="_selectedPromptName"
                       ValueChanged="OnPromptNameChangedAsync"
                       Variant="Variant.Outlined"
                       Disabled="@_isReloadingPrompt"
                       Class="mb-3">
                @foreach (var name in PromptNames)
                {
                    <MudSelectItem Value="@name">@name</MudSelectItem>
                }
            </MudSelect>
            @if (_unusedKeys.Count > 0)
            {
                <MudAlert Severity="Severity.Warning" Class="mb-3" Dense="true">
                    Unused placeholders: <strong>@string.Join(", ", _unusedKeys)</strong>
                </MudAlert>
            }
            <MudTextField T="string"
                          Value="_prompt"
                          ValueChanged="@((string v) => _prompt = v)"
                          Label="Prompt"
                          Variant="Variant.Outlined"
                          Lines="14"
                          FullWidth="true"
                          Disabled="@_isReloadingPrompt" />
        }
        else if (_phase == Phase.GeneratingText)
        {
            <MudStack AlignItems="AlignItems.Center" Class="py-8" Spacing="3">
                <MudProgressCircular Color="Color.Primary" Indeterminate="true" />
                <MudText Typo="Typo.body2" Color="Color.Secondary">Generating application text…</MudText>
                <MudButton Variant="Variant.Text"
                           Color="Color.Warning"
                           StartIcon="@Icons.Material.Filled.Stop"
                           OnClick="AbortGeneration">
                    Abort
                </MudButton>
            </MudStack>
        }
        else if (_phase == Phase.TextReady)
        {
            <MudStack Row="true" AlignItems="AlignItems.Center" Class="mb-2">
                <MudText Typo="Typo.subtitle2" Color="Color.Secondary">Generated text</MudText>
                <MudSpacer />
                <MudTooltip Text="Copy to clipboard">
                    <MudIconButton Icon="@Icons.Material.Outlined.ContentCopy"
                                   Size="Size.Small"
                                   Color="Color.Default"
                                   OnClick="CopyTextAsync" />
                </MudTooltip>
            </MudStack>
            <MudTextField T="string"
                          Value="_generatedText"
                          ReadOnly="true"
                          Variant="Variant.Outlined"
                          Lines="14"
                          FullWidth="true" />
        }
    </DialogContent>
    <DialogActions>
        <MudButton Variant="Variant.Text"
                   OnClick="Cancel"
                   Disabled="_phase is Phase.LoadingPrompt or Phase.GeneratingText">
            @(_phase == Phase.TextReady ? "Close" : "Cancel")
        </MudButton>
        <MudSpacer />
        @if (_phase == Phase.PromptReady)
        {
            <MudButton Variant="Variant.Filled"
                       Color="Color.Primary"
                       StartIcon="@Icons.Material.Filled.Send"
                       Disabled="@_isReloadingPrompt"
                       OnClick="SendPromptAsync">
                Send Prompt
            </MudButton>
        }
        else if (_phase == Phase.TextReady)
        {
            <MudButton Variant="Variant.Filled"
                       Color="Color.Primary"
                       StartIcon="@Icons.Material.Outlined.ContentCopy"
                       OnClick="CopyAndCloseAsync">
                Copy and Close
            </MudButton>
        }
    </DialogActions>
</MudDialog>
```

---

### Task 16: Build and commit

- [ ] **Build:**

```bash
dotnet build AppTrack.sln --configuration Release
```

Expected: 0 errors, 0 warnings.

- [ ] **Run all unit tests:**

```bash
dotnet test test/AppTrack.Application.UnitTests/AppTrack.Application.UnitTests.csproj --configuration Release
```

Expected: all tests pass.

- [ ] **Commit:**

```bash
git add AppTrack.BlazorUi/Components/Pages/Home.razor \
        AppTrack.BlazorUi/Components/Pages/Home.razor.cs \
        AppTrack.BlazorUi/Components/Dialogs/GenerateTextDialog.razor \
        AppTrack.BlazorUi/Components/Dialogs/GenerateTextDialog.razor.cs
git commit -m "feat: add prompt selection dropdown to GenerateTextDialog, rename trigger to Send Prompt"
```
