using AppTrack.Frontend.Models.Base;
using FluentValidation;

namespace AppTrack.Frontend.Models.ModelValidator;

public class ModelValidator<T>(IValidator<T> validator) : IModelValidator<T> where T : ModelBase
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public IReadOnlyDictionary<string, List<string>> Errors => _errors;

    public bool Validate(T instance)
    {
        _errors.Clear();

        var result = validator.Validate(instance);

        foreach (var failure in result.Errors)
        {
            if (!_errors.ContainsKey(failure.PropertyName))
                _errors[failure.PropertyName] = new List<string>();

            _errors[failure.PropertyName].Add(failure.ErrorMessage);
        }

        return result.IsValid;
    }

    public void ResetErrors(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return;

        _errors.Remove(propertyName);
    }
}
