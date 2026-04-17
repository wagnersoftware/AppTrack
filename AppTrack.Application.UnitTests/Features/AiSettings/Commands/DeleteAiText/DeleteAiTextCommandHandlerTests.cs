using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.AiSettings.Commands.DeleteAiText;
using AppTrack.Application.Shared;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;
using DomainJobApplicationAiText = AppTrack.Domain.JobApplicationAiText;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Commands.DeleteAiText;

public class DeleteAiTextCommandHandlerTests
{
    private const string OwnerId = "owner-user";
    private const int ExistingAiTextId = 10;

    private readonly Mock<IJobApplicationAiTextRepository> _mockAiTextRepo;
    private readonly Mock<IValidator<DeleteAiTextCommand>> _mockValidator;

    public DeleteAiTextCommandHandlerTests()
    {
        _mockAiTextRepo = new Mock<IJobApplicationAiTextRepository>();
        _mockValidator = new Mock<IValidator<DeleteAiTextCommand>>();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteAiTextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var existingAiText = new DomainJobApplicationAiText
        {
            Id = ExistingAiTextId,
            JobApplicationId = 1,
            PromptKey = "cover-letter",
            GeneratedText = "Some generated text"
        };

        _mockAiTextRepo
            .Setup(r => r.GetByIdAsync(ExistingAiTextId))
            .ReturnsAsync(existingAiText);

        _mockAiTextRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingAiTextId)))
            .ReturnsAsync((DomainJobApplicationAiText?)null);

        _mockAiTextRepo
            .Setup(r => r.DeleteAsync(It.IsAny<DomainJobApplicationAiText>()))
            .Returns(Task.CompletedTask);
    }

    private DeleteAiTextCommandHandler CreateHandler() =>
        new(_mockAiTextRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldReturnUnitValue_WhenCommandIsValid()
    {
        // Arrange
        var command = new DeleteAiTextCommand { Id = ExistingAiTextId, UserId = OwnerId };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteAsync_WhenCommandIsValid()
    {
        // Arrange
        var command = new DeleteAiTextCommand { Id = ExistingAiTextId, UserId = OwnerId };
        var handler = CreateHandler();

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockAiTextRepo.Verify(r => r.DeleteAsync(It.IsAny<DomainJobApplicationAiText>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenValidationFails()
    {
        // Arrange
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteAiTextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "AI text entry not found")]));

        var command = new DeleteAiTextCommand { Id = 999, UserId = OwnerId };
        var handler = CreateHandler();

        // Act & Assert
        await Should.ThrowAsync<BadRequestException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAiTextBelongsToAnotherUser()
    {
        // Arrange
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteAiTextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "AI text entry does not belong to this user")]));

        var command = new DeleteAiTextCommand { Id = ExistingAiTextId, UserId = "other-user" };
        var handler = CreateHandler();

        // Act & Assert
        await Should.ThrowAsync<BadRequestException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotCallDeleteAsync_WhenValidationFails()
    {
        // Arrange
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteAiTextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "AI text entry not found")]));

        var command = new DeleteAiTextCommand { Id = 999, UserId = OwnerId };
        var handler = CreateHandler();

        // Act
        await Should.ThrowAsync<BadRequestException>(() =>
            handler.Handle(command, CancellationToken.None));

        // Assert
        _mockAiTextRepo.Verify(r => r.DeleteAsync(It.IsAny<DomainJobApplicationAiText>()), Times.Never);
    }
}
