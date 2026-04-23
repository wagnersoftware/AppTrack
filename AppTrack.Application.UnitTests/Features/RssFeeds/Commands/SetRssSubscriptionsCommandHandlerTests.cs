using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Commands.SetRssSubscriptions;
using AppTrack.Application.Features.RssFeeds.Dto;
using AppTrack.Application.Shared;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Commands;

public class SetRssSubscriptionsCommandHandlerTests
{
    private readonly Mock<IUserRssSubscriptionRepository> _mockRepo;
    private readonly Mock<IValidator<SetRssSubscriptionsCommand>> _mockValidator;
    private readonly Mock<IUnitOfWork> _mockUow;

    public SetRssSubscriptionsCommandHandlerTests()
    {
        _mockRepo = new Mock<IUserRssSubscriptionRepository>();
        _mockValidator = new Mock<IValidator<SetRssSubscriptionsCommand>>();
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<SetRssSubscriptionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _mockUow = new Mock<IUnitOfWork>();
        _mockUow
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(async (action, ct) => await action(ct));
    }

    private SetRssSubscriptionsCommandHandler CreateHandler() =>
        new(_mockRepo.Object, _mockValidator.Object, _mockUow.Object);

    [Fact]
    public async Task Handle_ShouldCallUpsert_ForEachSubscription()
    {
        var command = new SetRssSubscriptionsCommand
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
        var command = new SetRssSubscriptionsCommand
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
        var command = new SetRssSubscriptionsCommand
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
