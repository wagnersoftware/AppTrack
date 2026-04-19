using Serilog.Core;
using Serilog.Events;

namespace AppTrack.Api.Logging;

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

        var properties = value.GetType().GetProperties();
        if (!properties.Any(p => SensitiveFields.Contains(p.Name)))
            return false;

        var logProperties = properties.Select(p =>
        {
            LogEventPropertyValue propValue = SensitiveFields.Contains(p.Name)
                ? new ScalarValue("[REDACTED]")
                : propertyValueFactory.CreatePropertyValue(p.GetValue(value), true);
            return new LogEventProperty(p.Name, propValue);
        });

        result = new StructureValue(logProperties);
        return true;
    }
}
