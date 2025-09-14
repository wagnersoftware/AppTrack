using AppTrack.Application.Contracts.Logging;
using Microsoft.Extensions.Logging;

namespace AppTrack.Infrastructure.Logging;

public class LoggingAdapter<T> : IAppLogger<T>
{
    readonly ILogger<T> _logger;
    public LoggingAdapter(ILoggerFactory loggingFactory)
    {
        _logger = loggingFactory.CreateLogger<T>();
    }
    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }
}
