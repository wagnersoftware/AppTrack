using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using Moq;
using Shouldly;
using DomainJobApplicationDefaults = AppTrack.Domain.JobApplicationDefaults;

namespace AppTrack.Application.UnitTests.Features.JobApplicationDefaults.Commands;

public class UpdateJobApplicationDefaultsCommandHandlerTests
{
    private const string OwnerId = "user-123";
    private const string OtherId = "other-user";
    private const int ExistingId = 1;

    private readonly Mock<IJobApplicationDefaultsRepository> _mockRepo;

    public UpdateJobApplicationDefaultsCommandHandlerTests()
    {
        _mockRepo = new Mock<IJobApplicationDefaultsRepository>();

        var existingDefaults = new DomainJobApplicationDefaults
        {
            Id = ExistingId,
            UserId = OwnerId,
            Name = "Old Name",
            Position = "Old Position",
            Location = "Old Location"
        };

        _mockRepo
            .Setup(r => r.GetByIdAsync(ExistingId))
            .ReturnsAsync(existingDefaults);

        _mockRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingId)))
            .ReturnsAsync((DomainJobApplicationDefaults?)null);

        _mockRepo
            .Setup(r => r.UpdateAsync(It.IsAny<DomainJobApplicationDefaults>()))
            .Returns(Task.CompletedTask);
    }

    private static UpdateJobApplicationDefaultsCommand BuildValidCommand(int id = ExistingId, string userId = OwnerId) => new()
    {
        Id = id,
        UserId = userId,
        Name = "New Name",
        Position = "New Position",
        Location = "New Location"
    };

    [Fact]
    public async Task Handle_ShouldReturnUpdatedDto_WhenCommandIsValid()
    {
        var command = BuildValidCommand();
        var handler = new UpdateJobApplicationDefaultsCommandHandler(_mockRepo.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<JobApplicationDefaultsDto>();
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<DomainJobApplicationDefaults>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenEntityDoesNotExist()
    {
        var command = BuildValidCommand(id: 9999);
        var handler = new UpdateJobApplicationDefaultsCommandHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenEntityBelongsToAnotherUser()
    {
        var command = BuildValidCommand(id: ExistingId, userId: OtherId);
        var handler = new UpdateJobApplicationDefaultsCommandHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenIdIsZero()
    {
        var command = BuildValidCommand(id: 0);
        var handler = new UpdateJobApplicationDefaultsCommandHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotCallUpdateAsync_WhenValidationFails()
    {
        var command = BuildValidCommand(id: 9999);
        var handler = new UpdateJobApplicationDefaultsCommandHandler(_mockRepo.Object);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));

        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<DomainJobApplicationDefaults>()), Times.Never);
    }
}
