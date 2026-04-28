using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ProjectMonitoring.Commands.SetPortalSubscriptions;
using AppTrack.Application.Features.ProjectMonitoring.Dto;
using AppTrack.Application.Shared;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

public class SetPortalSubscriptionsCommandHandlerTests
{
    private readonly Mock<IUserPortalSubscriptionRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly SetPortalSubscriptionsCommandValidator _validator = new();

    public SetPortalSubscriptionsCommandHandlerTests()
    {
        _unitOfWork
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> action, CancellationToken ct) => action(ct));
    }

    private SetPortalSubscriptionsCommandHandler CreateHandler() => new(
        _repository.Object,
        _validator,
        _unitOfWork.Object);

    private static SetPortalSubscriptionsCommand BuildValidCommand() => new()
    {
        UserId = "user-1",
        Subscriptions = new List<PortalSubscriptionItemDto>
        {
            new(PortalId: 1, IsActive: true),
            new(PortalId: 2, IsActive: false)
        }
    };

    [Fact]
    public async Task Handle_ShouldCallUpsertAsyncOncePerSubscription_WhenCommandIsValid()
    {
        // Arrange
        _repository.Setup(r => r.UpsertAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var command = BuildValidCommand();

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        _repository.Verify(r => r.UpsertAsync("user-1", 1, true), Times.Once);
        _repository.Verify(r => r.UpsertAsync("user-1", 2, false), Times.Once);
        _repository.Verify(r => r.UpsertAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldNeverCallUpsert_WhenSubscriptionsIsEmpty()
    {
        // Arrange
        var command = BuildValidCommand();
        command.Subscriptions = new List<PortalSubscriptionItemDto>();

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        _repository.Verify(r => r.UpsertAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenSubscriptionsIsNull()
    {
        // Arrange
        var command = BuildValidCommand();
        command.Subscriptions = null!;

        // Act & Assert
        await Should.ThrowAsync<BadRequestException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));

        _repository.Verify(r => r.UpsertAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnitValue_WhenCommandIsValid()
    {
        // Arrange
        _repository.Setup(r => r.UpsertAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await CreateHandler().Handle(BuildValidCommand(), CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
    }

    [Fact]
    public async Task Handle_ShouldUseTransactionalWrapper_WhenCommandIsValid()
    {
        // Arrange
        _repository.Setup(r => r.UpsertAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        // Act
        await CreateHandler().Handle(BuildValidCommand(), CancellationToken.None);

        // Assert
        _unitOfWork.Verify(
            u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
