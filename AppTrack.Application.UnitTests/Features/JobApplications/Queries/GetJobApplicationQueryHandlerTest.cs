using AppTrack.Application.Contracts.Logging;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplications;
using AppTrack.Application.MappingProfiles;
using AppTrack.Application.UnitTests.Mocks;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Queries;

public class GetJobApplicationQueryHandlerTest
{
    private readonly Mock<IJobApplicationRepository> _mockRepo;
    private IMapper _mapper;
    private Mock<IAppLogger<GetJobApplicationsQueryHandler>> _mockAppLogger;

    public GetJobApplicationQueryHandlerTest()
    {
        _mockRepo = MockJobApplicationRepository.GetJobApplicationRepository();
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<JobApplicationProfile>();
        }, NullLoggerFactory.Instance); 

        _mapper = mapperConfig.CreateMapper();
        _mockAppLogger = new Mock<IAppLogger<GetJobApplicationsQueryHandler>>();
    }

    [Fact]
    public async Task GetJobApplicationListTest()
    {
        var handler = new GetJobApplicationsQueryHandler(_mapper, _mockRepo.Object, _mockAppLogger.Object);

        var result = await handler.Handle(new GetJobApplicationsQuery(), CancellationToken.None);

        result.ShouldBeOfType<List<JobApplicationDto>>();
        result.Count().ShouldBe(2);
    }
}
