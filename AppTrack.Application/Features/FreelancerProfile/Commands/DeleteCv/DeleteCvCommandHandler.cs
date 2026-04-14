using AppTrack.Application.Contracts.CvStorage;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.Mappings;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.DeleteCv;

public class DeleteCvCommandHandler : IRequestHandler<DeleteCvCommand, FreelancerProfileDto>
{
    private readonly IFreelancerProfileRepository _repository;
    private readonly ICvStorageService _cvStorageService;

    public DeleteCvCommandHandler(IFreelancerProfileRepository repository, ICvStorageService cvStorageService)
    {
        _repository = repository;
        _cvStorageService = cvStorageService;
    }

    public async Task<FreelancerProfileDto> Handle(DeleteCvCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByUserIdAsync(request.UserId)
            ?? throw new NotFoundException(nameof(Domain.FreelancerProfile), request.UserId);

        if (profile.CvBlobPath is not null)
        {
            var blobPath = profile.CvBlobPath;

            profile.CvBlobPath = null;
            profile.CvFileName = null;
            profile.CvText = null;
            profile.CvUploadDate = null;
            await _repository.UpsertAsync(profile);

            await _cvStorageService.DeleteAsync(blobPath);
        }

        return profile.ToDto();
    }
}
