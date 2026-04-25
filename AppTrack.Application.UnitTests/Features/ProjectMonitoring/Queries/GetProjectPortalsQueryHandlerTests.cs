using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Queries.GetProjectPortals;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Queries;

public class GetProjectPortalsQueryHandlerTests
{
    private readonly Mock<IProjectPortalRepository> _mockPortalRepo;
    private readonly Mock<IUserPortalSubscriptionRepository> _mockSubRepo;

    public GetProjectPortalsQueryHandlerTests()
    {
        _mockPortalRepo = new Mock<IProjectPortalRepository>();
        _mockSubRepo = new Mock<IUserPortalSubscriptionRepository>();

        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([
            new ProjectPortal { Id = 1, Name = "Freelancermap", Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true }
        ]);
    }

    private GetProjectPortalsQueryHandler CreateHandler() =>
        new(_mockPortalRepo.Object, _mockSubRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnAllActivePortals()
    {
        _mockSubRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync([]);

        var result = await CreateHandler().Handle(
            new GetProjectPortalsQuery { UserId = "user-1" }, CancellationToken.None);

        result.ShouldNotBeEmpty();
        result.ShouldContain(p => p.Name == "Freelancermap");
    }

    [Fact]
    public async Task Handle_ShouldSetIsSubscribedTrue_WhenUserHasActiveSubscription()
    {
        _mockSubRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync([
            new UserPortalSubscription { ProjectPortalId = 1, IsActive = true, UserId = "user-1" }
        ]);

        var result = await CreateHandler().Handle(
            new GetProjectPortalsQuery { UserId = "user-1" }, CancellationToken.None);

        result.Single(p => p.Id == 1).IsSubscribed.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSetIsSubscribedFalse_WhenNoSubscriptionExists()
    {
        _mockSubRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync([]);

        var result = await CreateHandler().Handle(
            new GetProjectPortalsQuery { UserId = "user-1" }, CancellationToken.None);

        result.Single(p => p.Id == 1).IsSubscribed.ShouldBeFalse();
    }
}
