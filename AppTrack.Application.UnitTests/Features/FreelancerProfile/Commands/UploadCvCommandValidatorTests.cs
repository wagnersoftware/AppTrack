using AppTrack.Application.Features.FreelancerProfile.Commands.UploadCv;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Commands;

public class UploadCvCommandValidatorTests
{
    private readonly UploadCvCommandValidator _validator = new();

    private static UploadCvCommand ValidCommand() => new()
    {
        UserId = "user-1",
        FileName = "my_cv.pdf",
        ContentType = "application/pdf",
        FileSizeBytes = 1024,
    };

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var result = await _validator.TestValidateAsync(ValidCommand());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenContentTypeIsOctetStream_AndFileNameEndsDotPdf()
    {
        var command = ValidCommand();
        command.ContentType = "application/octet-stream";
        command.FileName = "cv.PDF"; // case-insensitive

        var result = await _validator.TestValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenFileNameIsEmpty()
    {
        var command = ValidCommand();
        command.FileName = string.Empty;

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenNotPdfByTypeOrExtension()
    {
        var command = ValidCommand();
        command.ContentType = "image/png";
        command.FileName = "photo.png";

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenFileSizeIsZero()
    {
        var command = ValidCommand();
        command.FileSizeBytes = 0;

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.FileSizeBytes);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenFileSizeExceeds10Mb()
    {
        var command = ValidCommand();
        command.FileSizeBytes = 10L * 1024 * 1024 + 1;

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.FileSizeBytes);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenFileSizeIsExactly10Mb()
    {
        var command = ValidCommand();
        command.FileSizeBytes = 10L * 1024 * 1024;

        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FileSizeBytes);
    }
}
