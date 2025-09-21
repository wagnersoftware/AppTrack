using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AutoMapper;

namespace AppTrack.Frontend.ApiService.Services;

public class JobApplicationService : BaseHttpService, IJobApplicationService
{
    private readonly IMapper _mapper;

    public JobApplicationService(IClient client, IMapper mapper, ITokenStorage tokenStorage) : base(client, tokenStorage)
    {
        this._mapper = mapper;
    }

    public async Task<Response<JobApplicationModel>> CreateJobApplication(JobApplicationModel jobApplicationModel)
    {
        try
        {
            await AddBearerTokenAsync();
            var createJobApplicationCommand = _mapper.Map<CreateJobApplicationCommand>(jobApplicationModel);
            var jobApplicationDto = await _client.JobApplicationsPOSTAsync(createJobApplicationCommand);
            return new Response<JobApplicationModel>() { Success = true, Data = _mapper.Map<JobApplicationModel>(jobApplicationDto)};
        }
        catch (ApiException e)
        {
            return ConvertApiException<JobApplicationModel>(e);
        }

    }

    public async Task<Response<JobApplicationModel>> DeleteJobApplication(int id)
    {
        try
        {
            await AddBearerTokenAsync();
            await _client.JobApplicationsDELETEAsync(id);
            return new Response<JobApplicationModel>() { Success = true };
        }
        catch (ApiException e)
        {
            return ConvertApiException<JobApplicationModel>(e);
        }
    }

    public async Task<JobApplicationModel> GetJobApplicationById(int id)
    {
        await AddBearerTokenAsync();
        var jobApplication = await _client.JobApplicationsGETAsync(id);
        return _mapper.Map<JobApplicationModel>(jobApplication);
    }

    public async Task<List<JobApplicationModel>> GetJobApplicationsAsync()
    {
        await AddBearerTokenAsync();
        var jobApplicationDtos = await _client.JobApplicationsAllAsync();
        return _mapper.Map<List<JobApplicationModel>>(jobApplicationDtos);
    }

    public async Task<Response<JobApplicationModel>> UpdateJobApplication(int id, JobApplicationModel jobApplicationModel)
    {
        try
        {
            var updateJobApplicationCommand = _mapper.Map<UpdateJobApplicationCommand>(jobApplicationModel);
            await AddBearerTokenAsync();
            await _client.JobApplicationsPUTAsync(id.ToString(), updateJobApplicationCommand);
            return new Response<JobApplicationModel>() { Success = true };
        }
        catch (ApiException e)
        {
            return ConvertApiException<JobApplicationModel>(e);
        }
    }
}
