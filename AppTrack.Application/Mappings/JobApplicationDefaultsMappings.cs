using AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using AppTrack.Domain;

namespace AppTrack.Application.Mappings;

internal static class JobApplicationDefaultsMappings
{
    internal static void ApplyTo(this UpdateJobApplicationDefaultsCommand command, JobApplicationDefaults entity)
    {
        entity.Name = command.Name;
        entity.Position = command.Position;
        entity.Location = command.Location;
        entity.UserId = command.UserId;
    }

    internal static JobApplicationDefaultsDto ToDto(this JobApplicationDefaults entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Position = entity.Position,
        Location = entity.Location,
        UserId = entity.UserId,
    };
}
