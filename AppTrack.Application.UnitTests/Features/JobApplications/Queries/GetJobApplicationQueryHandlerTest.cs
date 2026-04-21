using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplicationsForUser;
using AppTrack.Application.UnitTests.Mocks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Queries;

public class GetJobApplicationQueryHandlerTest
{
    private readonly Mock<IJobApplicationRepository> _mockRepo;
    private readonly Mock<ILogger<GetJobApplicationsForUserQueryHandler>> _mockLogger;
    private readonly Mock<IValidator<GetJobApplicationsForUserQuery>> _mockValidator;

    public GetJobApplicationQueryHandlerTest()
    {
        _mockRepo = MockJobApplicationRepository.GetJobApplicationRepository();
        _mockLogger = new Mock<ILogger<GetJobApplicationsForUserQueryHandler>>();
        _mockValidator = new Mock<IValidator<GetJobApplicationsForUserQuery>>();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetJobApplicationsForUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task GetJobApplicationListTest()
    {
        var handler = new GetJobApplicationsForUserQueryHandler(_mockRepo.Object, _mockLogger.Object, _mockValidator.Object);

        var result = await handler.Handle(new GetJobApplicationsForUserQuery() { UserId = "TestUser1" }, CancellationToken.None);

        result.ShouldBeOfType<List<JobApplicationDto>>();
        result.Count.ShouldBe(2);
    }
}
