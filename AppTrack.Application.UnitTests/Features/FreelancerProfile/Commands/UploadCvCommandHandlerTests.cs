using AppTrack.Application.Contracts.CvStorage;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Commands.UploadCv;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.UnitTests.Mocks;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Commands;

public class UploadCvCommandHandlerTests
{
    private readonly Mock<IFreelancerProfileRepository> _mockRepo;
    private readonly Mock<ICvStorageService> _mockStorage;
    private readonly Mock<IPdfTextExtractor> _mockExtractor;
    private readonly UploadCvCommandHandler _handler;

    public UploadCvCommandHandlerTests()
    {
        _mockRepo = MockFreelancerProfileRepository.GetMock();
        _mockStorage = MockCvStorageService.GetMock();
        _mockExtractor = MockPdfTextExtractor.GetMock();
        _handler = new UploadCvCommandHandler(_mockRepo.Object, _mockStorage.Object, _mockExtractor.Object);
    }

    private static UploadCvCommand ValidCommand(string userId = MockFreelancerProfileRepository.ExistingUserId) => new()
    {
        UserId = userId,
        FileStream = new MemoryStream([0x25, 0x50, 0x44, 0x46]), // %PDF
        FileName = "cv.pdf",
        ContentType = "application/pdf",
        FileSizeBytes = 4,
    };

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenExistingProfileIsUpdated()
    {
        var command = ValidCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<FreelancerProfileDto>();
        result.CvFileName.ShouldBe("cv.pdf");
        result.CvBlobPath.ShouldBe($"{MockFreelancerProfileRepository.ExistingUserId}/cv.pdf");
        result.CvText.ShouldBe(MockPdfTextExtractor.ExtractedText);
    }

    [Fact]
    public async Task Handle_ShouldAutoCreateProfile_WhenNoExistingProfile()
    {
        var command = ValidCommand(userId: "new-user");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        _mockRepo.Verify(
            r => r.UpsertAsync(It.Is<AppTrack.Domain.FreelancerProfile>(p => p.Id == 0 && p.UserId == "new-user")),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldExtractTextAndSetOnProfile()
    {
        var command = ValidCommand();

        await _handler.Handle(command, CancellationToken.None);

        _mockExtractor.Verify(e => e.ExtractText(It.IsAny<Stream>()), Times.Once);
        _mockRepo.Verify(
            r => r.UpsertAsync(It.Is<AppTrack.Domain.FreelancerProfile>(p => p.CvText == MockPdfTextExtractor.ExtractedText)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenValidationFails()
    {
        var command = ValidCommand();
        command.FileName = string.Empty;

        await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotCallStorage_WhenValidationFails()
    {
        var command = ValidCommand();
        command.ContentType = "image/png";
        command.FileName = "photo.png";

        await Should.ThrowAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _mockStorage.Verify(
            s => s.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteBlob_WhenUpsertFails()
    {
        _mockRepo
            .Setup(r => r.UpsertAsync(It.IsAny<AppTrack.Domain.FreelancerProfile>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var command = ValidCommand();

        await Should.ThrowAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));

        _mockStorage.Verify(
            s => s.DeleteAsync($"{MockFreelancerProfileRepository.ExistingUserId}/cv.pdf"),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRethrow_WhenUpsertFails()
    {
        var dbException = new InvalidOperationException("DB error");
        _mockRepo
            .Setup(r => r.UpsertAsync(It.IsAny<AppTrack.Domain.FreelancerProfile>()))
            .ThrowsAsync(dbException);

        var thrown = await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(ValidCommand(), CancellationToken.None));

        thrown.ShouldBeSameAs(dbException);
    }
}
