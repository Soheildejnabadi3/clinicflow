namespace ClinicFlow.Api.Exceptions;

public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string error)
        : this(new List<string> { error }) { }
}