using System.Net;
using System.Text.Json;

namespace MinimalClean.Api.Middleware;

public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await WriteJson(context, new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogInformation(ex, "Resource not found: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await WriteJson(context, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await WriteJson(context, new
            {
                error = "Unexpected server error.",
                traceId = context.TraceIdentifier
            });
        }
    }

    private static Task WriteJson(HttpContext ctx, object payload)
    {
        ctx.Response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(payload);
        return ctx.Response.WriteAsync(json);
    }
}