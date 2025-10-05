using AppTrack.Application.Contracts.Logging;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplicationsForUser;
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
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<GetJobApplicationsForUserQueryHandler>> _mockAppLogger;

    public GetJobApplicationQueryHandlerTest()
    {
        _mockRepo = MockJobApplicationRepository.GetJobApplicationRepository();
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<JobApplicationProfile>();
        }, NullLoggerFactory.Instance);

        _mapper = mapperConfig.CreateMapper();
        _mockAppLogger = new Mock<IAppLogger<GetJobApplicationsForUserQueryHandler>>();
    }

    [Fact]
    public async Task GetJobApplicationListTest()
    {
        var handler = new GetJobApplicationsForUserQueryHandler(_mapper, _mockRepo.Object, _mockAppLogger.Object);

        var result = await handler.Handle(new GetJobApplicationsForUserQuery(), CancellationToken.None);

        result.ShouldBeOfType<List<JobApplicationDto>>();
        result.Count.ShouldBe(2);
    }
}
