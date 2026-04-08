// test/AppTrack.Application.UnitTests/Features/ApplicationText/Queries/GeneratePromptQueryValidatorTests.cs
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;
using AppTrack.Domain;
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
    private readonly Mock<IDefaultPromptRepository> _defaultPromptRepo;
    private readonly GeneratePromptQueryValidator _validator;

    public GeneratePromptQueryValidatorTests()
    {
        _jobAppRepo = new Mock<IJobApplicationRepository>();
        _aiSettingsRepo = new Mock<IAiSettingsRepository>();
        _defaultPromptRepo = new Mock<IDefaultPromptRepository>();

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

        // Default: no default prompts
        _defaultPromptRepo
            .Setup(r => r.GetByLanguageAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<DefaultPrompt>());

        _validator = new GeneratePromptQueryValidator(_jobAppRepo.Object, _aiSettingsRepo.Object, _defaultPromptRepo.Object);
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
    public async Task Validate_ShouldHaveError_WhenJobApplicationBelongsToAnotherUser()
    {
        const string otherUserId = "user-other";
        _jobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(new DomainJobApplication { Id = ExistingJobApplicationId, UserId = otherUserId });

        var result = await _validator.TestValidateAsync(BuildValidQuery());
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Job application doesn't belong to this user.");
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
    public async Task Validate_ShouldHaveError_WhenCustomPromptNameNotFoundInUserSettings()
    {
        var result = await _validator.TestValidateAsync(BuildValidQuery(promptName: "NonExistentPrompt"));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Prompt not found in AI settings.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenDefaultPrefixedPromptNotFoundInDefaultRepository()
    {
        // Default_ name not present in default repo — user prompts must not be checked as fallback
        var result = await _validator.TestValidateAsync(BuildValidQuery(promptName: "Default_NonExistent"));
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

    [Fact]
    public async Task Validate_ShouldPass_WhenDefaultPrefixedPromptExistsInDefaultRepository()
    {
        const string defaultPromptName = "Default_Cover_Letter";

        _defaultPromptRepo
            .Setup(r => r.GetByLanguageAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<DefaultPrompt>
            {
                DefaultPrompt.Create(defaultPromptName, "Write a cover letter for {Position}.", "de"),
            });

        var result = await _validator.TestValidateAsync(BuildValidQuery(promptName: defaultPromptName));
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenDefaultPromptTemplateIsEmpty()
    {
        const string defaultOnlyPromptName = "Default_Empty";
        _aiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId }); // no user prompts

        _defaultPromptRepo
            .Setup(r => r.GetByLanguageAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<DefaultPrompt>
            {
                DefaultPrompt.Create(defaultOnlyPromptName, " ", "de"), // empty template
            });

        _jobAppRepo
            .Setup(r => r.GetByIdAsync(ExistingJobApplicationId))
            .ReturnsAsync(new DomainJobApplication { Id = ExistingJobApplicationId, UserId = UserId });

        var localValidator = new GeneratePromptQueryValidator(
            _jobAppRepo.Object, _aiSettingsRepo.Object, _defaultPromptRepo.Object);
        var result = await localValidator.TestValidateAsync(BuildValidQuery(promptName: defaultOnlyPromptName));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Prompt template is empty.");
    }
}
