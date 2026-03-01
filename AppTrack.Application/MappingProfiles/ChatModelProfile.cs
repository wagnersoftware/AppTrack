using AppTrack.Application.Features.AiSettings.Queries.GetChatModelsQuery;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Domain;
using AutoMapper;

namespace AppTrack.Application.MappingProfiles;

public class ChatModelProfile: Profile
{
    public ChatModelProfile()
    {
        CreateMap<GetChatModelsQuery, ChatModel>();
        CreateMap<ChatModel, ChatModelDto>();
    }
}
