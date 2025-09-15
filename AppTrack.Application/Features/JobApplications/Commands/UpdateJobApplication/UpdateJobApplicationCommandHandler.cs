using AppTrack.Application.Contracts.Persistance;
using AutoMapper;
using MediatR;

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
        var jobApplicationToUpdate = _mapper.Map<Domain.JobApplication>(request);

        await _jobApplicationRepository.UpdateAsync(jobApplicationToUpdate);

        return Unit.Value;
    }
}
