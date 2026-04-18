using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.ApplicationText.Query.GetPromptKeysQuery;
using FluentValidation.TestHelper;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.ApplicationText.Queries;

public class GetPromptKeysQueryValidatorTests
{
    private const string UserId = "user-1";
    private readonly Mock<IAiSettingsRepository> _mockAiSettingsRepo = new();
    private readonly GetPromptKeysQueryValidator _validator;

    public GetPromptKeysQueryValidatorTests()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId });
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(It.Is<string>(id => id != UserId)))
            .ReturnsAsync((DomainAiSettings?)null);

        _validator = new GetPromptKeysQueryValidator(_mockAiSettingsRepo.Object);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenAiSettingsExist()
    {
        var result = await _validator.TestValidateAsync(new GetPromptKeysQuery { UserId = UserId });
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenAiSettingsNotFound()
    {
        var result = await _validator.TestValidateAsync(new GetPromptKeysQuery { UserId = "unknown-user" });
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "AI settings not found for this user.");
    }
}
