using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Features.JobApplications.Queries.GetJobApplicationById;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Queries;

public class GetJobApplicationByIdQueryHandlerTests
{
    private const string OwnerId = "owner-user";
    private const string OtherId = "other-user";
    private const int ExistingId = 7;

    private readonly Mock<IJobApplicationRepository> _mockRepo;

    public GetJobApplicationByIdQueryHandlerTests()
    {
        _mockRepo = new Mock<IJobApplicationRepository>();

        var existingEntity = new JobApplication
        {
            Id = ExistingId,
            UserId = OwnerId,
            Name = "Existing Application",
            Position = "Developer",
            URL = "https://company.com/job",
            JobDescription = "Good job",
            Location = "Remote",
            ContactPerson = "Jane Doe",
            StartDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            Status = JobApplicationStatus.New
        };

        _mockRepo
            .Setup(r => r.GetByIdAsync(ExistingId))
            .ReturnsAsync(existingEntity);

        _mockRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingId)))
            .ReturnsAsync((JobApplication?)null);
    }

    [Fact]
    public async Task Handle_ShouldReturnJobApplicationDto_WhenEntityExistsAndBelongsToUser()
    {
        var query = new GetJobApplicationByIdQuery { Id = ExistingId, UserId = OwnerId };
        var handler = new GetJobApplicationByIdQueryHandler(_mockRepo.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<JobApplicationDto>();
        result.Id.ShouldBe(ExistingId);
        result.UserId.ShouldBe(OwnerId);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenIdIsZero()
    {
        var query = new GetJobApplicationByIdQuery { Id = 0, UserId = OwnerId };
        var handler = new GetJobApplicationByIdQueryHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationDoesNotExist()
    {
        var query = new GetJobApplicationByIdQuery { Id = 9999, UserId = OwnerId };
        var handler = new GetJobApplicationByIdQueryHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationBelongsToAnotherUser()
    {
        var query = new GetJobApplicationByIdQuery { Id = ExistingId, UserId = OtherId };
        var handler = new GetJobApplicationByIdQueryHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(query, CancellationToken.None));
    }
}
