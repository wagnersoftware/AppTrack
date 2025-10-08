using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Domain;
using AutoMapper;

namespace AppTrack.Application.MappingProfiles;


public class PromptParameterProfile : Profile
{
    public PromptParameterProfile()
    {
        CreateMap<PromptParameter, PromptParameterDto>().ReverseMap();
    }
}
