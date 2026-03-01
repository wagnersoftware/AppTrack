using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class RegistrationBaseValidator<T> : UserCredentialsBaseValidator<T>
    where T : IRegistrationValidatable
{
    protected RegistrationBaseValidator()
    {
        RuleFor(x => x.ConfirmPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}
