namespace ERMS.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public T? Data { get; init; }

    public IReadOnlyCollection<string>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> Fail(string message, IReadOnlyCollection<string>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}
