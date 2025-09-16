using AppTrack.BlazorUI.Models.JobApplications;
using AppTrack.BlazorUI.Services.Base;
using AutoMapper;

namespace AppTrack.BlazorUI.MappingProfiles
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<JobApplicationDto, JobApplicationVM>().ReverseMap();
            CreateMap<CreateJobApplicationCommand, JobApplicationVM>().ReverseMap();
            CreateMap<UpdateJobApplicationCommand, JobApplicationVM>().ReverseMap();
        }
    }
}
