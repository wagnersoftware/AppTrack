using AppTrack.Application.Models.Identity;
using AppTrack.Shared.Validation.Validators;

namespace AppTrack.Identity.Validation;

public class RegistrationRequestValidator : UserCredentialsBaseValidator<RegistrationRequest>
{
    public RegistrationRequestValidator() { }
}
