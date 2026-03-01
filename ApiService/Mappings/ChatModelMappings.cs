using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Mappings;

internal static class ChatModelMappings
{
    internal static ChatModel ToModel(this ChatModelDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name ?? string.Empty,
        Description = dto.Description ?? string.Empty,
        ApiModelName = dto.ApiModelName ?? string.Empty,
    };
}
