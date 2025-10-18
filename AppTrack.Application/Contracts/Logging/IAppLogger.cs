namespace AppTrack.Application.Contracts.Logging;

#pragma warning disable S2326 // Unused type parameters should be removed
public interface IAppLogger<T>
#pragma warning restore S2326 // Unused type parameters should be removed
{
    void LogInformation(string message, params object[] args);

    void LogWarning(string message, params object[] args);

    void LogError(Exception ex, string message, params object[] args);
}
