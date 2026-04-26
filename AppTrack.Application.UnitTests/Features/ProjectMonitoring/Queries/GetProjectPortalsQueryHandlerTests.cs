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

    public GetProjectPortalsQueryHandlerTests()
    {
        _mockPortalRepo = new Mock<IProjectPortalRepository>();

        _mockPortalRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync([
            new ProjectPortal { Id = 1, Name = "Freelancermap", Url = "https://freelancermap.de", ScraperType = ScraperType.FreelancerMap, IsActive = true }
        ]);
    }

    private GetProjectPortalsQueryHandler CreateHandler() => new(_mockPortalRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnAllActivePortals()
    {
        var result = await CreateHandler().Handle(new GetProjectPortalsQuery(), CancellationToken.None);

        result.ShouldNotBeEmpty();
        result.ShouldContain(p => p.Name == "Freelancermap");
    }

    [Fact]
    public async Task Handle_ShouldMapPortalFieldsCorrectly()
    {
        var result = await CreateHandler().Handle(new GetProjectPortalsQuery(), CancellationToken.None);

        var portal = result.Single();
        portal.Id.ShouldBe(1);
        portal.Name.ShouldBe("Freelancermap");
        portal.Url.ShouldBe("https://freelancermap.de");
    }
}
