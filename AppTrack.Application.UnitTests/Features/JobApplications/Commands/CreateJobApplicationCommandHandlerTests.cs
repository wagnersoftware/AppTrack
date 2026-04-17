using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Commands;

public class CreateJobApplicationCommandHandlerTests
{
    private readonly Mock<IJobApplicationRepository> _mockRepo;
    private readonly Mock<IValidator<CreateJobApplicationCommand>> _mockValidator;

    public CreateJobApplicationCommandHandlerTests()
    {
        _mockRepo = new Mock<IJobApplicationRepository>();
        _mockValidator = new Mock<IValidator<CreateJobApplicationCommand>>();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepo
            .Setup(r => r.CreateAsync(It.IsAny<JobApplication>()))
            .Returns(Task.CompletedTask);
    }

    private static CreateJobApplicationCommand BuildValidCommand(string userId = "user-1") => new()
    {
        UserId = userId,
        Name = "Acme Corp",
        Position = "Software Engineer",
        URL = "https://acme.com/jobs/1",
        JobDescription = "Great opportunity",
        Location = "Remote",
        ContactPerson = "Jane Doe",
        StartDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        Status = JobApplicationStatus.New,
        DurationInMonths = "6"
    };

    private CreateJobApplicationCommandHandler CreateHandler() =>
        new(_mockRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldReturnJobApplicationDto_WhenCommandIsValid()
    {
        var command = BuildValidCommand();

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<JobApplicationDto>();
        result.Name.ShouldBe(command.Name);
        result.Position.ShouldBe(command.Position);
        result.UserId.ShouldBe(command.UserId);
    }

    [Fact]
    public async Task Handle_ShouldCallCreateAsync_WhenCommandIsValid()
    {
        var command = BuildValidCommand();

        await CreateHandler().Handle(command, CancellationToken.None);

        _mockRepo.Verify(r => r.CreateAsync(It.IsAny<JobApplication>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenNameIsEmpty()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "Name is required")]));

        var command = BuildValidCommand();
        command.Name = string.Empty;

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenPositionIsEmpty()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Position", "Position is required")]));

        var command = BuildValidCommand();
        command.Position = string.Empty;

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenUrlIsInvalid()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("URL", "URL is not valid")]));

        var command = BuildValidCommand();
        command.URL = "not-a-valid-url";

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenStartDateIsDefault()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("StartDate", "StartDate is required")]));

        var command = BuildValidCommand();
        command.StartDate = default;

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenDurationInMonthsIsNotNumeric()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("DurationInMonths", "DurationInMonths must be numeric")]));

        var command = BuildValidCommand();
        command.DurationInMonths = "abc";

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotCallCreateAsync_WhenValidationFails()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateJobApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "Name is required")]));

        var command = BuildValidCommand();
        command.Name = string.Empty;

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));

        _mockRepo.Verify(r => r.CreateAsync(It.IsAny<JobApplication>()), Times.Never);
    }
}
