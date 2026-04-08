using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Domain.Enums;
using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;

public class UpsertFreelancerProfileCommand : IRequest<FreelancerProfileDto>, IUserScopedRequest, IFreelancerProfileValidatable
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? DailyRate { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public RemotePreference? WorkMode { get; set; }
    public string? Skills { get; set; }
    public ApplicationLanguage? Language { get; set; }
}
