using AppTrack.Application.Contracts.CvStorage;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Commands.DeleteCv;
using AppTrack.Application.UnitTests.Mocks;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Commands;

public class DeleteCvCommandHandlerTests
{
    private readonly Mock<IFreelancerProfileRepository> _mockRepo;
    private readonly Mock<ICvStorageService> _mockStorage;
    private readonly DeleteCvCommandHandler _handler;

    private const string BlobPath = $"{MockFreelancerProfileRepository.ExistingUserId}/cv.pdf";

    public DeleteCvCommandHandlerTests()
    {
        _mockRepo = MockFreelancerProfileRepository.GetMock();
        _mockStorage = MockCvStorageService.GetMock();
        _handler = new DeleteCvCommandHandler(_mockRepo.Object, _mockStorage.Object);
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
    public async Task Handle_ShouldDeleteBlobAndClearCvFields_WhenCvExists()
    {
        SetupProfileWithCv();
        var command = new DeleteCvCommand { UserId = MockFreelancerProfileRepository.ExistingUserId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.CvBlobPath.ShouldBeNull();
        result.CvFileName.ShouldBeNull();
        result.CvText.ShouldBeNull();
        _mockRepo.Verify(
            r => r.UpsertAsync(It.Is<AppTrack.Domain.FreelancerProfile>(p =>
                p.CvBlobPath == null && p.CvFileName == null && p.CvText == null)),
            Times.Once);
        _mockStorage.Verify(s => s.DeleteAsync(BlobPath), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteBlob_WhenUpsertFails()
    {
        SetupProfileWithCv();
        _mockRepo
            .Setup(r => r.UpsertAsync(It.IsAny<AppTrack.Domain.FreelancerProfile>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var command = new DeleteCvCommand { UserId = MockFreelancerProfileRepository.ExistingUserId };

        await Should.ThrowAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));

        _mockStorage.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCallStorageOrUpsert_WhenNoCvExists()
    {
        // default mock profile has no CV (CvBlobPath is null)
        var command = new DeleteCvCommand { UserId = MockFreelancerProfileRepository.ExistingUserId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        _mockStorage.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);
        _mockRepo.Verify(r => r.UpsertAsync(It.IsAny<AppTrack.Domain.FreelancerProfile>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProfileDoesNotExist()
    {
        var command = new DeleteCvCommand { UserId = "unknown-user" };

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _mockStorage.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);
    }
}
