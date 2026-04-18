namespace AppTrack.Shared.Validation.Interfaces;

public interface IPromptValidatable
{
    string Key { get; }
    string PromptTemplate { get; }
}
