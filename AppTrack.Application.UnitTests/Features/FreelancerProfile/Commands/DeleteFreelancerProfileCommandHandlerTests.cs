using AppTrack.Application.Contracts.CvStorage;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Commands.DeleteFreelancerProfile;
using AppTrack.Application.UnitTests.Mocks;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Commands;

public class DeleteFreelancerProfileCommandHandlerTests
{
    private readonly Mock<IFreelancerProfileRepository> _mockRepo;
    private readonly Mock<ICvStorageService> _mockStorage;
    private readonly DeleteFreelancerProfileCommandHandler _handler;

    private const string BlobPath = $"{MockFreelancerProfileRepository.ExistingUserId}/cv.pdf";

    public DeleteFreelancerProfileCommandHandlerTests()
    {
        _mockRepo = MockFreelancerProfileRepository.GetMock();
        _mockStorage = MockCvStorageService.GetMock();
        _handler = new DeleteFreelancerProfileCommandHandler(_mockRepo.Object, _mockStorage.Object);
    }

    private void SetupProfileWithCv()
    {
        var profile = new AppTrack.Domain.FreelancerProfile
        {
            Id = MockFreelancerProfileRepository.ExistingId,
            UserId = MockFreelancerProfileRepository.ExistingUserId,
            CvBlobPath = BlobPath,
            CvFileName = "resume.pdf",
            CvText = "CV content",
        };
        _mockRepo
            .Setup(r => r.GetByUserIdAsync(MockFreelancerProfileRepository.ExistingUserId))
            .ReturnsAsync(profile);
    }

    [Fact]
    public async Task Handle_ShouldDeleteBlobAndProfile_WhenCvExists()
    {
        SetupProfileWithCv();
        _mockRepo.Setup(r => r.DeleteAsync(It.IsAny<AppTrack.Domain.FreelancerProfile>()))
            .Returns(Task.CompletedTask);
        var command = new DeleteFreelancerProfileCommand { UserId = MockFreelancerProfileRepository.ExistingUserId };

        await _handler.Handle(command, CancellationToken.None);

        _mockStorage.Verify(s => s.DeleteAsync(BlobPath), Times.Once);
        _mockRepo.Verify(r => r.DeleteAsync(It.Is<AppTrack.Domain.FreelancerProfile>(
            p => p.UserId == MockFreelancerProfileRepository.ExistingUserId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDeleteProfileWithoutCallingStorage_WhenNoCvExists()
    {
        // default mock profile has no CV (CvBlobPath is null)
        _mockRepo.Setup(r => r.DeleteAsync(It.IsAny<AppTrack.Domain.FreelancerProfile>()))
            .Returns(Task.CompletedTask);
        var command = new DeleteFreelancerProfileCommand { UserId = MockFreelancerProfileRepository.ExistingUserId };

        await _handler.Handle(command, CancellationToken.None);

        _mockStorage.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<AppTrack.Domain.FreelancerProfile>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProfileDoesNotExist()
    {
        var command = new DeleteFreelancerProfileCommand { UserId = "unknown-user" };

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _mockStorage.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<AppTrack.Domain.FreelancerProfile>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteBlob_WhenDeleteProfileFails()
    {
        SetupProfileWithCv();
        _mockRepo.Setup(r => r.DeleteAsync(It.IsAny<AppTrack.Domain.FreelancerProfile>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
        var command = new DeleteFreelancerProfileCommand { UserId = MockFreelancerProfileRepository.ExistingUserId };

        await Should.ThrowAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));

        _mockStorage.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);
    }
}
