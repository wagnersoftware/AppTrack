using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ProjectMonitoring.Dto;

namespace AppTrack.Application.Features.ProjectMonitoring.Queries.GetProjectMonitoringSettings;

public class GetProjectMonitoringSettingsQuery : IRequest<ProjectMonitoringSettingsDto>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
