using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Domain;

namespace AppTrack.Application.Mappings;

internal static class ChatModelMappings
{
    internal static ChatModelDto ToDto(this ChatModel entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        ApiModelName = entity.ApiModelName,
    };
}
