using AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;
using AppTrack.Application.Features.FreelancerProfile.Dto;

namespace AppTrack.Application.Mappings;

internal static class FreelancerProfileMappings
{
    internal static Domain.FreelancerProfile ToNewDomain(this UpsertFreelancerProfileCommand command) => new()
    {
        UserId = command.UserId,
        FirstName = command.FirstName,
        LastName = command.LastName,
        HourlyRate = command.HourlyRate,
        DailyRate = command.DailyRate,
        AvailableFrom = command.AvailableFrom,
        WorkMode = command.WorkMode,
        Skills = command.Skills,
        Language = command.Language,
    };

    internal static void ApplyTo(this UpsertFreelancerProfileCommand command, Domain.FreelancerProfile entity)
    {
        entity.UserId = command.UserId;
        entity.FirstName = command.FirstName;
        entity.LastName = command.LastName;
        entity.HourlyRate = command.HourlyRate;
        entity.DailyRate = command.DailyRate;
        entity.AvailableFrom = command.AvailableFrom;
        entity.WorkMode = command.WorkMode;
        entity.Skills = command.Skills;
        entity.Language = command.Language;
    }

    internal static FreelancerProfileDto ToDto(this Domain.FreelancerProfile entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        HourlyRate = entity.HourlyRate,
        DailyRate = entity.DailyRate,
        AvailableFrom = entity.AvailableFrom,
        WorkMode = entity.WorkMode,
        Skills = entity.Skills,
        Language = entity.Language,
        CvBlobPath = entity.CvBlobPath,
        CvFileName = entity.CvFileName,
        CvText = entity.CvText,
        CreationDate = entity.CreationDate ?? default,
        ModifiedDate = entity.ModifiedDate ?? default,
    };
}
