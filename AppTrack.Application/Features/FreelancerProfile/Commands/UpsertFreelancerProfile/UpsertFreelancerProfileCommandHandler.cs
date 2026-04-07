using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.Mappings;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;

public class UpsertFreelancerProfileCommandHandler : IRequestHandler<UpsertFreelancerProfileCommand, FreelancerProfileDto>
{
    private readonly IFreelancerProfileRepository _repository;

    public UpsertFreelancerProfileCommandHandler(IFreelancerProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<FreelancerProfileDto> Handle(UpsertFreelancerProfileCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpsertFreelancerProfileCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count > 0)
        {
            throw new BadRequestException("Invalid freelancer profile request", validationResult);
        }

        var existing = await _repository.GetByUserIdAsync(request.UserId);

        if (existing == null)
        {
            var newProfile = request.ToNewDomain();
            await _repository.UpsertAsync(newProfile);
            return newProfile.ToDto();
        }

        request.ApplyTo(existing);
        await _repository.UpsertAsync(existing);
        return existing.ToDto();
    }
}
