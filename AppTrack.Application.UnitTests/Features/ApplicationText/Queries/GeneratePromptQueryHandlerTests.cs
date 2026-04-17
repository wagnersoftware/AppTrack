// test/AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryHandlerTests.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;
using AppTrack.Domain;
using AppTrack.Domain.Contracts;
using AppTrack.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.ApplicationText.Queries;

public class GeneratePromptQueryHandlerTests
{
    private const string UserId = "user-1";
    private const int JobApplicationId = 5;
    private const string PromptKey = "Default";

    private readonly Mock<IAiSettingsRepository> _mockAiSettingsRepo;
    private readonly Mock<IJobApplicationRepository> _mockJobApplicationRepo;
    private readonly Mock<IPromptBuilder> _mockPromptBuilder;
    private readonly Mock<IBuiltInPromptRepository> _mockBuiltInPromptRepo;
    private readonly Mock<IValidator<GeneratePromptQuery>> _mockValidator;

    public GeneratePromptQueryHandlerTests()
    {
        _mockAiSettingsRepo = new Mock<IAiSettingsRepository>();
        _mockJobApplicationRepo = new Mock<IJobApplicationRepository>();
        _mockPromptBuilder = new Mock<IPromptBuilder>();
        _mockBuiltInPromptRepo = new Mock<IBuiltInPromptRepository>();
        _mockValidator = new Mock<IValidator<GeneratePromptQuery>>();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GeneratePromptQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

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
            Prompts = new List<Prompt> { Prompt.Create(PromptKey,"Hello {{Name}}") },
            PromptParameter = new List<PromptParameter>()
        };

        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(JobApplicationId))
            .ReturnsAsync(existingJobApplication);
        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != JobApplicationId)))
            .ReturnsAsync((JobApplication?)null);

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync(existingAiSettings);

        // Default: no built-in prompts
        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>());

        _mockPromptBuilder
            .Setup(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), It.IsAny<string>()))
            .Returns(("Hello Test Company", new List<string>()));
    }

    private GeneratePromptQueryHandler CreateHandler() =>
        new(_mockAiSettingsRepo.Object, _mockJobApplicationRepo.Object, _mockPromptBuilder.Object, _mockBuiltInPromptRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldReturnGeneratedPromptDto_WhenQueryIsValid()
    {
        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptKey = PromptKey };

        var result = await CreateHandler().Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<GeneratedPromptDto>();
        result.Prompt.ShouldBe("Hello Test Company");
        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), "Hello {{Name}}"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldBuildPromptFromNamedTemplate()
    {
        const string secondPromptKey = "LinkedIn";
        const string secondTemplate = "LinkedIn template for {{Name}}";

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync(new DomainAiSettings
            {
                Id = 1,
                UserId = UserId,
                Prompts = new List<Prompt>
                {
                    Prompt.Create(PromptKey, "Hello {{Name}}"),
                    Prompt.Create(secondPromptKey, secondTemplate)
                },
                PromptParameter = new List<PromptParameter>()
            });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptKey = secondPromptKey };
        await CreateHandler().Handle(query, CancellationToken.None);

        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), secondTemplate), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationDoesNotExist()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GeneratePromptQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("JobApplicationId", "Job application doesn't exist")]));

        var query = new GeneratePromptQuery { JobApplicationId = 9999, UserId = UserId, PromptKey = PromptKey };
        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationBelongsToAnotherUser()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GeneratePromptQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("JobApplicationId", "Job application doesn't exist")]));

        const string otherUserId = "user-other";
        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(JobApplicationId))
            .ReturnsAsync(new JobApplication { Id = JobApplicationId, UserId = otherUserId });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptKey = PromptKey };
        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAiSettingsDoNotExistForUser()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GeneratePromptQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("UserId", "AI settings not found for this user.")]));

        const string noSettingsUser = "user-no-settings";

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(noSettingsUser))
            .ReturnsAsync((DomainAiSettings?)null);

        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(JobApplicationId))
            .ReturnsAsync(new JobApplication { Id = JobApplicationId, UserId = noSettingsUser });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = noSettingsUser, PromptKey = PromptKey };
        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldBuildPromptFromBuiltInRepository_WhenPromptKeyHasBuiltInPrefix()
    {
        const string builtInPromptKey = "builtIn_Cover_Letter";
        const string builtInTemplate = "Write a cover letter for {{Position}}.";

        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>
            {
                BuiltInPrompt.Create(builtInPromptKey, builtInTemplate),
            });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptKey = builtInPromptKey };
        await CreateHandler().Handle(query, CancellationToken.None);

        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), builtInTemplate), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseBuiltInTemplate_NotUserTemplate_WhenPromptKeyHasBuiltInPrefix()
    {
        const string builtInPromptKey = "builtIn_Cover_Letter";
        const string userTemplate = "User's own template";
        const string builtInTemplate = "Write a cover letter for {{Position}}.";

        // User also has a prompt with the same name — must be ignored
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync(new DomainAiSettings
            {
                Id = 1,
                UserId = UserId,
                Prompts = new List<Prompt> { Prompt.Create("builtIn_Cover_Letter", userTemplate) },
                PromptParameter = new List<PromptParameter>()
            });

        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>
            {
                BuiltInPrompt.Create(builtInPromptKey, builtInTemplate),
            });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptKey = builtInPromptKey };
        await CreateHandler().Handle(query, CancellationToken.None);

        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), builtInTemplate), Times.Once);
        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), userTemplate), Times.Never);
    }
}
