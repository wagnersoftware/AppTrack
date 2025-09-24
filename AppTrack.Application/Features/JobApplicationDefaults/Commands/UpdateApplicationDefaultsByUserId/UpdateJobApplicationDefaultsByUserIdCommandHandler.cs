using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;
using AutoMapper;

namespace AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaultsByUserId;

public class UpdateJobApplicationDefaultsByUserIdCommandHandler : IRequestHandler<UpdateJobApplicationDefaultsByUserIdCommand, Unit>
{
    private readonly IMapper _mapper;
    private readonly IJobApplicationDefaultsRepository _jobApplicationDefaultsRepository;

    public UpdateJobApplicationDefaultsByUserIdCommandHandler(IMapper mapper, IJobApplicationDefaultsRepository jobApplicationDefaultsRepository)
    {
        this._mapper = mapper;
        this._jobApplicationDefaultsRepository = jobApplicationDefaultsRepository;
    }

    public async Task<Unit> Handle(UpdateJobApplicationDefaultsByUserIdCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateJobApplicationDefaultsByUserIdCommandValidator(_jobApplicationDefaultsRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid get job application defaults request", validationResult);
        }

        var jobApplicationDefaultsToUpdate = _mapper.Map<Domain.JobApplicationDefaults>(request);

        await _jobApplicationDefaultsRepository.UpdateAsync(jobApplicationDefaultsToUpdate);

        return Unit.Value;
    }
}
