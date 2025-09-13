using AppTrack.Application.Contracts.Persistance;
using AutoMapper;
using MediatR;

namespace AppTrack.Application.Features.JobApplication.Queries.GetAllJobApplications;
public class GetJobApplicationsQueryHandler : IRequestHandler<GetJobApplicationsQuery, List<JobApplicationDto>>
{
    private readonly IMapper _mapper;
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public GetJobApplicationsQueryHandler(IMapper mapper, IJobApplicationRepository jobApplicationRepository)
    {
        _mapper = mapper;
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<List<JobApplicationDto>> Handle(GetJobApplicationsQuery request, CancellationToken cancellationToken)
    {
        var jobApplications = await _jobApplicationRepository.GetAsync();

        var jobApplicationDtos = _mapper.Map<List<JobApplicationDto>>(jobApplications);

        return jobApplicationDtos;
    }
}

