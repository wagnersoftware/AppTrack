using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.UnitTests.Mocks;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Commands;

public class UpsertFreelancerProfileCommandHandlerTests
{
    private readonly Mock<IFreelancerProfileRepository> _mockRepo;
    private readonly UpsertFreelancerProfileCommandHandler _handler;

    public UpsertFreelancerProfileCommandHandlerTests()
    {
        _mockRepo = MockFreelancerProfileRepository.GetMock();
        _handler = new UpsertFreelancerProfileCommandHandler(_mockRepo.Object);
    }

    private static UpsertFreelancerProfileCommand ValidCommand(string userId = MockFreelancerProfileRepository.ExistingUserId) => new()
    {
        UserId = userId,
        FirstName = "Anna",
        LastName = "Müller",
    };

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenCommandIsValid()
    {
        // Arrange
        var command = ValidCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<FreelancerProfileDto>();
        result.FirstName.ShouldBe("Anna");
        result.LastName.ShouldBe("Müller");
    }

    [Fact]
    public async Task Handle_ShouldCreate_WhenNoExistingProfile()
    {
        // Arrange
        var command = ValidCommand(userId: "new-user");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        _mockRepo.Verify(r => r.UpsertAsync(It.Is<AppTrack.Domain.FreelancerProfile>(p => p.Id == 0)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdate_WhenProfileExists()
    {
        // Arrange
        var command = ValidCommand(userId: MockFreelancerProfileRepository.ExistingUserId);
        command.FirstName = "Updated";

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FirstName.ShouldBe("Updated");
        _mockRepo.Verify(r => r.UpsertAsync(It.Is<AppTrack.Domain.FreelancerProfile>(p => p.Id == MockFreelancerProfileRepository.ExistingId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenHourlyRateIsNegative()
    {
        // Arrange
        var command = ValidCommand();
        command.HourlyRate = -10;

        // Act & Assert
        var ex = await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        ex.ValidationErrors.ShouldContainKey("HourlyRate");
    }

    [Fact]
    public async Task Handle_ShouldNotCallUpsertAsync_WhenValidationFails()
    {
        // Arrange
        var command = ValidCommand();
        command.HourlyRate = -10;

        // Act & Assert
        await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _mockRepo.Verify(r => r.UpsertAsync(It.IsAny<AppTrack.Domain.FreelancerProfile>()), Times.Never);
    }
}
