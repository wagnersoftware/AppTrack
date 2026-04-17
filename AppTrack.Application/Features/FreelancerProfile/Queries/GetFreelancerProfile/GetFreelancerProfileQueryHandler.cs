using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.Mappings;
using FluentValidation;

namespace AppTrack.Application.Features.FreelancerProfile.Queries.GetFreelancerProfile;

public class GetFreelancerProfileQueryHandler : IRequestHandler<GetFreelancerProfileQuery, FreelancerProfileDto>
{
    private readonly IFreelancerProfileRepository _repository;
    private readonly IValidator<GetFreelancerProfileQuery> _validator;

    public GetFreelancerProfileQueryHandler(IFreelancerProfileRepository repository, IValidator<GetFreelancerProfileQuery> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<FreelancerProfileDto> Handle(GetFreelancerProfileQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

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
