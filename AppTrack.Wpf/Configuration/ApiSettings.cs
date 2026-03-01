namespace AppTrack.WpfUi.Configuration;

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public int HealthCheckRetryCount { get; set; }
}
