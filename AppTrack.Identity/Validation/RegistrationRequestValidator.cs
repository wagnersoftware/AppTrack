using AppTrack.Application.Models.Identity;
using FluentValidation;

namespace AppTrack.Identity.Validation
{
    public class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
    {
        public RegistrationRequestValidator()
        {
            RuleFor(x => x.UserName)
                .Cascade(CascadeMode.Stop) // all validations will stop on first failure
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
                .MaximumLength(20).WithMessage("Username must not exceed 20 characters")
                .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Username can only contain letters, numbers, hyphens and underscores");

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop) // all validations will stop on first failure
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
                .Matches(@"[A-Z]+").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]+").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"\d+").WithMessage("Password must contain at least one number")
                .Matches(@"[\!\?\*\.\@\$\#]+").WithMessage("Password must contain at least one special character");
        }
    }
}
