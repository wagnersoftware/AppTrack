using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Mappings;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Queries.GetJobApplicationById;

public class GetJobApplicationByIdQueryHandler : IRequestHandler<GetJobApplicationByIdQuery, JobApplicationDto>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IValidator<GetJobApplicationByIdQuery> _validator;

    public GetJobApplicationByIdQueryHandler(IJobApplicationRepository jobApplicationRepository, IValidator<GetJobApplicationByIdQuery> validator)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _validator = validator;
    }

    public async Task<JobApplicationDto> Handle(GetJobApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid get job application request", validationResult);
        }

        var jobApplication = await _jobApplicationRepository.GetByIdWithAiTextHistoryAsync(request.Id);

        if (jobApplication == null)
        {
            throw new NotFoundException(nameof(jobApplication), request.Id);
        }

        return jobApplication.ToDto();
    }
}
