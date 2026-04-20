using Serilog.Core;
using Serilog.Events;

namespace AppTrack.Api.Logging;

/// <summary>
/// Serilog destructuring policy that redacts personally identifiable information (PII)
/// from logged objects. When Serilog destructures an object whose type contains at least
/// one property listed in <see cref="SensitiveFields"/>, every sensitive property is
/// replaced with the literal string <c>[REDACTED]</c> while all other properties are
/// logged normally. Objects without any sensitive properties are left to Serilog's
/// default destructuring.
/// </summary>
public class PiiDestructuringPolicy : IDestructuringPolicy
{
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "ApiKey", "Token", "Email", "Name", "GivenName", "FamilyName"
    };

    public bool TryDestructure(
        object value,
        ILogEventPropertyValueFactory propertyValueFactory,
        out LogEventPropertyValue result)
    {
        result = null!;
        if (value is null) return false;

        var properties = value.GetType().GetProperties()
            .Where(p => p.GetIndexParameters().Length == 0)
            .ToArray();

        if (!properties.Any(p => SensitiveFields.Contains(p.Name)))
            return false;

        var logProperties = properties.Select(p =>
        {
            if (SensitiveFields.Contains(p.Name))
                return new LogEventProperty(p.Name, new ScalarValue("[REDACTED]"));

            try
            {
                var propValue = propertyValueFactory.CreatePropertyValue(p.GetValue(value), true);
                return new LogEventProperty(p.Name, propValue);
            }
            catch
            {
                return new LogEventProperty(p.Name, new ScalarValue("[ERROR_READING_PROPERTY]"));
            }
        });

        result = new StructureValue(logProperties);
        return true;
    }
}
