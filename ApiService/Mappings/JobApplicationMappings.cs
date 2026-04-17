using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Mappings;

internal static class JobApplicationMappings
{
    internal static JobApplicationModel ToModel(this JobApplicationDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name ?? string.Empty,
        Position = dto.Position ?? string.Empty,
        URL = dto.Url ?? string.Empty,
        AiTextHistory = dto.AiTextHistory?.Select(x => new JobApplicationAiTextModel
        {
            Id = x.Id,
            PromptKey = x.PromptKey ?? string.Empty,
            GeneratedText = x.GeneratedText ?? string.Empty,
            GeneratedAt = x.GeneratedAt,
        }).ToList() ?? [],
        CreationDate = dto.CreationDate,
        ModifiedDate = dto.ModifiedDate,
        Status = (JobApplicationModel.JobApplicationStatus)(int)dto.Status,
        JobDescription = dto.JobDescription ?? string.Empty,
        Location = dto.Location ?? string.Empty,
        ContactPerson = dto.ContactPerson ?? string.Empty,
        StartDate = DateOnly.FromDateTime(dto.StartDate),
        DurationInMonths = dto.DurationInMonths ?? string.Empty,
    };

    internal static CreateJobApplicationCommand ToCreateCommand(this JobApplicationModel model) => new()
    {
        Name = model.Name,
        Position = model.Position,
        Url = model.URL,
        Status = (JobApplicationStatus)(int)model.Status,
        JobDescription = model.JobDescription,
        Location = model.Location,
        ContactPerson = model.ContactPerson,
        StartDate = model.StartDate.ToDateTime(TimeOnly.MinValue),
        DurationInMonths = model.DurationInMonths,
    };

    internal static UpdateJobApplicationCommand ToUpdateCommand(this JobApplicationModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Position = model.Position,
        Url = model.URL,
        Status = (JobApplicationStatus)(int)model.Status,
        JobDescription = model.JobDescription,
        Location = model.Location,
        ContactPerson = model.ContactPerson,
        StartDate = model.StartDate.ToDateTime(TimeOnly.MinValue),
        DurationInMonths = model.DurationInMonths,
    };
}
