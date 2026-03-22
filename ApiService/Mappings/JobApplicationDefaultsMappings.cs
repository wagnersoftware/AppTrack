using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Mappings;

internal static class JobApplicationDefaultsMappings
{
    internal static JobApplicationDefaultsModel ToModel(this JobApplicationDefaultsDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name ?? string.Empty,
        Position = dto.Position ?? string.Empty,
        Location = dto.Location ?? string.Empty,
    };

    internal static UpdateJobApplicationDefaultsCommand ToUpdateCommand(this JobApplicationDefaultsModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Position = model.Position,
        Location = model.Location,
    };
}
