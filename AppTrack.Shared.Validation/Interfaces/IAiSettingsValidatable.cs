namespace AppTrack.Shared.Validation.Interfaces;

public interface IAiSettingsValidatable
{
    IEnumerable<IPromptParameterValidatable> PromptParameter { get; }
}
