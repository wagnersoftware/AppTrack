using AppTrack.Application.Contracts.Logging;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Mappings;

namespace AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplicationsForUser;

public class GetJobApplicationsForUserQueryHandler : IRequestHandler<GetJobApplicationsForUserQuery, List<JobApplicationDto>>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IAppLogger<GetJobApplicationsForUserQueryHandler> _logger;

    public GetJobApplicationsForUserQueryHandler(IJobApplicationRepository jobApplicationRepository, IAppLogger<GetJobApplicationsForUserQueryHandler> logger)
    {
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

        var jobApplications = await _jobApplicationRepository.GetAllForUserWithAiTextHistoryAsync(request.UserId);
        var jobApplicationDtos = jobApplications.Select(j => j.ToDto()).ToList();
        _logger.LogInformation("JobApplications were retieved successfully.");
        return jobApplicationDtos;
    }
}
