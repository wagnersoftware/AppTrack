using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Commands;

public class CreateJobApplicationCommandHandlerTests
{
    private readonly Mock<IJobApplicationRepository> _mockRepo;

    public CreateJobApplicationCommandHandlerTests()
    {
        _mockRepo = new Mock<IJobApplicationRepository>();
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

    [Fact]
    public async Task Handle_ShouldReturnJobApplicationDto_WhenCommandIsValid()
    {
        var command = BuildValidCommand();
        var handler = new CreateJobApplicationCommandHandler(_mockRepo.Object);

        var result = await handler.Handle(command, CancellationToken.None);

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
        var handler = new CreateJobApplicationCommandHandler(_mockRepo.Object);

        await handler.Handle(command, CancellationToken.None);

        _mockRepo.Verify(r => r.CreateAsync(It.IsAny<JobApplication>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenNameIsEmpty()
    {
        var command = BuildValidCommand();
        command.Name = string.Empty;
        var handler = new CreateJobApplicationCommandHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenPositionIsEmpty()
    {
        var command = BuildValidCommand();
        command.Position = string.Empty;
        var handler = new CreateJobApplicationCommandHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenUrlIsInvalid()
    {
        var command = BuildValidCommand();
        command.URL = "not-a-valid-url";
        var handler = new CreateJobApplicationCommandHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenStartDateIsDefault()
    {
        var command = BuildValidCommand();
        command.StartDate = default;
        var handler = new CreateJobApplicationCommandHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenDurationInMonthsIsNotNumeric()
    {
        var command = BuildValidCommand();
        command.DurationInMonths = "abc";
        var handler = new CreateJobApplicationCommandHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotCallCreateAsync_WhenValidationFails()
    {
        var command = BuildValidCommand();
        command.Name = string.Empty;
        var handler = new CreateJobApplicationCommandHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));

        _mockRepo.Verify(r => r.CreateAsync(It.IsAny<JobApplication>()), Times.Never);
    }
}
