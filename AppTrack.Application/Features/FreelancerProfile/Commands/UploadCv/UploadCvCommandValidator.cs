using FluentValidation;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.UploadCv;

public class UploadCvCommandValidator : AbstractValidator<UploadCvCommand>
{
    private const long MaxFileSizeBytes = 10L * 1024 * 1024;

    public UploadCvCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.");

        RuleFor(x => x.ContentType)
            .Must((cmd, contentType) =>
                string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase) ||
                cmd.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only PDF files are allowed.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File must not be empty.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("File must not exceed 10 MB.");
    }
}
