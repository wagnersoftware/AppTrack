using AppTrack.Application.Contracts.Logging;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AutoMapper;

namespace AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplicationsForUser;
public class GetJobApplicationsForUserQueryHandler : IRequestHandler<GetJobApplicationsForUserQuery, List<JobApplicationDto>>
{
    private readonly IMapper _mapper;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IAppLogger<GetJobApplicationsForUserQueryHandler> _logger;

    public GetJobApplicationsForUserQueryHandler(IMapper mapper, IJobApplicationRepository jobApplicationRepository, IAppLogger<GetJobApplicationsForUserQueryHandler> logger)
    {
        _mapper = mapper;
        _jobApplicationRepository = jobApplicationRepository;
        _logger = logger;
    }

    public async Task<List<JobApplicationDto>> Handle(GetJobApplicationsForUserQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetJobApplicationsForUserQueryValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid Request", validationResult);
        }

        var jobApplications = await _jobApplicationRepository.GetAllForUserAsync(request.UserId);
        var jobApplicationDtos = _mapper.Map<List<JobApplicationDto>>(jobApplications);
        _logger.LogInformation("JobApplications were retieved successfully.");
        return jobApplicationDtos;
    }
}

