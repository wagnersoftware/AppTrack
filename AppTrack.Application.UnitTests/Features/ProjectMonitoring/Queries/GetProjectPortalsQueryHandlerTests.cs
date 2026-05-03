using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Queries.GetProjectPortals;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Queries;

public class GetProjectPortalsQueryHandlerTests
{
    private const string UserId = "user-1";

    private readonly Mock<IProjectPortalRepository> _mockPortalRepo;
    private readonly Mock<IUserPortalSubscriptionRepository> _mockSubscriptionRepo;

    public GetProjectPortalsQueryHandlerTests()
    {
        _mockPortalRepo = new Mock<IProjectPortalRepository>();
        _mockSubscriptionRepo = new Mock<IUserPortalSubscriptionRepository>();

        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([
            new ProjectPortal { Id = 1, Name = "Freelancermap", Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true }
        ]);

        _mockSubscriptionRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<string>())).ReturnsAsync([]);
    }

    private GetProjectPortalsQueryHandler CreateHandler() =>
        new(_mockPortalRepo.Object, _mockSubscriptionRepo.Object);

    private static GetProjectPortalsQuery BuildQuery(string userId = UserId) =>
        new() { UserId = userId };

    [Fact]
    public async Task Handle_ShouldReturnAllActivePortals()
    {
        var result = await CreateHandler().Handle(BuildQuery(), CancellationToken.None);

        result.ShouldNotBeEmpty();
        result.ShouldContain(p => p.Name == "Freelancermap");
    }

    [Fact]
    public async Task Handle_ShouldMapPortalFieldsCorrectly()
    {
        var result = await CreateHandler().Handle(BuildQuery(), CancellationToken.None);

        var portal = result.Single();
        portal.Id.ShouldBe(1);
        portal.Name.ShouldBe("Freelancermap");
        portal.Url.ShouldBe("https://freelancermap.de");
    }

    [Fact]
    public async Task Handle_ShouldMarkPortalAsSubscribed_WhenUserHasActiveSubscription()
    {
        _mockSubscriptionRepo.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync([
            new UserPortalSubscription { ProjectPortalId = 1, UserId = UserId, IsActive = true }
        ]);

        var result = await CreateHandler().Handle(BuildQuery(), CancellationToken.None);

        result.Single().IsSubscribed.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ShouldMarkPortalAsNotSubscribed_WhenUserHasNoSubscription()
    {
        _mockSubscriptionRepo.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync([]);

        var result = await CreateHandler().Handle(BuildQuery(), CancellationToken.None);

        result.Single().IsSubscribed.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldMarkPortalAsNotSubscribed_WhenSubscriptionIsInactive()
    {
        _mockSubscriptionRepo.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync([
            new UserPortalSubscription { ProjectPortalId = 1, UserId = UserId, IsActive = false }
        ]);

        var result = await CreateHandler().Handle(BuildQuery(), CancellationToken.None);

        result.Single().IsSubscribed.ShouldBeFalse();
    }
}
