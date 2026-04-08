using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.FreelancerProfile.Dto;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.UploadCv;

public class UploadCvCommand : IRequest<FreelancerProfileDto>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Set by the controller from IFormFile — not serialized.</summary>
    [JsonIgnore]
    public Stream FileStream { get; set; } = Stream.Null;

    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}
