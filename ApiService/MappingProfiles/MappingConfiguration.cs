using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;
using AutoMapper;

namespace AppTrack.Frontend.ApiService.MappingProfiles;

public class MappingConfiguration: Profile
{
    public MappingConfiguration()
    {
        CreateMap<JobApplicationDto, JobApplicationModel>().ReverseMap();
        CreateMap<CreateJobApplicationCommand, JobApplicationModel>().ReverseMap();
        CreateMap<UpdateJobApplicationCommand, JobApplicationModel>().ReverseMap();
    }
}
