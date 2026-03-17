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
        aiSettings.Prompts.Add(DomainPrompt.Create("Default", "Write a cover letter for {position}"));

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
        int jobApplicationId = ExistingJobApplicationId) => new()
    {
        UserId = userId,
        JobApplicationId = jobApplicationId
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
        var query = BuildValidQuery(jobApplicationId: 0);
        var result = await _validator.TestValidateAsync(query);
        result.ShouldHaveValidationErrorFor(x => x.JobApplicationId);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationDoesNotExist()
    {
        var query = BuildValidQuery(jobApplicationId: 9999);
        var result = await _validator.TestValidateAsync(query);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Job application doesn't exist");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenAiSettingsNotFound()
    {
        var query = BuildValidQuery(userId: "unknown-user");
        var result = await _validator.TestValidateAsync(query);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "AI settings not found for this user.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenAiSettingsHasNoPrompts()
    {
        const string userWithNoPrompts = "user-no-prompts";
        var aiSettingsWithNoPrompts = new DomainAiSettings { Id = 2, UserId = userWithNoPrompts };

        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userWithNoPrompts))
            .ReturnsAsync(aiSettingsWithNoPrompts);

        var jobApp = new DomainJobApplication { Id = ExistingJobApplicationId, UserId = userWithNoPrompts };
        _jobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(jobApp);

        var query = BuildValidQuery(userId: userWithNoPrompts);
        var result = await _validator.TestValidateAsync(query);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Prompt in AI settings is missing.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenFirstPromptTemplateIsEmpty()
    {
        const string userWithEmptyTemplate = "user-empty-template";
        var aiSettingsWithEmptyTemplate = new DomainAiSettings { Id = 3, UserId = userWithEmptyTemplate };
        aiSettingsWithEmptyTemplate.Prompts.Add(DomainPrompt.Create("Empty", " "));

        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userWithEmptyTemplate))
            .ReturnsAsync(aiSettingsWithEmptyTemplate);

        var jobApp = new DomainJobApplication { Id = ExistingJobApplicationId, UserId = userWithEmptyTemplate };
        _jobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(jobApp);

        var query = BuildValidQuery(userId: userWithEmptyTemplate);
        var result = await _validator.TestValidateAsync(query);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Prompt in AI settings is missing.");
    }
}
