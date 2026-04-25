using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Commands.SetPortalSubscriptions;
using AppTrack.Application.Features.ProjectMonitoring.Dto;
using AppTrack.Application.Shared;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

public class SetPortalSubscriptionsCommandHandlerTests
{
    private readonly Mock<IUserPortalSubscriptionRepository> _mockRepo;
    private readonly Mock<IValidator<SetPortalSubscriptionsCommand>> _mockValidator;
    private readonly Mock<IUnitOfWork> _mockUow;

    public SetPortalSubscriptionsCommandHandlerTests()
    {
        _mockRepo = new Mock<IUserPortalSubscriptionRepository>();
        _mockValidator = new Mock<IValidator<SetPortalSubscriptionsCommand>>();
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<SetPortalSubscriptionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _mockUow = new Mock<IUnitOfWork>();
        _mockUow
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(async (action, ct) => await action(ct));
    }

    private SetPortalSubscriptionsCommandHandler CreateHandler() =>
        new(_mockRepo.Object, _mockValidator.Object, _mockUow.Object);

    [Fact]
    public async Task Handle_ShouldCallUpsert_ForEachSubscription()
    {
        var command = new SetPortalSubscriptionsCommand
        {
            UserId = "user-1",
            Subscriptions = [new(1, true), new(2, false)]
        };

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockRepo.Verify(r => r.UpsertAsync("user-1", 1, true), Times.Once);
        _mockRepo.Verify(r => r.UpsertAsync("user-1", 2, false), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnit()
    {
        var command = new SetPortalSubscriptionsCommand
        {
            UserId = "user-1",
            Subscriptions = [new(1, true)]
        };

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.ShouldBe(Unit.Value);
    }

    [Fact]
    public async Task Handle_ShouldUseTransaction_ForAllSubscriptions()
    {
        var command = new SetPortalSubscriptionsCommand
        {
            UserId = "user-1",
            Subscriptions = [new(1, true), new(2, false)]
        };

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockUow.Verify(u => u.ExecuteInTransactionAsync(
            It.IsAny<Func<CancellationToken, Task>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
