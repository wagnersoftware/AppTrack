

using FluentValidation.Results;

namespace AppTrack.Application.Exceptions;

public class BadRequestException : Exception
{
    public List<string> ErrorMessages { get; set; } = new List<string>();
    public BadRequestException(string message) : base(message)
    {

    }

    public BadRequestException(string message, ValidationResult validationResult) : base(message)
    {
        validationResult.Errors.ForEach(x => ErrorMessages.Add(x.ErrorMessage));
    }
}
