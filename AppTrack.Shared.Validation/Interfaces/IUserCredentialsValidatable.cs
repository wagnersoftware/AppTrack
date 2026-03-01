namespace AppTrack.Shared.Validation.Interfaces;

public interface IUserCredentialsValidatable
{
    string UserName { get; }
    string Password { get; }
}
