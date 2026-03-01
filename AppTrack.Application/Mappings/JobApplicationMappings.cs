using AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
using AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain;

namespace AppTrack.Application.Mappings;

internal static class JobApplicationMappings
{
    internal static JobApplication ToDomain(this CreateJobApplicationCommand command) => new()
    {
        Name = command.Name,
        Position = command.Position,
        URL = command.URL,
        ApplicationText = command.ApplicationText,
        Status = command.Status,
        UserId = command.UserId,
        JobDescription = command.JobDescription,
        Location = command.Location,
        ContactPerson = command.ContactPerson,
        StartDate = command.StartDate,
        DurationInMonths = command.DurationInMonths,
    };

    internal static void ApplyTo(this UpdateJobApplicationCommand command, JobApplication entity)
    {
        entity.Name = command.Name;
        entity.Position = command.Position;
        entity.URL = command.URL;
        entity.ApplicationText = command.ApplicationText;
        entity.Status = command.Status;
        entity.UserId = command.UserId;
        entity.JobDescription = command.JobDescription;
        entity.Location = command.Location;
        entity.ContactPerson = command.ContactPerson;
        entity.StartDate = command.StartDate;
        entity.DurationInMonths = command.DurationInMonths;
    }

    internal static JobApplicationDto ToDto(this JobApplication entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        Name = entity.Name,
        Position = entity.Position,
        URL = entity.URL,
        ApplicationText = entity.ApplicationText,
        CreationDate = entity.CreationDate ?? default,
        ModifiedDate = entity.ModifiedDate ?? default,
        Status = entity.Status,
        JobDescription = entity.JobDescription,
        Location = entity.Location,
        ContactPerson = entity.ContactPerson,
        StartDate = entity.StartDate,
        DurationInMonths = entity.DurationInMonths,
    };
}
