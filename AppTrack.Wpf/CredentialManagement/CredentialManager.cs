using CredentialManagement;

namespace AppTrack.WpfUi.CredentialManagement;

public class CredentialManager : ICredentialManager
{
    private const string _target = "MyAppCredentials";

    public void SaveCredentials(string username, string password)
    {
        var cred = new Credential
        {
            Target = _target,
            Username = username,
            Password = password,
            PersistanceType = PersistanceType.LocalComputer
        };
        cred.Save();
    }

    public (string username, string password)? LoadCredentials()
    {
        var cred = new Credential { Target = _target };
        if (cred.Load())
        {
            return (cred.Username, cred.Password);
        }
        return null;
    }

    public void DeleteCredentials()
    {
        var cred = new Credential { Target = _target };
        cred.Delete();
    }
}

