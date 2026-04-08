using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Mappings;

internal static class FreelancerProfileMappings
{
    internal static UpsertFreelancerProfileCommand ToUpsertCommand(this FreelancerProfileModel model) => new()
    {
        FirstName = model.FirstName ?? string.Empty,
        LastName = model.LastName ?? string.Empty,
        HourlyRate = (double?)model.HourlyRate,
        DailyRate = (double?)model.DailyRate,
        AvailableFrom = model.AvailableFrom.HasValue
            ? model.AvailableFrom.Value.ToDateTime(TimeOnly.MinValue)
            : (DateTime?)null,
        WorkMode = model.WorkMode.HasValue
            ? (AppTrack.Frontend.ApiService.Base.RemotePreference)(int)model.WorkMode.Value
            : default,
        Skills = model.Skills,
        Language = model.Language.HasValue
            ? (AppTrack.Frontend.ApiService.Base.ApplicationLanguage)(int)model.Language.Value
            : default,
    };

    internal static FreelancerProfileModel ToModel(this FreelancerProfileDto dto)
    {
        RateKind? selectedRateType = null;
        if (dto.HourlyRate.HasValue)
        {
            selectedRateType = RateKind.Hourly;
        }
        else if (dto.DailyRate.HasValue)
        {
            selectedRateType = RateKind.Daily;
        }

        return new()
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            HourlyRate = (decimal?)dto.HourlyRate,
            DailyRate = (decimal?)dto.DailyRate,
            AvailableFrom = dto.AvailableFrom.HasValue
                ? DateOnly.FromDateTime(dto.AvailableFrom.Value)
                : (DateOnly?)null,
            WorkMode = (AppTrack.Frontend.Models.RemotePreference)(int)dto.WorkMode,
            Skills = dto.Skills,
            Language = (AppTrack.Frontend.Models.ApplicationLanguage)(int)dto.Language,
            SelectedRateType = selectedRateType,
            CreationDate = dto.CreationDate,
            ModifiedDate = dto.ModifiedDate,
            CvFileName = dto.CvFileName,
        };
    }
}
