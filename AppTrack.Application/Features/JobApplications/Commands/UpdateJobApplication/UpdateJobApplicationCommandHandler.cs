using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AutoMapper;

namespace AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;

public class UpdateJobApplicationCommandHandler : IRequestHandler<UpdateJobApplicationCommand, JobApplicationDto>
{
    private readonly IMapper _mapper;
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public UpdateJobApplicationCommandHandler(IMapper mapper, IJobApplicationRepository jobApplicationRepository)
    {
        _mapper = mapper;
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<JobApplicationDto> Handle(UpdateJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateJobApplicationCommandValidator(_jobApplicationRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid JobApplication", validationResult);
        }

        var jobApplicationToUpdate = await _jobApplicationRepository.GetByIdAsync(request.Id);

        if (jobApplicationToUpdate == null)
        {
            throw new NotFoundException("Job Application", request.Id);
        }

        _mapper.Map(request, jobApplicationToUpdate);

        await _jobApplicationRepository.UpdateAsync(jobApplicationToUpdate);

        var jobApplicationDto = _mapper.Map<JobApplicationDto>(jobApplicationToUpdate);

        return jobApplicationDto;
    }
}
