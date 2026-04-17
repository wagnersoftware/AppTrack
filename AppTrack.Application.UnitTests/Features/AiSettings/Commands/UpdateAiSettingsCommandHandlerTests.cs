using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.UnitTests.Mocks;
using AppTrack.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Commands;

public class UpdateAiSettingsCommandHandlerTests
{
    private const string OwnerId = "user1";
    private const string OtherId = "other-user";
    private const int ExistingId = 1;

    private readonly Mock<IAiSettingsRepository> _mockRepo;
    private readonly Mock<IValidator<UpdateAiSettingsCommand>> _mockValidator;
    private readonly UpdateAiSettingsCommandHandler _handler;

    public UpdateAiSettingsCommandHandlerTests()
    {
        _mockRepo = MockAiSettingsRepository.GetMock();
        _mockValidator = new Mock<IValidator<UpdateAiSettingsCommand>>();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateAiSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = new UpdateAiSettingsCommandHandler(_mockRepo.Object, _mockValidator.Object);
    }

    private static UpdateAiSettingsCommand BuildValidCommand(int id = ExistingId, string userId = OwnerId) => new()
    {
        Id = id,
        UserId = userId,
        SelectedChatModelId = 2,
        Prompts = [new PromptDto { Name = "My_Prompt", PromptTemplate = "Hello {name}" }],
        PromptParameter = [new PromptParameterDto { Key = "Temperature", Value = "0.7" }]
    };

    [Fact]
    public async Task Handle_WithPrompts_ShouldReturnDtoWithPrompts()
    {
        var command = BuildValidCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeOfType<AiSettingsDto>();
        result.Prompts.Count.ShouldBe(1);
        result.Prompts[0].Name.ShouldBe("My_Prompt");
    }

    [Fact]
    public async Task Handle_WithEmptyPrompts_ShouldReturnEmptyPromptsList()
    {
        var command = new UpdateAiSettingsCommand
        {
            Id = ExistingId,
            UserId = OwnerId,
            SelectedChatModelId = 2,
            Prompts = [],
            PromptParameter = []
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Prompts.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnUpdatedDto_WhenCommandIsValid()
    {
        var command = BuildValidCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<AiSettingsDto>();
        result.UserId.ShouldBe(OwnerId);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<DomainAiSettings>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenIdIsZero()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateAiSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "Id is required")]));

        var command = BuildValidCommand(id: 0);

        var ex = await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        ex.ValidationErrors.ShouldContainKey("Id");
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAiSettingsDoesNotExist()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateAiSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "Ai settings not found.")]));

        var command = BuildValidCommand(id: 9999);

        var ex = await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        ex.ValidationErrors.ShouldContainKey("Id");
        ex.ValidationErrors["Id"].ShouldContain("Ai settings not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAiSettingsBelongsToAnotherUser()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateAiSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("UserId", "Ai settings not assigned to this user.")]));

        var command = BuildValidCommand(id: ExistingId, userId: OtherId);

        var ex = await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        ex.ValidationErrors.ShouldContainKey("UserId");
        ex.ValidationErrors["UserId"].ShouldContain("Ai settings not assigned to this user.");
    }

    [Fact]
    public async Task Handle_ShouldNotCallUpdateAsync_WhenValidationFails()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateAiSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "Ai settings not found.")]));

        var command = BuildValidCommand(id: 9999);

        await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<DomainAiSettings>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WithLanguage_WhenLanguageIsGerman()
    {
        var command = new UpdateAiSettingsCommand
        {
            Id = ExistingId,
            UserId = OwnerId,
            SelectedChatModelId = 2,
            Language = AiResponseLanguage.German,
            Prompts = [],
            PromptParameter = []
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Language.ShouldBe(AiResponseLanguage.German);
    }
}
