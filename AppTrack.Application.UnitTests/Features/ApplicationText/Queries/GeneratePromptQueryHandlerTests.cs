// test/AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryHandlerTests.cs
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
    private readonly Mock<IBuiltInPromptRepository> _mockBuiltInPromptRepo;

    public GeneratePromptQueryHandlerTests()
    {
        _mockAiSettingsRepo = new Mock<IAiSettingsRepository>();
        _mockJobApplicationRepo = new Mock<IJobApplicationRepository>();
        _mockPromptBuilder = new Mock<IPromptBuilder>();
        _mockBuiltInPromptRepo = new Mock<IBuiltInPromptRepository>();

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
            Prompts = new List<Prompt> { Prompt.Create(PromptName, "Hello {Name}") },
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

        // Default: no built-in prompts
        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>());

        _mockPromptBuilder
            .Setup(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), It.IsAny<string>()))
            .Returns(("Hello Test Company", new List<string>()));
    }

    private GeneratePromptQueryHandler CreateHandler() =>
        new(_mockAiSettingsRepo.Object, _mockJobApplicationRepo.Object, _mockPromptBuilder.Object, _mockBuiltInPromptRepo.Object);

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
        const string secondPromptName = "LinkedIn";
        const string secondTemplate = "LinkedIn template for {Name}";

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings
            {
                Id = 1,
                UserId = UserId,
                Prompts = new List<Prompt>
                {
                    Prompt.Create(PromptName, "Hello {Name}"),
                    Prompt.Create(secondPromptName, secondTemplate)
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
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationBelongsToAnotherUser()
    {
        const string otherUserId = "user-other";
        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(JobApplicationId))
            .ReturnsAsync(new JobApplication { Id = JobApplicationId, UserId = otherUserId });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptName = PromptName };
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

    [Fact]
    public async Task Handle_ShouldBuildPromptFromBuiltInRepository_WhenPromptNameHasDefaultPrefix()
    {
        const string builtInPromptName = "Default_Cover_Letter";
        const string builtInTemplate = "Write a cover letter for {Position}.";

        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>
            {
                BuiltInPrompt.Create(builtInPromptName, builtInTemplate),
            });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptName = builtInPromptName };
        await CreateHandler().Handle(query, CancellationToken.None);

        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), builtInTemplate), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseBuiltInTemplate_NotUserTemplate_WhenPromptNameHasDefaultPrefix()
    {
        const string builtInPromptName = "Default_Cover_Letter";
        const string userTemplate = "User's own template";
        const string builtInTemplate = "Write a cover letter for {Position}.";

        // User also has a prompt with the same name — must be ignored
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings
            {
                Id = 1,
                UserId = UserId,
                Prompts = new List<Prompt> { Prompt.Create("Default_Cover_Letter", userTemplate) },
                PromptParameter = new List<PromptParameter>()
            });

        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>
            {
                BuiltInPrompt.Create(builtInPromptName, builtInTemplate),
            });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId, PromptName = builtInPromptName };
        await CreateHandler().Handle(query, CancellationToken.None);

        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), builtInTemplate), Times.Once);
        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), userTemplate), Times.Never);
    }
}
