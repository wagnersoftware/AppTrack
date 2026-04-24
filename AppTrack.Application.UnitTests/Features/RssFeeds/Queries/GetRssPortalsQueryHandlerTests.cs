using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Queries.GetRssPortals;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Queries;

public class GetRssPortalsQueryHandlerTests
{
    private readonly Mock<IRssPortalRepository> _mockPortalRepo;
    private readonly Mock<IUserRssSubscriptionRepository> _mockSubRepo;

    public GetRssPortalsQueryHandlerTests()
    {
        _mockPortalRepo = new Mock<IRssPortalRepository>();
        _mockSubRepo = new Mock<IUserRssSubscriptionRepository>();

        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([
            new RssPortal { Id = 1, Name = "Freelancermap", Url = "https://freelancermap.de", ParserType = RssParserType.FreelancerMap, IsActive = true }
        ]);
    }

    private GetRssPortalsQueryHandler CreateHandler() =>
        new(_mockPortalRepo.Object, _mockSubRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnAllActivePortals()
    {
        _mockSubRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync([]);

        var result = await CreateHandler().Handle(
            new GetRssPortalsQuery { UserId = "user-1" }, CancellationToken.None);

        result.ShouldNotBeEmpty();
        result.ShouldContain(p => p.Name == "Freelancermap");
    }

    [Fact]
    public async Task Handle_ShouldSetIsSubscribedTrue_WhenUserHasActiveSubscription()
    {
        _mockSubRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync([
            new UserRssSubscription { RssPortalId = 1, IsActive = true, UserId = "user-1" }
        ]);

        var result = await CreateHandler().Handle(
            new GetRssPortalsQuery { UserId = "user-1" }, CancellationToken.None);

        result.Single(p => p.Id == 1).IsSubscribed.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSetIsSubscribedFalse_WhenNoSubscriptionExists()
    {
        _mockSubRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync([]);

        var result = await CreateHandler().Handle(
            new GetRssPortalsQuery { UserId = "user-1" }, CancellationToken.None);

        result.Single(p => p.Id == 1).IsSubscribed.ShouldBeFalse();
    }
}
