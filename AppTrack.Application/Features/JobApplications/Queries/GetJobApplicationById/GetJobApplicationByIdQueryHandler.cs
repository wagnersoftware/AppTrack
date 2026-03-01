using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Mappings;

namespace AppTrack.Application.Features.JobApplications.Queries.GetJobApplicationById;

public class GetJobApplicationByIdQueryHandler : IRequestHandler<GetJobApplicationByIdQuery, JobApplicationDto>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public GetJobApplicationByIdQueryHandler(IJobApplicationRepository jobApplicationRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<JobApplicationDto> Handle(GetJobApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetJobApplicationByIdQueryValidator(_jobApplicationRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid get job application request", validationResult);
        }

        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.Id);

        if (jobApplication == null)
        {
            throw new NotFoundException(nameof(jobApplication), request.Id);
        }

        return jobApplication.ToDto();
    }
}
