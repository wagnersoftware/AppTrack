namespace AppTrack.Frontend.Models.ModelValidator;

public interface IModelValidator<T> where T : ModelBase
{
    IReadOnlyDictionary<string, List<string>> Errors { get; }
    bool Validate(T instance);
    void ResetErrors(string propertyName);
}
