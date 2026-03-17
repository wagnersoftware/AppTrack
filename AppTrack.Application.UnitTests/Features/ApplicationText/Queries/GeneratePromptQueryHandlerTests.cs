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
            Prompts = new List<AppTrack.Domain.Prompt> { AppTrack.Domain.Prompt.Create("Default", "Hello {Name}") },
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

    [Fact]
    public async Task Handle_ShouldReturnGeneratedPromptDto_WhenQueryIsValid()
    {
        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = UserId };
        var handler = new GeneratePromptQueryHandler(_mockAiSettingsRepo.Object, _mockJobApplicationRepo.Object, _mockPromptBuilder.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<GeneratedPromptDto>();
        result.Prompt.ShouldBe("Hello Test Company");
        _mockPromptBuilder.Verify(b => b.BuildPrompt(It.IsAny<IEnumerable<PromptParameter>>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationDoesNotExist()
    {
        var query = new GeneratePromptQuery { JobApplicationId = 9999, UserId = UserId };
        var handler = new GeneratePromptQueryHandler(_mockAiSettingsRepo.Object, _mockJobApplicationRepo.Object, _mockPromptBuilder.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(query, CancellationToken.None));
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

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = noSettingsUser };
        var handler = new GeneratePromptQueryHandler(_mockAiSettingsRepo.Object, _mockJobApplicationRepo.Object, _mockPromptBuilder.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenNoPromptsConfigured()
    {
        const string emptyTemplateUser = "user-empty-template";

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(emptyTemplateUser))
            .ReturnsAsync(new DomainAiSettings
            {
                Id = 2,
                UserId = emptyTemplateUser,
                Prompts = new List<AppTrack.Domain.Prompt>(),
                PromptParameter = new List<PromptParameter>()
            });

        _mockJobApplicationRepo
            .Setup(r => r.GetByIdAsync(JobApplicationId))
            .ReturnsAsync(new JobApplication { Id = JobApplicationId, UserId = emptyTemplateUser });

        var query = new GeneratePromptQuery { JobApplicationId = JobApplicationId, UserId = emptyTemplateUser };
        var handler = new GeneratePromptQueryHandler(_mockAiSettingsRepo.Object, _mockJobApplicationRepo.Object, _mockPromptBuilder.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(query, CancellationToken.None));
    }
}
