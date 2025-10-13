using Meziantou.Framework.Win32;

namespace AppTrack.WpfUi.CredentialManagement;

/// <summary>
/// Provides persistence functions for the user credentials.
/// </summary>
public class CredentialManager : ICredentialManager
{
    public void SaveCredentials(string userName, string password)
    {
        var applicationName = GetApplicationName();

        Meziantou.Framework.Win32.CredentialManager.WriteCredential(
            applicationName: applicationName,
            userName: userName,
            secret: password,
            comment: "Persisted credentials",
            persistence: CredentialPersistence.LocalMachine);
    }

    public (string? userName, string? password)? LoadCredentials()
    {
        var applicationName = GetApplicationName();

        var cred = Meziantou.Framework.Win32.CredentialManager.ReadCredential(applicationName);
        if (cred != null)
        {
            return (cred.UserName, cred.Password);
        }

        return null;
    }

    public void DeleteCredentials()
    {
        var applicationName = GetApplicationName();
        Meziantou.Framework.Win32.CredentialManager.DeleteCredential(applicationName);
    }

    /// <summary>
    /// Application name is the current environment stage + current windows user name.
    /// The application name serves as key for the credential manager.
    /// </summary>
    /// <returns></returns>
    private static string GetApplicationName()
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        var userName = Environment.UserName;

        return $"AppTrack_{environment}_{userName}";
    }
}

