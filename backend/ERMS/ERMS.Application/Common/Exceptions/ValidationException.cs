namespace ERMS.Application.Common.Exceptions;

public class ValidationException : AppException
{
    public ValidationException(string message, IReadOnlyCollection<string>? errors = null)
        : base(message)
    {
        Errors = errors;
    }

    public IReadOnlyCollection<string>? Errors { get; }

    public override int StatusCode => 400;
}
