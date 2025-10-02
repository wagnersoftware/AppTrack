namespace AppTrack.WpfUi.CredentialManagement;

public interface ICredentialManager
{
    void SaveCredentials(string username, string password);
    (string username, string password)? LoadCredentials();
    void DeleteCredentials();
}

