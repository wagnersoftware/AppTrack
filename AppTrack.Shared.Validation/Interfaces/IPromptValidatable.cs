namespace AppTrack.Shared.Validation.Interfaces;

public interface IPromptValidatable
{
    string Name { get; }
    string PromptTemplate { get; }
}
