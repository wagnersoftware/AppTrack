using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplications;
using AutoMapper;

namespace AppTrack.Application.Features.JobApplications.Queries.GetJobApplicationById;

public class GetJobApplicationByIdQueryHandler : IRequestHandler<GetJobApplicationByIdQuery, JobApplicationDto>
{
    private readonly IMapper _mapper;
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public GetJobApplicationByIdQueryHandler(IMapper mapper,IJobApplicationRepository jobApplicationRepository)
    {
        this._mapper = mapper;
        this._jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<JobApplicationDto> Handle(GetJobApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.Id);

        if (jobApplication == null)
        {
            throw new NotFoundException(nameof(jobApplication), request.Id);
        }

        var jobApplicationDto = _mapper.Map<JobApplicationDto>(jobApplication);

        return jobApplicationDto;
    }
}
