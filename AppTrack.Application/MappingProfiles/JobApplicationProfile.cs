using AppTrack.Application.Features.JobApplication.Queries.GetAllJobApplications;
using AppTrack.Domain;
using AutoMapper;

namespace AppTrack.Application.MappingProfiles;
public class JobApplicationProfile: Profile
{
    public JobApplicationProfile()
    {
        CreateMap<JobApplicationDto, JobApplication>().ReverseMap();
    }
}

