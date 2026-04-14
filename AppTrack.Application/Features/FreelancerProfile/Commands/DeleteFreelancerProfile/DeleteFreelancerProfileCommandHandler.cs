using AppTrack.Application.Contracts.CvStorage;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.DeleteFreelancerProfile;

public class DeleteFreelancerProfileCommandHandler : IRequestHandler<DeleteFreelancerProfileCommand, Unit>
{
    private readonly IFreelancerProfileRepository _repository;
    private readonly ICvStorageService _cvStorageService;

    public DeleteFreelancerProfileCommandHandler(IFreelancerProfileRepository repository, ICvStorageService cvStorageService)
    {
        _repository = repository;
        _cvStorageService = cvStorageService;
    }

    public async Task<Unit> Handle(DeleteFreelancerProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByUserIdAsync(request.UserId)
            ?? throw new NotFoundException(nameof(Domain.FreelancerProfile), request.UserId);

        var blobPath = profile.CvBlobPath;

        await _repository.DeleteAsync(profile);

        if (blobPath is not null)
        {
            await _cvStorageService.DeleteAsync(blobPath);
        }

        return Unit.Value;
    }
}
