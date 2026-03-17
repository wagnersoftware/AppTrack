using AppTrack.Application.Contracts.Logging;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Commands.DeleteJobApplication;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Commands;

public class DeleteJobApplicationCommandHandlerTests
{
    private const string OwnerId = "owner-user";
    private const string OtherId = "other-user";
    private const int ExistingId = 42;

    private readonly Mock<IJobApplicationRepository> _mockRepo;
    private readonly Mock<IAppLogger<DeleteJobApplicationCommandHandler>> _mockLogger;

    public DeleteJobApplicationCommandHandlerTests()
    {
        _mockRepo = new Mock<IJobApplicationRepository>();
        _mockLogger = new Mock<IAppLogger<DeleteJobApplicationCommandHandler>>();

        var existingEntity = new JobApplication
        {
            Id = ExistingId,
            UserId = OwnerId,
            Name = "My Application"
        };

        _mockRepo
            .Setup(r => r.GetByIdAsync(ExistingId))
            .ReturnsAsync(existingEntity);

        _mockRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingId)))
            .ReturnsAsync((JobApplication?)null);

        _mockRepo
            .Setup(r => r.DeleteAsync(It.IsAny<JobApplication>()))
            .Returns(Task.CompletedTask);
    }

    private DeleteJobApplicationCommandHandler CreateHandler() =>
        new(_mockRepo.Object, _mockLogger.Object);

    [Fact]
    public async Task Handle_ShouldReturnUnitValue_WhenCommandIsValid()
    {
        // Arrange
        var command = new DeleteJobApplicationCommand { Id = ExistingId, UserId = OwnerId };
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
        var command = new DeleteJobApplicationCommand { Id = ExistingId, UserId = OwnerId };
        var handler = CreateHandler();

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<JobApplication>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationDoesNotExist()
    {
        // Arrange
        var command = new DeleteJobApplicationCommand { Id = 999, UserId = OwnerId };
        var handler = CreateHandler();

        // Act & Assert
        await Should.ThrowAsync<BadRequestException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationBelongsToAnotherUser()
    {
        // Arrange
        var command = new DeleteJobApplicationCommand { Id = ExistingId, UserId = OtherId };
        var handler = CreateHandler();

        // Act & Assert
        await Should.ThrowAsync<BadRequestException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotCallDeleteAsync_WhenValidationFails()
    {
        // Arrange
        var command = new DeleteJobApplicationCommand { Id = ExistingId, UserId = OtherId };
        var handler = CreateHandler();

        // Act
        await Should.ThrowAsync<BadRequestException>(() =>
            handler.Handle(command, CancellationToken.None));

        // Assert
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<JobApplication>()), Times.Never);
    }
}
