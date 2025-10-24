using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class UserNameValidationAttribute : ValidationAttribute
    {
        public int MaxLength { get; set; } = 256;

        public int MinLength { get; set; } = 3;

        // Default Identity-Zeichen
        public string AllowedCharacters { get; set; } =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

        public override bool IsValid(object? value)
        {
            if (value == null)
                return false;

            var userName = value as string;
            if (string.IsNullOrWhiteSpace(userName))
            {
                ErrorMessage = "The UserName field is required.";
                return false;
            }

            if (userName.Length < MinLength)
            {
                ErrorMessage = $"The UserName must be at least {MinLength} characters long.";
                return false;
            }

            if (userName.Length > MaxLength)
            {
                ErrorMessage = $"The UserName must be at most {MaxLength} characters long.";
                return false;
            }

            if (userName.Any(c => !AllowedCharacters.Contains(c)))
            {
                ErrorMessage = $"The UserName contains invalid characters. " +
                               $"Allowed are: {AllowedCharacters}";
                return false;
            }

            return true;
        }
    }

}
