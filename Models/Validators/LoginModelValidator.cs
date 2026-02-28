using AppTrack.Shared.Validation.Validators;

namespace AppTrack.Frontend.Models.Validators;

public class LoginModelValidator : UserCredentialsBaseValidator<LoginModel>
{
    public LoginModelValidator() { }
}
