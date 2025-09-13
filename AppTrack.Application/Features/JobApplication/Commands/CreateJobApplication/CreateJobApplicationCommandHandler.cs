using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AutoMapper;
using MediatR;

namespace AppTrack.Application.Features.JobApplication.Commands.CreateJobApplication;
public class CreateJobApplicationCommandHandler : IRequestHandler<CreateJobApplicationCommand, int>
{
    private readonly IMapper _mapper;
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public CreateJobApplicationCommandHandler(IMapper mapper, IJobApplicationRepository jobApplicationRepository)
    {
        _mapper = mapper;
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<int> Handle(CreateJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateJobApplicationCommandValidator(_jobApplicationRepository);
        var validationResult = await validator.ValidateAsync(request);

        if(validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid JobApplication", validationResult);
        }

        var jobApplicationToCreate = _mapper.Map<Domain.JobApplication>(request);

        await _jobApplicationRepository.CreateAsync(jobApplicationToCreate);

        return jobApplicationToCreate.Id;
    }
}

