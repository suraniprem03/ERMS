namespace ERMS.Application.Common.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }

    public override int StatusCode => 401;
}
