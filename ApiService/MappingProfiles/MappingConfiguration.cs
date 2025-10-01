using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;
using AutoMapper;

namespace AppTrack.Frontend.ApiService.MappingProfiles;

public class MappingConfiguration: Profile
{
    public MappingConfiguration()
    {
        //Job Application
        CreateMap<JobApplicationDto, JobApplicationModel>().ReverseMap();
        CreateMap<CreateJobApplicationCommand, JobApplicationModel>().ReverseMap();
        CreateMap<UpdateJobApplicationCommand, JobApplicationModel>().ReverseMap();

        //Job Application Defaults
        CreateMap<JobApplicationDefaultsDto, JobApplicationDefaultsModel>().ReverseMap();
        CreateMap<JobApplicationDefaultsModel, UpdateJobApplicationDefaultsCommand>();

        //Ai Settings
        CreateMap<AiSettingsDto, AiSettingsModel>().ReverseMap();
        CreateMap<AiSettingsModel, UpdateAiSettingsCommand>();

        //Authentication
        CreateMap<LoginModel, AuthRequest>();
        CreateMap<RegisterModel, RegistrationRequest>();
    }
}
