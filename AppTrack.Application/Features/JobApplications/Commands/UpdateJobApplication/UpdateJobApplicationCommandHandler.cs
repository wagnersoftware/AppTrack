using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;
using AutoMapper;

namespace AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;

public class UpdateJobApplicationCommandHandler : IRequestHandler<UpdateJobApplicationCommand, Unit>
{
    private readonly IMapper _mapper;
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public UpdateJobApplicationCommandHandler(IMapper mapper, IJobApplicationRepository jobApplicationRepository)
    {
        _mapper = mapper;
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<Unit> Handle(UpdateJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateJobApplicationCommandValidator(_jobApplicationRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid JobApplication", validationResult);
        }

        var jobApplicationToUpdate = _mapper.Map<Domain.JobApplication>(request);

        await _jobApplicationRepository.UpdateAsync(jobApplicationToUpdate);

        return Unit.Value;
    }
}
