using AppTrack.Application.Contracts.Logging;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplicationsForUser;
using AppTrack.Application.UnitTests.Mocks;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Queries;

public class GetJobApplicationQueryHandlerTest
{
    private readonly Mock<IJobApplicationRepository> _mockRepo;
    private readonly Mock<IAppLogger<GetJobApplicationsForUserQueryHandler>> _mockAppLogger;

    public GetJobApplicationQueryHandlerTest()
    {
        _mockRepo = MockJobApplicationRepository.GetJobApplicationRepository();
        _mockAppLogger = new Mock<IAppLogger<GetJobApplicationsForUserQueryHandler>>();
    }

    [Fact]
    public async Task GetJobApplicationListTest()
    {
        var handler = new GetJobApplicationsForUserQueryHandler(_mockRepo.Object, _mockAppLogger.Object);

        var result = await handler.Handle(new GetJobApplicationsForUserQuery() { UserId = "TestUser1" }, CancellationToken.None);

        result.ShouldBeOfType<List<JobApplicationDto>>();
        result.Count.ShouldBe(2);
    }
}
