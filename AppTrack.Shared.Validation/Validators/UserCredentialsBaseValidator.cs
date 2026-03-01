using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class UserCredentialsBaseValidator<T> : AbstractValidator<T>
    where T : IUserCredentialsValidatable
{
    protected UserCredentialsBaseValidator()
    {
        RuleFor(x => x.UserName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
            .MaximumLength(20).WithMessage("Username must not exceed 20 characters")
            .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Username can only contain letters, numbers, hyphens and underscores");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters")
            .Matches(@"[A-Z]+").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]+").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d+").WithMessage("Password must contain at least one number")
            .Matches(@"[!?*.@$#&%+\-=_()]+").WithMessage("Password must contain at least one special character (!?*.@$#&%+-=_())");
    }
}
