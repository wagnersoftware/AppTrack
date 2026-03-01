namespace AppTrack.Shared.Validation.Interfaces;

public interface IRegistrationValidatable : IUserCredentialsValidatable
{
    string ConfirmPassword { get; }
}
