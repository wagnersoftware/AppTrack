using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models.ModelValidator;

public class ModelValidator<T> : IModelValidator<T> where T : ModelBase
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public IReadOnlyDictionary<string, List<string>> Errors => _errors;

    public bool Validate(T instance)
    {
        _errors.Clear();

        var context = new ValidationContext(instance);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, context, results, true);

        foreach (var result in results)
        {
            foreach (var memberName in result.MemberNames)
            {
                if (!_errors.ContainsKey(memberName))
                    _errors[memberName] = new List<string>();

                _errors[memberName].Add(result.ErrorMessage ?? "");
            }
        }

        return !_errors.Any();
    }

    public void ResetErrors(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        _errors.Remove(propertyName);
    }
}
