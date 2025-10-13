namespace AppTrack.WpfUi.CredentialManagement;

public interface ICredentialManager
{
    void SaveCredentials(string userName, string password);
    (string? userName, string? password)? LoadCredentials();
    void DeleteCredentials();
}

