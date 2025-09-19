using AppTrack.BlazorUI.Contracts;
using AppTrack.BlazorUI.Models.JobApplications;
using AppTrack.BlazorUI.Services.Base;
using AutoMapper;
using Blazored.LocalStorage;

namespace AppTrack.BlazorUI.Services
{
    public class JobApplicationService : BaseHttpService, IJobApplicationService
    {
        private readonly IMapper _mapper;

        public JobApplicationService(IClient client, IMapper mapper, ILocalStorageService localStorageService) : base(client, localStorageService)
        {
            this._mapper = mapper;
        }

        public async Task<Response<Guid>> CreateJobApplication(JobApplicationVM jobApplication)
        {
            try
            {
                await AddBearerTokenAsync();
                var createJobApplicationCommand = _mapper.Map<CreateJobApplicationCommand>(jobApplication);
                await _client.JobApplicationsPOSTAsync(createJobApplicationCommand);
                return new Response<Guid>() { Success = true };
            }
            catch (ApiException e)
            {
                return ConvertApiException(e);
            }

        }

        public async Task<Response<Guid>> DeleteJobApplication(int id)
        {
            try
            {
                await AddBearerTokenAsync();
                await _client.JobApplicationsDELETEAsync(id);
                return new Response<Guid>() { Success = true };
            }
            catch (ApiException e)
            {
                return ConvertApiException(e);
            }
        }

        public async Task<JobApplicationVM> GetJobApplicationById(int id)
        {
            await AddBearerTokenAsync();
            var jobApplication = await _client.JobApplicationsGETAsync(id);
            return _mapper.Map<JobApplicationVM>(jobApplication);
        }

        public async Task<List<JobApplicationVM>> GetJobApplicationsAsync()
        {
            await AddBearerTokenAsync();
            var jobApplicationDtos = await _client.JobApplicationsAllAsync();
            return _mapper.Map<List<JobApplicationVM>>(jobApplicationDtos);
        }

        public async Task<Response<Guid>> UpdateJobApplication(int id, JobApplicationVM jobApplication)
        {
            try
            {
                var updateJobApplicationCommand = _mapper.Map<UpdateJobApplicationCommand>(jobApplication);
                await AddBearerTokenAsync();
                await _client.JobApplicationsPUTAsync(id.ToString(), updateJobApplicationCommand);
                return new Response<Guid>() { Success = true };
            }
            catch (ApiException e)
            {
                return ConvertApiException(e);
            }
        }
    }
}
