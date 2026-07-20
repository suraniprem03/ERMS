using System.Net;
using System.Text.Json;
using ERMS.Application.Common.Exceptions;
using ERMS.Application.Common.Models;
namespace ERMS.API.Common.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationException => (
                validationException.StatusCode,
                ApiResponse<object>.Fail(
                    validationException.Message,
                    validationException.Errors)),

            AppException appException => (
                appException.StatusCode,
                ApiResponse<object>.Fail(appException.Message)),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred."))
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
        }
        else
        {
            _logger.LogWarning(exception, "Request failed with status code {StatusCode}.", statusCode);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
