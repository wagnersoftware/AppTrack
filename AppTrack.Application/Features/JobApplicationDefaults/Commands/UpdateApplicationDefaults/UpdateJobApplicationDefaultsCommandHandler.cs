using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;
using AutoMapper;

namespace AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;

public class UpdateJobApplicationDefaultsCommandHandler : IRequestHandler<UpdateJobApplicationDefaultsCommand, Unit>
{
    private readonly IMapper _mapper;
    private readonly IJobApplicationDefaultsRepository _jobApplicationDefaultsRepository;

    public UpdateJobApplicationDefaultsCommandHandler(IMapper mapper, IJobApplicationDefaultsRepository jobApplicationDefaultsRepository)
    {
        _mapper = mapper;
        _jobApplicationDefaultsRepository = jobApplicationDefaultsRepository;
    }

    public async Task<Unit> Handle(UpdateJobApplicationDefaultsCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateJobApplicationDefaultsCommandValidator(_jobApplicationDefaultsRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid update job application defaults request", validationResult);
        }

        var jobApplicationDefaultsToUpdate = await _jobApplicationDefaultsRepository.GetByIdAsync(request.Id);
        _mapper.Map(request, jobApplicationDefaultsToUpdate);
        await _jobApplicationDefaultsRepository.UpdateAsync(jobApplicationDefaultsToUpdate!);

        return Unit.Value;
    }
}
