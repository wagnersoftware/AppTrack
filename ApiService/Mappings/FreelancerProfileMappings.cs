using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Mappings;

internal static class FreelancerProfileMappings
{
    internal static UpsertFreelancerProfileCommand ToUpsertCommand(this FreelancerProfileModel model) => new()
    {
        FirstName = model.FirstName,
        LastName = model.LastName,
        HourlyRate = (double?)model.HourlyRate,
        DailyRate = (double?)model.DailyRate,
        AvailableFrom = model.AvailableFrom.HasValue
            ? model.AvailableFrom.Value.ToDateTime(TimeOnly.MinValue)
            : (DateTime?)null,
        WorkMode = model.WorkMode.HasValue
            ? (AppTrack.Frontend.ApiService.Base.RemotePreference)(int)model.WorkMode.Value
            : default,
        Skills = model.Skills,
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
            WorkMode = dto.WorkMode.HasValue
                ? (AppTrack.Frontend.Models.RemotePreference)(int)dto.WorkMode.Value
                : (AppTrack.Frontend.Models.RemotePreference?)null,
            Skills = dto.Skills,
            SelectedRateType = selectedRateType,
            CreationDate = dto.CreationDate,
            ModifiedDate = dto.ModifiedDate,
            CvFileName = dto.CvFileName,
            CvUploadDate = dto.CvUploadDate,
        };
    }
}
