using AppTrack.Domain.Enums;

namespace AppTrack.Application.Features.FreelancerProfile.Dto;

public class FreelancerProfileDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? DailyRate { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public RemotePreference? WorkMode { get; set; }
    public string? Skills { get; set; }
    public string? CvBlobPath { get; set; }
    public string? CvFileName { get; set; }
    public string? CvText { get; set; }
    public DateTime? CvUploadDate { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
