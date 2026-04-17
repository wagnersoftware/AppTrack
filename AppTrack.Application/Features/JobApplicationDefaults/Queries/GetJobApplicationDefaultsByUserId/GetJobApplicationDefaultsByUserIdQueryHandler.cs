using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using AppTrack.Application.Mappings;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId
{
    public class GetJobApplicationDefaultsByUserIdQueryHandler : IRequestHandler<GetJobApplicationDefaultsByUserIdQuery, JobApplicationDefaultsDto>
    {
        private readonly IJobApplicationDefaultsRepository _jobApplicationDefaultsRepository;
        private readonly IValidator<GetJobApplicationDefaultsByUserIdQuery> _validator;

        public GetJobApplicationDefaultsByUserIdQueryHandler(IJobApplicationDefaultsRepository jobApplicationRepository, IValidator<GetJobApplicationDefaultsByUserIdQuery> validator)
        {
            this._jobApplicationDefaultsRepository = jobApplicationRepository;
            _validator = validator;
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
            var validationResult = await _validator.ValidateAsync(request);

            if (validationResult.Errors.Any())
            {
                throw new BadRequestException($"Invalid get job application defaults request", validationResult);
            }

            var jobApplicationDefaults = await _jobApplicationDefaultsRepository.GetByUserIdAsync(request.UserId);

            if (jobApplicationDefaults == null)
            {
                jobApplicationDefaults = await _jobApplicationDefaultsRepository.CreateForUserAsync(request.UserId);
            }

            return jobApplicationDefaults!.ToDto();
        }
    }
}
