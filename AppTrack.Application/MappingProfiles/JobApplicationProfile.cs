using AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
using AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;
using AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplications;
using AppTrack.Domain;
using AutoMapper;

namespace AppTrack.Application.MappingProfiles;
public class JobApplicationProfile: Profile
{
    public JobApplicationProfile()
    {
        CreateMap<JobApplicationDto, JobApplication>().ReverseMap();
        CreateMap<CreateJobApplicationCommand, JobApplication>();
        CreateMap<UpdateJobApplicationCommand, JobApplication>();
    }
}

