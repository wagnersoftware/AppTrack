using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;
using AutoMapper;

namespace AppTrack.Frontend.ApiService.MappingProfiles;

public class MappingConfiguration : Profile
{
    public MappingConfiguration()
    {
        CreateMap<DateOnly, DateTime>().ConvertUsing(d => d.ToDateTime(TimeOnly.MinValue));
        CreateMap<DateTime, DateOnly>().ConvertUsing(dt => DateOnly.FromDateTime(dt));

        //Job Application
        CreateMap<JobApplicationDto, JobApplicationModel>();

        CreateMap<JobApplicationModel, CreateJobApplicationCommand>();

        CreateMap<UpdateJobApplicationCommand, JobApplicationModel>().ReverseMap();

        //Job Application Defaults
        CreateMap<JobApplicationDefaultsDto, JobApplicationDefaultsModel>().ReverseMap();
        CreateMap<JobApplicationDefaultsModel, UpdateJobApplicationDefaultsCommand>();

        //Ai Settings
        CreateMap<AiSettingsDto, AiSettingsModel>().ReverseMap();
        CreateMap<AiSettingsModel, UpdateAiSettingsCommand>();

        //Authentication
        CreateMap<LoginModel, AuthRequest>();
        CreateMap<RegistrationModel, RegistrationRequest>();

        //PromptParameter
        CreateMap<PromptParameterDto, PromptParameterModel>().ReverseMap();
    }
}
