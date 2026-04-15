namespace AppTrack.Domain;

/// <summary>
/// Centralised constants for the built-in prompt parameter keys that are
/// automatically synced from the freelancer profile into <c>AiSettings</c>.
/// </summary>
public static class BuiltInParameterKeys
{
    /// <summary>Reserved prefix that identifies a built-in parameter key.</summary>
    public const string Prefix = "builtIn_";

    public const string FirstName     = "builtIn_FirstName";
    public const string LastName      = "builtIn_LastName";
    public const string HourlyRate    = "builtIn_HourlyRate";
    public const string DailyRate     = "builtIn_DailyRate";
    public const string AvailableFrom = "builtIn_AvailableFrom";
    public const string WorkMode      = "builtIn_WorkMode";
    public const string Skills        = "builtIn_Skills";
    public const string CvText        = "builtIn_CvText";
}
