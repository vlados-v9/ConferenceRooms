using System.Net;

namespace ConferenceRooms.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(exception, "Unhandled exception");
            await WriteProblemAsync(context, exception);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid request"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            InvalidOperationException => (HttpStatusCode.Conflict, "Operation cannot be completed"),
            _ => (HttpStatusCode.InternalServerError, "Unexpected error")
        };

        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsJsonAsync(new
        {
            title,
            detail = exception.Message,
            status = context.Response.StatusCode
        });
    }
}
