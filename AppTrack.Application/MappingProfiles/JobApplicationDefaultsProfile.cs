using AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using AppTrack.Domain;
using AutoMapper;

namespace AppTrack.Application.MappingProfiles;

public class JobApplicationDefaultsProfile : Profile
{
    public JobApplicationDefaultsProfile()
    {
        CreateMap<JobApplicationDefaultsDto, JobApplicationDefaults>().ReverseMap();
        CreateMap<UpdateJobApplicationDefaultsCommand, JobApplicationDefaults>();
    }
}
