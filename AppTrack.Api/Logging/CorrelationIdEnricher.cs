using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace AppTrack.Api.Logging;

public class CorrelationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = Activity.Current?.TraceId.ToString() ?? "none";
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("CorrelationId", correlationId));
    }
}
