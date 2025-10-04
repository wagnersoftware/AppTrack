using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class PasswordValidationAttribute : ValidationAttribute
{
    public int RequiredLength { get; set; } = 6;
    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;

    public override bool IsValid(object? value)
    {
        if (value == null)
            return false;

        var password = value as string;
        if (string.IsNullOrEmpty(password))
            return false;

        if (password.Length < RequiredLength)
        {
            ErrorMessage = $"Password must be at least {RequiredLength} characters long.";
            return false;
        }
        if (RequireDigit && !password.Any(char.IsDigit))
        {
            ErrorMessage = "Password must contain at least one digit.";
            return false;
        }
        if (RequireLowercase && !password.Any(char.IsLower))
        {
            ErrorMessage = "Password must contain at least one lowercase letter.";
            return false;
        }
        if (RequireUppercase && !password.Any(char.IsUpper))
        {
            ErrorMessage = "Password must contain at least one uppercase letter.";
            return false;
        }
        if (RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
        {
            ErrorMessage = "Password must contain at least one non-alphanumeric character.";
            return false;
        }

        return true;
    }
}
