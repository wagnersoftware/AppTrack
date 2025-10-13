using Meziantou.Framework.Win32;

namespace AppTrack.WpfUi.CredentialManagement;

/// <summary>
/// Provides persistence functions for the user credentials.
/// </summary>
public class CredentialManager : ICredentialManager
{
    private const string _applicationName = "AppTrack";

    public void SaveCredentials(string userName, string password)
    {
        Meziantou.Framework.Win32.CredentialManager.WriteCredential(
            applicationName: _applicationName,
            userName: userName,
            secret: password,
            comment: "Persisted credentials",
            persistence: CredentialPersistence.LocalMachine);
    }

    public (string? userName, string? password)? LoadCredentials()
    {
        var cred = Meziantou.Framework.Win32.CredentialManager.ReadCredential(_applicationName);
        if (cred != null)
        {
            return (cred.UserName, cred.Password);
        }

        return null;
    }

    public void DeleteCredentials()
    {
        Meziantou.Framework.Win32.CredentialManager.DeleteCredential(_applicationName);
    }
}

