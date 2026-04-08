namespace AppTrack.Shared.Validation.Interfaces;

public interface IFreelancerProfileValidatable
{
    string? FirstName { get; }
    string? LastName { get; }
    decimal? HourlyRate { get; }
    decimal? DailyRate { get; }
    string? Skills { get; }
}
