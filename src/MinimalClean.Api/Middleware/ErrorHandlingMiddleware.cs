using System.Net;
using System.Text.Json;

namespace MinimalClean.Api.Middleware;

public class ErrorHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await WriteJson(context, new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await WriteJson(context, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await WriteJson(context, new { error = "Unexpected server error.", traceId = context.TraceIdentifier });
            // Log ex with your logger here
        }
    }

    private static Task WriteJson(HttpContext ctx, object payload)
    {
        ctx.Response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(payload);
        return ctx.Response.WriteAsync(json);
    }
}