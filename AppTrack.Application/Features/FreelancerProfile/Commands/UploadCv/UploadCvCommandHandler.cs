using AppTrack.Application.Contracts.CvStorage;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.Mappings;
using FluentValidation;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.UploadCv;

public class UploadCvCommandHandler : IRequestHandler<UploadCvCommand, FreelancerProfileDto>
{
    private readonly IFreelancerProfileRepository _repository;
    private readonly ICvStorageService _cvStorageService;
    private readonly IPdfTextExtractor _pdfTextExtractor;
    private readonly IValidator<UploadCvCommand> _validator;

    public UploadCvCommandHandler(
        IFreelancerProfileRepository repository,
        ICvStorageService cvStorageService,
        IPdfTextExtractor pdfTextExtractor,
        IValidator<UploadCvCommand> validator)
    {
        _repository = repository;
        _cvStorageService = cvStorageService;
        _pdfTextExtractor = pdfTextExtractor;
        _validator = validator;
    }

    public async Task<FreelancerProfileDto> Handle(UploadCvCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count > 0)
        {
            throw new BadRequestException("Invalid CV upload request", validationResult);
        }

        using var memStream = new MemoryStream();
        await request.FileStream.CopyToAsync(memStream, cancellationToken);
        memStream.Position = 0;

        var extractedText = _pdfTextExtractor.ExtractText(memStream);
        memStream.Position = 0;

        var blobPath = await _cvStorageService.UploadAsync(request.UserId, memStream, request.FileName);

        var profile = await _repository.GetByUserIdAsync(request.UserId)
            ?? new Domain.FreelancerProfile { UserId = request.UserId };

        profile.CvBlobPath = blobPath;
        profile.CvFileName = request.FileName;
        profile.CvText = extractedText;
        profile.CvUploadDate = DateTime.UtcNow;

        try
        {
            await _repository.UpsertAsync(profile);
        }
        catch
        {
            await _cvStorageService.DeleteAsync(blobPath);
            throw;
        }

        return profile.ToDto();
    }
}
