using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Features.JobApplications.Queries.GetJobApplicationById;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Queries;

public class GetJobApplicationByIdQueryHandlerTests
{
    private const string OwnerId = "owner-user";
    private const string OtherId = "other-user";
    private const int ExistingId = 7;

    private readonly Mock<IJobApplicationRepository> _mockRepo;
    private readonly Mock<IValidator<GetJobApplicationByIdQuery>> _mockValidator;

    public GetJobApplicationByIdQueryHandlerTests()
    {
        _mockRepo = new Mock<IJobApplicationRepository>();
        _mockValidator = new Mock<IValidator<GetJobApplicationByIdQuery>>();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetJobApplicationByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

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

        _mockRepo
            .Setup(r => r.GetByIdWithAiTextHistoryAsync(ExistingId))
            .ReturnsAsync(existingEntity);

        _mockRepo
            .Setup(r => r.GetByIdWithAiTextHistoryAsync(It.Is<int>(id => id != ExistingId)))
            .ReturnsAsync((JobApplication?)null);
    }

    private GetJobApplicationByIdQueryHandler CreateHandler() =>
        new(_mockRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldReturnJobApplicationDto_WhenEntityExistsAndBelongsToUser()
    {
        var query = new GetJobApplicationByIdQuery { Id = ExistingId, UserId = OwnerId };

        var result = await CreateHandler().Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<JobApplicationDto>();
        result.Id.ShouldBe(ExistingId);
        result.UserId.ShouldBe(OwnerId);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenIdIsZero()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetJobApplicationByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "Id is required")]));

        var query = new GetJobApplicationByIdQuery { Id = 0, UserId = OwnerId };

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationDoesNotExist()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetJobApplicationByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "Job application not found.")]));

        var query = new GetJobApplicationByIdQuery { Id = 9999, UserId = OwnerId };

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationBelongsToAnotherUser()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetJobApplicationByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("UserId", "Job application doesn't belong to this user.")]));

        var query = new GetJobApplicationByIdQuery { Id = ExistingId, UserId = OtherId };

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(query, CancellationToken.None));
    }
}
