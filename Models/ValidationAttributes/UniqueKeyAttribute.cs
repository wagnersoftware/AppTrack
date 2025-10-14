using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class UniqueKeyAttribute : ValidationAttribute
{
    private readonly string _collectionPropertyName;
    private readonly string _idPropertyName;

    /// <summary>
    /// Checks if the key property of an object is unique against a collection of objects.
    /// </summary>
    /// <param name="collectionPropertyName">The name of the collection</param>
    /// <param name="idPropertyName">Used to filter same instances.</param>
    public UniqueKeyAttribute(string collectionPropertyName, string idPropertyName = "TempId")
    {
        _collectionPropertyName = collectionPropertyName;
        _idPropertyName = idPropertyName;
        ErrorMessage = "The key must be unique.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }


        var instance = validationContext.ObjectInstance;
        var collectionProperty = instance.GetType().GetProperty(_collectionPropertyName);
        var idProp = instance.GetType().GetProperty(_idPropertyName);

        var collection = collectionProperty?.GetValue(instance) as IEnumerable;
        if (collection == null)
        {
            return ValidationResult.Success;
        }

        var instanceId = idProp?.GetValue(instance);

        bool duplicate = collection.Cast<object>().Any(item =>
        {
            var keyProp = item.GetType().GetProperty("Key");
            if (keyProp == null) return false;
            var keyVal = keyProp.GetValue(item);

            // do not compare same instances
            var otherId = item.GetType().GetProperty(_idPropertyName)?.GetValue(item);
            if (Equals(otherId, instanceId))
                return false;

            return string.Equals(
                keyVal?.ToString()?.Trim(),
                value?.ToString()?.Trim(),
                StringComparison.OrdinalIgnoreCase);
        });

        return duplicate
            ? new ValidationResult(ErrorMessage, new[] { validationContext.MemberName ?? string.Empty })
            : ValidationResult.Success;
    }
}

