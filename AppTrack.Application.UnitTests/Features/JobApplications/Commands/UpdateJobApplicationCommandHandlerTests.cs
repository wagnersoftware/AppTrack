using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Commands;

public class UpdateJobApplicationCommandHandlerTests
{
    private const string OwnerId = "owner-user";
    private const string OtherId = "other-user";
    private const int ExistingId = 10;

    private readonly Mock<IJobApplicationRepository> _mockRepo;
    private readonly Mock<IValidator<UpdateJobApplicationCommand>> _mockValidator;

    public UpdateJobApplicationCommandHandlerTests()
    {
        _mockRepo = new Mock<IJobApplicationRepository>();
        _mockValidator = new Mock<IValidator<UpdateJobApplicationCommand>>();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var existingEntity = new JobApplication
        {
            Id = ExistingId,
            UserId = OwnerId,
            Name = "Original Name",
            Position = "Original Position",
            URL = "https://original.com/job",
            JobDescription = "Original description",
            Location = "Berlin",
            ContactPerson = "Max Mustermann",
            StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Status = JobApplicationStatus.New
        };

        _mockRepo
            .Setup(r => r.GetByIdAsync(ExistingId))
            .ReturnsAsync(existingEntity);

        _mockRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingId)))
            .ReturnsAsync((JobApplication?)null);

        _mockRepo
            .Setup(r => r.UpdateAsync(It.IsAny<JobApplication>()))
            .Returns(Task.CompletedTask);
    }

    private static UpdateJobApplicationCommand BuildValidCommand(int id = ExistingId, string userId = OwnerId) => new()
    {
        Id = id,
        UserId = userId,
        Name = "Updated Name",
        Position = "Updated Position",
        URL = "https://updated.com/job",
        JobDescription = "Updated description",
        Location = "Remote",
        ContactPerson = "Jane Doe",
        StartDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        Status = JobApplicationStatus.WaitingForFeedback,
        DurationInMonths = "3"
    };

    private UpdateJobApplicationCommandHandler CreateHandler() =>
        new(_mockRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldReturnUpdatedDto_WhenCommandIsValid()
    {
        var command = BuildValidCommand();

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<JobApplicationDto>();
        result.Name.ShouldBe(command.Name);
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAsync_WhenCommandIsValid()
    {
        var command = BuildValidCommand();

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<JobApplication>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationDoesNotExist()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "Job application doesn't exist for user")]));

        var command = BuildValidCommand(id: 9999);

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenJobApplicationBelongsToAnotherUser()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "Job application doesn't exist for user")]));

        var command = BuildValidCommand(id: ExistingId, userId: OtherId);

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenNameIsEmpty()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "Name is required")]));

        var command = BuildValidCommand();
        command.Name = string.Empty;

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenUrlIsInvalid()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("URL", "URL is not valid")]));

        var command = BuildValidCommand();
        command.URL = "not-a-url";

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotCallUpdateAsync_WhenValidationFails()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "Name is required")]));

        var command = BuildValidCommand();
        command.Name = string.Empty;

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));

        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<JobApplication>()), Times.Never);
    }
}
