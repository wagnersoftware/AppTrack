namespace AppTrack.Shared.Validation.Interfaces;

public interface IAiSettingsValidatable
{
    string ApiKey { get; }
    IEnumerable<IPromptParameterValidatable> PromptParameters { get; }
}
