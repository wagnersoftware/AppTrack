namespace AppTrack.Shared.Validation.Interfaces;

public interface IJobApplicationValidatable
{
    string Name { get; }
    string Position { get; }
    string URL { get; }
    string JobDescription { get; }
    string Location { get; }
    string ContactPerson { get; }
    string DurationInMonths { get; }
}
