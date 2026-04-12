using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.Features.FreelancerProfile.Queries.GetFreelancerProfile;
using AppTrack.Application.UnitTests.Mocks;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Queries;

public class GetFreelancerProfileQueryHandlerTests
{
    private readonly Mock<IFreelancerProfileRepository> _mockRepo;
    private readonly GetFreelancerProfileQueryHandler _handler;

    public GetFreelancerProfileQueryHandlerTests()
    {
        _mockRepo = MockFreelancerProfileRepository.GetMock();
        _handler = new GetFreelancerProfileQueryHandler(_mockRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenProfileExists()
    {
        // Arrange
        var query = new GetFreelancerProfileQuery { UserId = MockFreelancerProfileRepository.ExistingUserId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<FreelancerProfileDto>();
        result.UserId.ShouldBe(MockFreelancerProfileRepository.ExistingUserId);
        result.FirstName.ShouldBe("Anna");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProfileDoesNotExist()
    {
        // Arrange
        var query = new GetFreelancerProfileQuery { UserId = "unknown-user" };

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
    }
}
