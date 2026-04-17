using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using FluentValidation;
using FluentValidation.Results;
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
    private readonly Mock<IValidator<UpdateJobApplicationDefaultsCommand>> _mockValidator;

    public UpdateJobApplicationDefaultsCommandHandlerTests()
    {
        _mockRepo = new Mock<IJobApplicationDefaultsRepository>();
        _mockValidator = new Mock<IValidator<UpdateJobApplicationDefaultsCommand>>();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateJobApplicationDefaultsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

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

    private UpdateJobApplicationDefaultsCommandHandler CreateHandler() =>
        new(_mockRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldReturnUpdatedDto_WhenCommandIsValid()
    {
        var command = BuildValidCommand();

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<JobApplicationDefaultsDto>();
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<DomainJobApplicationDefaults>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenEntityDoesNotExist()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateJobApplicationDefaultsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "Job application defaults not found.")]));

        var command = BuildValidCommand(id: 9999);

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenEntityBelongsToAnotherUser()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateJobApplicationDefaultsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("UserId", "Job application defaults not assigned to this user.")]));

        var command = BuildValidCommand(id: ExistingId, userId: OtherId);

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenIdIsZero()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateJobApplicationDefaultsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "Id is required")]));

        var command = BuildValidCommand(id: 0);

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotCallUpdateAsync_WhenValidationFails()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateJobApplicationDefaultsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Id", "Job application defaults not found.")]));

        var command = BuildValidCommand(id: 9999);

        await Should.ThrowAsync<BadRequestException>(() => CreateHandler().Handle(command, CancellationToken.None));

        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<DomainJobApplicationDefaults>()), Times.Never);
    }
}
