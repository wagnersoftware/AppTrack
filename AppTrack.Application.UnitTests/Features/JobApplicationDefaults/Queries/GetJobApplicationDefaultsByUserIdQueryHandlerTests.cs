using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;
using DomainJobApplicationDefaults = AppTrack.Domain.JobApplicationDefaults;

namespace AppTrack.Application.UnitTests.Features.JobApplicationDefaults.Queries;

public class GetJobApplicationDefaultsByUserIdQueryHandlerTests
{
    private readonly Mock<IJobApplicationDefaultsRepository> _mockRepo;
    private readonly Mock<IValidator<GetJobApplicationDefaultsByUserIdQuery>> _mockValidator;

    public GetJobApplicationDefaultsByUserIdQueryHandlerTests()
    {
        _mockRepo = new Mock<IJobApplicationDefaultsRepository>();
        _mockValidator = new Mock<IValidator<GetJobApplicationDefaultsByUserIdQuery>>();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetJobApplicationDefaultsByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private GetJobApplicationDefaultsByUserIdQueryHandler CreateHandler() =>
        new(_mockRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldReturnExistingDefaults_WhenDefaultsExistForUser()
    {
        const string userId = "user-1";
        var existingDefaults = new DomainJobApplicationDefaults
        {
            Id = 1,
            UserId = userId,
            Name = "My Company",
            Position = "Developer",
            Location = "Remote"
        };

        _mockRepo
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(existingDefaults);

        var query = new GetJobApplicationDefaultsByUserIdQuery { UserId = userId };

        var result = await CreateHandler().Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<JobApplicationDefaultsDto>();
        result.UserId.ShouldBe(userId);
        result.Id.ShouldBe(1);
        result.Name.ShouldBe("My Company");
        _mockRepo.Verify(r => r.CreateForUserAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateAndReturnNewDefaults_WhenNoDefaultsExistForUser()
    {
        const string userId = "new-user";
        var newDefaults = new DomainJobApplicationDefaults
        {
            Id = 2,
            UserId = userId,
            Name = string.Empty,
            Position = string.Empty,
            Location = string.Empty
        };

        _mockRepo
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((DomainJobApplicationDefaults?)null);

        _mockRepo
            .Setup(r => r.CreateForUserAsync(userId))
            .ReturnsAsync(newDefaults);

        var query = new GetJobApplicationDefaultsByUserIdQuery { UserId = userId };

        var result = await CreateHandler().Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<JobApplicationDefaultsDto>();
        result.UserId.ShouldBe(userId);
        _mockRepo.Verify(r => r.CreateForUserAsync(userId), Times.Once);
    }
}
