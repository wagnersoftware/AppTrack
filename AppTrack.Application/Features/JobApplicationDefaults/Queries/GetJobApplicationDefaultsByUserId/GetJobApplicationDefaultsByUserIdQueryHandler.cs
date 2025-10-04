using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using AutoMapper;

namespace AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId
{
    public class GetJobApplicationDefaultsByUserIdQueryHandler : IRequestHandler<GetJobApplicationDefaultsByUserIdQuery, JobApplicationDefaultsDto>
    {
        private readonly IMapper _mapper;
        private readonly IJobApplicationDefaultsRepository _jobApplicationDefaultsRepository;

        public GetJobApplicationDefaultsByUserIdQueryHandler(IMapper mapper, IJobApplicationDefaultsRepository jobApplicationRepository)
        {
            this._mapper = mapper;
            this._jobApplicationDefaultsRepository = jobApplicationRepository;
        }

        /// <summary>
        /// Gets the job application default values for the specified user. Creates and returns a new instance, if the entity doesn't exist.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="BadRequestException"></exception>
        public async Task<JobApplicationDefaultsDto> Handle(GetJobApplicationDefaultsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var validator = new GetJobApplicationDefaultsByUserIdQueryValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Any())
            {
                throw new BadRequestException($"Invalid get job application defaults request", validationResult);
            }

            var jobApplicationDefaults = await _jobApplicationDefaultsRepository.GetByUserIdAsync(request.UserId);

            if (jobApplicationDefaults == null)
            {
                jobApplicationDefaults = await _jobApplicationDefaultsRepository.CreateForUserAsync(request.UserId);
            }

            var jobApplicationDto = _mapper.Map<JobApplicationDefaultsDto>(jobApplicationDefaults);

            return jobApplicationDto;
        }
    }
}
