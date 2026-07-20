namespace ERMS.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message)
        : base(message)
    {
    }

    public abstract int StatusCode { get; }
}
