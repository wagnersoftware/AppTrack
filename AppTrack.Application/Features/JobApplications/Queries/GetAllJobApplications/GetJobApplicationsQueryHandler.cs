using AppTrack.Application.Contracts.Logging;
using AppTrack.Application.Contracts.Persistance;
using AutoMapper;
using MediatR;

namespace AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplications;
public class GetJobApplicationsQueryHandler : IRequestHandler<GetJobApplicationsQuery, List<JobApplicationDto>>
{
    private readonly IMapper _mapper;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IAppLogger<GetJobApplicationsQueryHandler> _logger;

    public GetJobApplicationsQueryHandler(IMapper mapper, IJobApplicationRepository jobApplicationRepository, IAppLogger<GetJobApplicationsQueryHandler> logger)
    {
        this._mapper = mapper;
        this._jobApplicationRepository = jobApplicationRepository;
        this._logger = logger;
    }

    public async Task<List<JobApplicationDto>> Handle(GetJobApplicationsQuery request, CancellationToken cancellationToken)
    {
        var jobApplications = await _jobApplicationRepository.GetAsync();
        var jobApplicationDtos = _mapper.Map<List<JobApplicationDto>>(jobApplications);
        _logger.LogInformation("JobApplications were retieved successfully.");
        return jobApplicationDtos;
    }
}

