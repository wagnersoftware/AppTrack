using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.Mappings;
using AppTrack.Domain;

namespace AppTrack.Application.Features.FreelancerProfile.Queries.GetFreelancerProfile;

public class GetFreelancerProfileQueryHandler : IRequestHandler<GetFreelancerProfileQuery, FreelancerProfileDto>
{
    private readonly IFreelancerProfileRepository _repository;

    public GetFreelancerProfileQueryHandler(IFreelancerProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<FreelancerProfileDto> Handle(GetFreelancerProfileQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetFreelancerProfileQueryValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count > 0)
        {
            throw new BadRequestException("Invalid get profile request", validationResult);
        }

        var profile = await _repository.GetByUserIdAsync(request.UserId);

        if (profile == null)
        {
            throw new NotFoundException(nameof(FreelancerProfile), request.UserId);
        }

        return profile.ToDto();
    }
}
