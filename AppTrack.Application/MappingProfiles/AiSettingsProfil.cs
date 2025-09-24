using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;
using AppTrack.Domain;
using AutoMapper;

namespace AppTrack.Application.MappingProfiles;

public class AiSettingsProfil : Profile
{
    public AiSettingsProfil()
    {
        CreateMap<AiSettingsDto, AiSettings>().ReverseMap();
        CreateMap<GetAiSettingsByUserIdQuery, AiSettings>();
        CreateMap<UpdateAiSettingsCommand, AiSettings>();
    }
}
